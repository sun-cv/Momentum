using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;

[InitializeOnLoad]
public static class AsepriteWatcher
{
    private static readonly string AsepritePath = @"C:\Sun\Dev\Tools\Aseprite\aseprite.exe";
    private static readonly string WatchFolder  = "Assets";

    private static FileSystemWatcher watcher;
    private static readonly Dictionary<string, DateTime> lastProcessed = new();

    static AsepriteWatcher()
    {
        StartWatcher();

        AssemblyReloadEvents.beforeAssemblyReload += () =>
        {
            watcher?.Dispose();
            watcher = null;
        };
    }

    private static void StartWatcher()
    {
        string fullPath = Path.GetFullPath(WatchFolder);

        if (!Directory.Exists(fullPath))
        {
            UnityEngine.Debug.LogWarning($"[AsepriteWatcher] Watch folder does not exist: {WatchFolder}");
            return;
        }

        watcher = new(fullPath, "*.aseprite")
        {
            IncludeSubdirectories   = true,
            NotifyFilter            = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents     = true
        };

        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileChanged;
        watcher.Renamed += OnFileRenamed;
    }

    private static void OnFileChanged(object sender, FileSystemEventArgs message)
    {
        if (!Debounce(message.FullPath)) 
            return;

        string path = message.FullPath;
        EditorApplication.delayCall += () => ProcessAseprite(path);
    }

    private static void OnFileRenamed(object sender, RenamedEventArgs message)
    {
        if (!Debounce(message.FullPath)) 
            return;

        string path = message.FullPath;
        EditorApplication.delayCall += () => ProcessAseprite(path);
    }

    private static bool Debounce(string path)
    {
        var now = DateTime.UtcNow;

        if (lastProcessed.TryGetValue(path, out var last) && (now - last).TotalSeconds < 2)
            return false;

        lastProcessed[path] = now;
        return true;
    }

    private static void ProcessAseprite(string sourceFullPath)
    {
        if (!File.Exists(sourceFullPath)) return;

        string sourceDir    = Path.GetDirectoryName(sourceFullPath);
        string filename     = Path.GetFileNameWithoutExtension(sourceFullPath);

        var parentInfo      = Directory.GetParent(sourceDir);

        if (parentInfo == null) 
            return;

        string spriteDir    = Path.Combine(parentInfo.FullName, "Sprite");

        if (!Directory.Exists(spriteDir))
            Directory.CreateDirectory(spriteDir);

        string pngPath      = Path.Combine(spriteDir, filename + ".png");
        string jsonPath     = Path.Combine(spriteDir, filename + ".json");
        string assetPath    = GetAssetPathRelativeToUnity(pngPath);

        var psi = new ProcessStartInfo
        {
            FileName               = AsepritePath,
            Arguments              = $"-b \"{sourceFullPath}\" --sheet \"{pngPath}\" --sheet-type rows --data \"{jsonPath}\" --format json-array",
            UseShellExecute        = false,
            CreateNoWindow         = true,
            RedirectStandardError  = true,
            RedirectStandardOutput = true
        };

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                using var process   = Process.Start(psi);
                string stderr       = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"[AsepriteWatcher] Aseprite error (code {process.ExitCode}): {stderr}");
                    return;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[AsepriteWatcher] Error running Aseprite CLI: " + ex.Message);
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(pngPath))
                {
                    UnityEngine.Debug.LogError($"[AsepriteWatcher] PNG not created: {pngPath}");
                    return;
                }

                // Read frame size from the exported JSON
                Vector2Int frameSize = ReadFrameSizeFromJson(jsonPath);

                if (frameSize == Vector2Int.zero)
                {
                    UnityEngine.Debug.LogError($"[AsepriteWatcher] Could not read frame size from JSON: {jsonPath}");
                    return;
                }

                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null)
                {
                    importer.textureType                = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit        = 16;
                    importer.filterMode                 = FilterMode.Point;
                    importer.mipmapEnabled              = false;
                    importer.alphaIsTransparency        = true;
                    importer.textureCompression         = TextureImporterCompression.Uncompressed;

                    importer.spriteImportMode           = SpriteImportMode.Multiple;

                    TextureImporterSettings settings    = new();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMode                 = (int)SpriteImportMode.Multiple;
                    settings.spriteMeshType             = SpriteMeshType.FullRect;
                    settings.spritePixelsPerUnit        = 16;
                    settings.spritePivot                = new Vector2(0.5f, 0.0f);
                    importer.SetTextureSettings(settings);

                    var factory = new SpriteDataProviderFactories();
                    factory.Init();
                    var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
                    dataProvider.InitSpriteEditorDataProvider();

                    GetPngDimensions(pngPath, out int texWidth, out int texHeight);

                    if (texWidth > 0 && texHeight > 0)
                    {
                        int cols        = texWidth  / frameSize.x;
                        int rows        = texHeight / frameSize.y;
                        int frameCount  = cols * rows;

                        var spriteRects = new SpriteRect[frameCount];
                        int index = 0;

                        for (int row = rows - 1; row >= 0; row--)
                        {
                            for (int col = 0; col < cols; col++)
                            {
                                spriteRects[index] = new SpriteRect
                                {
                                    name   = $"{filename}_{index}",
                                    rect   = new Rect(col * frameSize.x, row * frameSize.y, frameSize.x, frameSize.y),
                                    pivot  = new Vector2(0.5f, 0f),
                                    alignment = SpriteAlignment.Center
                                };
                                index++;
                            }
                        }

                        dataProvider.SetSpriteRects(spriteRects);
                        dataProvider.Apply();
                    }

                    importer.SaveAndReimport();
                }

                if (File.Exists(jsonPath))
                    File.Delete(jsonPath);

                AssetDatabase.Refresh();

                UnityEngine.Debug.Log($"[AsepriteWatcher] Exported {filename}.png ({frameSize.x}x{frameSize.y} frames) → {assetPath}");
            };
        });
    }

    private static Vector2Int ReadFrameSizeFromJson(string jsonPath)
    {
        if (!File.Exists(jsonPath)) return Vector2Int.zero;

        try
        {
            string json = File.ReadAllText(jsonPath);

            int frameIdx = json.IndexOf("\"frame\"");
            if (frameIdx < 0) return Vector2Int.zero;

            int w = ExtractJsonInt(json, "\"w\"", frameIdx);
            int h = ExtractJsonInt(json, "\"h\"", frameIdx);

            if (w > 0 && h > 0)
                return new Vector2Int(w, h);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[AsepriteWatcher] Failed to parse JSON: " + ex.Message);
        }

        return Vector2Int.zero;
    }

    private static int ExtractJsonInt(string json, string key, int startIndex)
    {
        int keyIdx = json.IndexOf(key, startIndex);
        if (keyIdx < 0) return 0;

        int colonIdx = json.IndexOf(':', keyIdx);
        if (colonIdx < 0) return 0;

        int numStart = colonIdx + 1;
        while (numStart < json.Length && (json[numStart] == ' ' || json[numStart] == '\t'))
            numStart++;

        int numEnd = numStart;
        while (numEnd < json.Length && char.IsDigit(json[numEnd]))
            numEnd++;

        if (numEnd == numStart) return 0;

        return int.Parse(json[numStart..numEnd]);
    }

    private static void GetPngDimensions(string pngPath, out int width, out int height)
    {
        width  = 0;
        height = 0;

        try
        {
            using var fs = new FileStream(pngPath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            fs.Seek(16, SeekOrigin.Begin);
            byte[] wBytes = br.ReadBytes(4);
            byte[] hBytes = br.ReadBytes(4);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(wBytes);
                Array.Reverse(hBytes);
            }

            width = BitConverter.ToInt32(wBytes, 0);
            height = BitConverter.ToInt32(hBytes, 0);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[AsepriteWatcher] Failed to read PNG dimensions: " + ex.Message);
        }
    }

    // ===============================================================================

    private static string GetAssetPathRelativeToUnity(string fullPath)
    {
        string projectPath = Path.GetFullPath(Application.dataPath + "/..");
        string relative    = fullPath[projectPath.Length..].Replace("\\", "/");
        return relative.TrimStart('/');
    }
}

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;

[InitializeOnLoad]
public static class AsepriteWatcher
{
    private static readonly string AsepritePath = @"C:\Dev\Tools\Aseprite\aseprite.exe";
    private static readonly string WatchFolder  = "Assets";

    // -----------------------------------

    private static FileSystemWatcher watcher;
    private static readonly Dictionary<string, DateTime> _lastProcessed = new Dictionary<string, DateTime>();

    // ===============================================================================

    static AsepriteWatcher()
    {
        StartWatcher();

        // Dispose watcher cleanly before domain reloads to avoid ghost watchers
        AssemblyReloadEvents.beforeAssemblyReload += () =>
        {
            watcher?.Dispose();
            watcher = null;
        };
    }

    // ===============================================================================

    private static void StartWatcher()
    {
        string fullPath = Path.GetFullPath(WatchFolder);

        if (!Directory.Exists(fullPath))
        {
            UnityEngine.Debug.LogWarning($"[AsepriteWatcher] Watch folder does not exist: {WatchFolder}");
            return;
        }

        watcher = new FileSystemWatcher(fullPath, "*.aseprite");

        watcher.IncludeSubdirectories   = true;
        watcher.NotifyFilter            = NotifyFilters.LastWrite | NotifyFilters.FileName;
        watcher.EnableRaisingEvents     = true;

        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileChanged;
        watcher.Renamed += OnFileRenamed;

        // UnityEngine.Debug.Log("[AsepriteWatcher] Watching folder for .aseprite changes: " + WatchFolder);
    }

    // ===============================================================================

    private static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!Debounce(e.FullPath)) return;
        string path = e.FullPath;
        EditorApplication.delayCall += () => ProcessAseprite(path);
    }

    private static void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (!Debounce(e.FullPath)) return;
        string path = e.FullPath;
        EditorApplication.delayCall += () => ProcessAseprite(path);
    }

    // Returns true if the event should be processed, false if it's a duplicate within the debounce window
    private static bool Debounce(string path)
    {
        var now = DateTime.UtcNow;
        if (_lastProcessed.TryGetValue(path, out var last) && (now - last).TotalSeconds < 2)
            return false;

        _lastProcessed[path] = now;
        return true;
    }

    // ===============================================================================

    private static void ProcessAseprite(string sourceFullPath)
    {
        if (!File.Exists(sourceFullPath)) return;

        string sourceDir = Path.GetDirectoryName(sourceFullPath);
        string filename  = Path.GetFileNameWithoutExtension(sourceFullPath);

        var parentInfo = Directory.GetParent(sourceDir);
        if (parentInfo == null) return;

        string spriteDir = Path.Combine(parentInfo.FullName, "Sprite");
        if (!Directory.Exists(spriteDir))
            Directory.CreateDirectory(spriteDir);

        string pngPath   = Path.Combine(spriteDir, filename + ".png");
        string assetPath = GetAssetPathRelativeToUnity(pngPath);

        var psi = new ProcessStartInfo
        {
            FileName               = AsepritePath,
            Arguments              = $"-b \"{sourceFullPath}\" --save-as \"{pngPath}\"",
            UseShellExecute        = false,
            CreateNoWindow         = true,
            RedirectStandardError  = true,
            RedirectStandardOutput = true
        };

        // Run Aseprite on a background thread so Unity's main thread stays alive
        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                using (var process = Process.Start(psi))
                {
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        UnityEngine.Debug.LogError($"[AsepriteWatcher] Aseprite error (code {process.ExitCode}): {stderr}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[AsepriteWatcher] Error running Aseprite CLI: " + ex.Message);
                return;
            }

            // Marshal back to main thread for all Unity API calls
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(pngPath))
                {
                    UnityEngine.Debug.LogError($"[AsepriteWatcher] PNG not created: {pngPath}");
                    return;
                }

                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType         = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32;
                    importer.filterMode          = FilterMode.Point;
                    importer.mipmapEnabled       = false;
                    importer.alphaIsTransparency = true;
                    importer.textureCompression  = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }

                AssetDatabase.Refresh();
                
                UnityEngine.Debug.Log($"[AsepriteWatcher] Exported {filename}.png → {assetPath}");
            };
        });
    }
    // ===============================================================================

    private static string GetAssetPathRelativeToUnity(string fullPath)
    {
        string projectPath  = Path.GetFullPath(Application.dataPath + "/..");
        string relative     = fullPath.Substring(projectPath.Length).Replace("\\", "/");
        return relative.TrimStart('/');
    }

}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;



public static class Assets
{

    private static readonly Dictionary<string, GameObject>           cache           = new();
    private static readonly Dictionary<string, AsyncOperationHandle> handles         = new();
    private static readonly Dictionary<string, HashSet<string>>      labelToPrefabs  = new();

    // ===============================================================================

    public static void Initialize()
    {
        LoadLabel("Core");
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public static GameObject Get(string name)
    {
        if (cache.TryGetValue(name, out var prefab))
            return prefab;

        Log.Debug($"Prefab '{name}' not found.");
        return null;
    }

    public static void LoadGroup(string label)
    {
        LoadLabel(label);
    }

    public static void UnloadGroup(string label)
    {
        if (!handles.TryGetValue(label, out var handle))
            return;

        if (labelToPrefabs.TryGetValue(label, out var prefabNames))
        {
            foreach (var name in prefabNames)
                cache.Remove(name);

            labelToPrefabs.Remove(label);
        }

        Addressables.Release(handle);
        handles.Remove(label);
    }

    // ===============================================================================

    private static void LoadLabel(string label)
    {
        if (handles.ContainsKey(label))
            return;

        var prefabNames = new HashSet<string>();

        var handle = Addressables.LoadAssetsAsync<GameObject>(label, prefab => { cache[prefab.name] = prefab; prefabNames.Add(prefab.name); });
        
        handle.WaitForCompletion();
        
        handles[label]          = handle;
        labelToPrefabs[label]   = prefabNames;
    }

    // ===============================================================================
    
    readonly static Logger Log = Logging.For(LogSystem.Assets);
}
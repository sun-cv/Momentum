using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Game.Common;
using Game.Diagnostic;


namespace Game.Content
{

    public partial class Assets
    {
        private readonly Registry registry;
        private readonly Loader loader;

        public Assets(Registry register)
        {
            registry    = register;
            loader      = new(registry);
        }

        public void Shutdown()
        {

        }

        public TAsset Get<TAsset>(string id) where TAsset : UnityEngine.Object
        {
            return registry.Get<TAsset>(id);
        
        }

        public Sheet Sheet(string id)
        {
            return registry.Get<Sheet>(id);
        }

        public Blueprint Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("[Assets] Blueprint: requested with no definition Id");

            var definition = registry.Get<Definition>(id);

            return new Blueprint
            {
                Definition  = definition,
                Prefab      = definition.Prefab is string key ? registry.Get<GameObject>(key) : throw new Exception($"[Assets] Blueprint: Definition ({definition.Id}) is missing prefab Id"),
            };
        }

        public Loader Load  => loader;

        static Assets() => Log<Assets>.Level(Diagnostic.Log.Level.Debug);
    }


    public partial class Assets
    {
        public sealed class Loader
        {
            private readonly AssetLoader<GameObject> prefabs;
            private readonly SheetLoader sheets;

            internal Loader(Registry registry)
            {
                prefabs = new AssetLoader<GameObject>(idOf: asset => asset.name, onLoaded: registry.Register<GameObject>, onUnloaded: registry.Deregister<GameObject>);
                sheets  = new SheetLoader(registry);
            }

            public AssetLoader<GameObject> Prefab => prefabs;
            public SheetLoader Sheet => sheets;
        }
    }

    public sealed class AssetLoader<TAsset>
    {
        private readonly Dictionary<HashSet<string>, (AsyncOperationHandle handle, List<string> ids)> handles = new(new LabelSetComparer());
        private readonly Dictionary<string, int> references = new();
        private readonly Func<TAsset, string> idOf;
        private readonly Action<string, TAsset> onLoaded;
        private readonly Action<string> onUnloaded;

        internal AssetLoader(Func<TAsset, string> idOf, Action<string, TAsset> onLoaded, Action<string> onUnloaded)
        {
            this.idOf       = idOf;
            this.onLoaded   = onLoaded;
            this.onUnloaded = onUnloaded;
        }

        public AsyncOperationHandle<IList<TAsset>> Load(HashSet<string> labels)
        {
            if (handles.TryGetValue(labels, out var existing))
                return existing.handle.Convert<IList<TAsset>>();

            var ids = new List<string>();



            var handle = Addressables.LoadAssetsAsync<TAsset>(labels, asset =>
            {
                var id = idOf(asset);
                ids.Add(id);

                references.TryGetValue(id, out var count);
                references[id] = count + 1;

                if (count == 0)
                    onLoaded(id, asset);
            }, Addressables.MergeMode.Intersection);

            handles[labels] = (handle, ids);

            return handle;
        }

        public void Unload(HashSet<string> labels)
        {
            if (!handles.TryGetValue(labels, out var entry))
                return;

            foreach (var id in entry.ids)
            {
                references[id]--;

                if (references[id] > 0)
                    continue;

                references.Remove(id);
                onUnloaded(id);
            }

            Addressables.Release(entry.handle);
            handles.Remove(labels);
        }
    }
}

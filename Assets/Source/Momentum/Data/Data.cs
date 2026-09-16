using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;
using Game.Common;
using Game.Diagnostic;



namespace Game.Content
{


    public partial class Data
    {
        private readonly Loader loader;
        private readonly Registry registry;
        private readonly Catalogue catalog;
        
        public Data()
        {
            registry    = new();
            loader      = new(registry);
            catalog     = new(registry);
        }

        public List<AsyncOperationHandle> Boot()
        {
            return catalog.Boot();
        }

        public void Shutdown()
        {

        }

        public Loader Load          => loader;
        public Catalogue Catalog    => catalog;

        static Data() => Log<Data>.Level(Diagnostic.Log.Level.Debug);
    }


    public partial class Data
    {
        public sealed class Registry
        {
            private readonly TypedStore<GameObject> prefabs      = new();
            private readonly TypedStore<IDefinition> definitions = new();

            public TypedStore<GameObject> Prefabs      => prefabs;
            public TypedStore<IDefinition> Definitions => definitions;
            
        }
    }

    public partial class Data
    {
        public sealed class Catalogue
        {
            private readonly Prefabs prefabs;
            private readonly Definitions definitions;

            public Prefabs Prefab           => prefabs;
            public Definitions Definition   => definitions;

            public Catalogue(Registry registry)
            {
                prefabs     = new(registry);
                definitions = new(registry);
            }

            public List<AsyncOperationHandle> Boot()
            {
                var handles = new List<AsyncOperationHandle>
                {
                    Definition.Load<Definition.Actor>(new HashSet<string> { "Definition", "Actor" }),
                    Definition.Load<Definition.Prop>(new HashSet<string> { "Definition", "Prop" }),
                    // Prefab.Load(new HashSet<string> { "Prefab", "Actor" })
                };

                return handles;
            }

            public sealed class Definitions 
            {
            
                private readonly Registry registry;
                private readonly Dictionary<HashSet<string>, AsyncOperationHandle> handles = new(new LabelSetComparer());

                public Definitions(Registry registry)
                {
                    this.registry = registry;
                }

                public AsyncOperationHandle<IList<TextAsset>> Load<TType>(HashSet<string> labels) where TType : IDefinition
                {
                    if (handles.TryGetValue(labels, out var existing))
                        return existing.Convert<IList<TextAsset>>();

                    var handle = Addressables.LoadAssetsAsync<TextAsset>(labels, asset =>
                            {
                                var definition = (TType)JsonConvert.DeserializeObject(asset.text, typeof(TType));
                                registry.Definitions.Register<TType>(definition.Id, definition);
                            }, Addressables.MergeMode.Intersection);

                    handles[labels] = handle;

                    return handle;
                }

                public void Unload(HashSet<string> labels)
                {
                    if (!handles.TryGetValue(labels, out var handle))
                        return;

                    Addressables.Release(handle);
                    handles.Remove(labels);
                }
            }

            public sealed class Prefabs
            {
            
                private readonly Registry registry;
                private readonly Dictionary<HashSet<string>, AsyncOperationHandle> handles = new(new LabelSetComparer());

                public Prefabs(Registry registry)
                {
                    this.registry = registry;
                }

                public AsyncOperationHandle<IList<GameObject>> Load(HashSet<string> labels)
                {
                    if (handles.TryGetValue(labels, out var existing))
                        return existing.Convert<IList<GameObject>>();

                    var handle = Addressables.LoadAssetsAsync<GameObject>(labels, asset =>
                            {
                            registry.Prefabs.Register<GameObject>(asset.name, asset);
                            }, Addressables.MergeMode.Intersection);

                    handles[labels] = handle;

                    return handle;
                }

                public void Unload(HashSet<string> labels)
                {
                    if (!handles.TryGetValue(labels, out var handle))
                        return;

                    Addressables.Release(handle);
                    handles.Remove(labels);
                }
            }
        }
    }

    public partial class Data
    {
        public sealed class Loader 
        {
            private readonly Registry registry;

            public Loader(Registry registry)
            {
                this.registry = registry;
            }

            public TDefinition Definition<TDefinition>(string id) where TDefinition : IDefinition
            {
                return registry.Definitions.Get<TDefinition>(id);
            }

            public GameObject Prefab(string id)
            {
                return registry.Prefabs.Get<GameObject>(id);
            }
        }
    }

    public class TypedStore<TBase>
    {   
        private readonly Dictionary<Type, Dictionary<string, TBase>> stores = new();

        public void Register<TValue>(string id, TValue value) where TValue : TBase
        {
            if (!stores.TryGetValue(typeof(TValue), out var dictionary))
            {
                dictionary = new();
                stores[typeof(TValue)] = dictionary;
            }

            dictionary[id] = value;
        }

        public void Deregister<TValue>(string id) where TValue : TBase
        {
            if (!stores.TryGetValue(typeof(TValue), out var _))
                return;

            stores[typeof(TValue)].Remove(id);
        }

        public TValue Get<TValue>(string id) where TValue : TBase
        {
            if (!stores.TryGetValue(typeof(TValue), out var dictionary))
                throw new KeyNotFoundException($"Registry<{typeof(TBase)}> missing <{typeof(TValue)}>key: {id}");

            return (TValue)dictionary[id];
        }
    }
}

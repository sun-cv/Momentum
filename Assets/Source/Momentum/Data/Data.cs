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
        private readonly Registry registry;
        private readonly Loader loader;

        public Data(Registry register)
        {
            registry    = register;
            loader      = new(registry);
        }

        public List<AsyncOperationHandle> Boot()
        {
            return loader.Boot();
        }

        public TData Get<TData>(string id) where TData : IRecord
        {
            return registry.Get<TData>(id);
        }
        public void Shutdown()
        {

        }

        static Data() => Log<Data>.Level(Diagnostic.Log.Level.Debug);
    }


    public partial class Data
    {
        public sealed class Loader
        {
            private readonly Registry registry;

            internal Loader(Registry registry)
            {
                this.registry = registry;
            }

            internal List<AsyncOperationHandle> Boot()
            {
                return new List<AsyncOperationHandle>
                {
                    Addressables.LoadAssetsAsync<TextAsset>(new HashSet<string> { "Definition" }, Parse, Addressables.MergeMode.Intersection)
                };
            }

            private void Parse(TextAsset asset)
            {
                var definition = JsonConvert.DeserializeObject<Definition>(asset.text);

                if (definition == null || string.IsNullOrEmpty(definition.Id))
                    throw new InvalidOperationException($"Definition file {asset.name} has no Id.");

                registry.Register(definition.Id, definition);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;
using Game.Common;
using UnityEngine.Identifiers;



namespace Game.Content
{


    public partial class Data
    {

        private readonly Loader loader;
        private readonly Finder lookup;
        private readonly Registry registry;
        
        public Data()
        {
            registry    = new();

            loader      = new(registry);
            lookup      = new(registry);

            loader.LoadAll();
        }

        public void Shutdown()
        {

        }

        public Finder Lookup => lookup;
    }

    public partial class Data
    {
        sealed class Loader 
        {
            private readonly Registry registry;

            public Loader(Registry registry)
            {
                this.registry = registry;
            }

            public void LoadAll()
            {
                foreach (var (type, path) in DataPath.Locations)
                    Load(type, path);
            }

            public void Load(Type type, string path)
            {
                foreach (var asset in Resources.LoadAll<TextAsset>(path))
                {
                    registry.Register(type, (IDefinition)JsonConvert.DeserializeObject(asset.text, type));
                }
            }
        }
    }

    public partial class Data
    {
        public sealed class Registry 
        {
            private readonly Dictionary<Type, Dictionary<string, IDefinition>> stores;

            public Registry()
            {
                stores = new();
            }

            public void Register<TDefinition>(TDefinition definition) where TDefinition : IDefinition
            {
                var store                   = new Dictionary<string, IDefinition>
                {
                    [definition.Id] = definition
                };

                stores[typeof(TDefinition)] = store;
            }

            public void Register(Type type, IDefinition definition)
            {
                var store                   = new Dictionary<string, IDefinition>
                {
                    [definition.Id] = definition
                };

                stores[type] = store;
            }

            public TDefinition Get<TDefinition>(string id) where TDefinition : IDefinition
            {
                if (!TryGet<TDefinition>(id, out var definition))
                    throw new KeyNotFoundException($"No {typeof(TDefinition).Name} definition registered with id '{id}'.");

                return definition;
            }

            public bool TryGet<TDefinition>(string id, out TDefinition definition) where TDefinition : IDefinition
            {
                definition = default;

                if (!stores.TryGetValue(typeof(TDefinition), out var store))
                    return false;

                if (!store.TryGetValue(id, out var value))
                    return false;

                definition = (TDefinition)value;
                return true;
            }
        }
    }

    public partial class Data
    {
        public sealed class Finder
        {
            private readonly Registry registry;

            public Finder(Registry registry)
            {
                this.registry = registry;
            }

            public TDefinition Definition<TDefinition>(string definition) where TDefinition : IDefinition
            {
                return registry.Get<TDefinition>(definition);
            }

            public Definition.Actor Actor(string definition)
            {
                return Definition<Definition.Actor>(definition);
            }
            public Definition.Prop Prop(string definition)
            {
                return Definition<Definition.Prop>(definition);
            }
            public Definition.Item Item(string definition)
            {
                return Definition<Definition.Item>(definition);
            }
        }
    }
}

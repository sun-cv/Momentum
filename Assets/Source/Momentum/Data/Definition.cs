using System;
using System.Collections.Generic;
using Game.Common;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;



namespace Game.Data
{

    public class Definition
    {
        private readonly Dictionary<Type, Dictionary<string, IDefinition>> stores;

        public Definition()
        {
            stores = new();
            LoadAll();
        }

        public void LoadAll()
        {
            foreach (var (type, path) in DefinitionRegistry.Locations)
                Load(type, path);
        }

        public void Load(Type type, string path)
        {
            var store = new Dictionary<string, IDefinition>();

            foreach (var asset in Resources.LoadAll<TextAsset>(path))
            {
                var definition = (IDefinition)JsonConvert.DeserializeObject(asset.text, type);
                store[definition.Id] = definition;
            }

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

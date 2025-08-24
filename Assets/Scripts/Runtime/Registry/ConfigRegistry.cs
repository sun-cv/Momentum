using System;
using System.Collections.Generic;
using UnityEngine;

namespace Momentum
{

    public interface IConfigRegistry
    {
        public T Resolve<T>();
    }

    public class ConfigRegistry : IRegistry, IConfigRegistry
    {
        private readonly Dictionary<Type, ScriptableObject> registry = new();

        public ConfigRegistry()
        {
            DiscoverConfigs();
        }

        public void DiscoverConfigs()
        {
            var configs = Resources.LoadAll<ScriptableObject>("ScriptableObjects");
            
            foreach (var config in configs)
            {
                Register(config);
            }
        }

    public void Register<T>(T instance)
    {
        if (instance is ScriptableObject so)
            registry[typeof(T)] = so;
        else
            throw new InvalidOperationException($"ConfigRegistry only accepts ScriptableObjects, not {typeof(T)}");
    }

    public T Resolve<T>()
    {
        if (registry.TryGetValue(typeof(T), out var config))
            return (T)(object)config;  // cast from ScriptableObject to T
        throw new Exception($"ConfigRegistry: No config registered for type {typeof(T).Name}");
    }
    }
}

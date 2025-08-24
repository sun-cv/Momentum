using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Momentum
{

    public interface IService {}

    public interface IServiceRegistry 
    {
        public T Resolve<T>();
    }

    public class ServiceRegistry : IRegistry, IServiceRegistry
    {
        private readonly Dictionary<Type, IService> registry = new();

        public void DiscoverSubRegistries()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(IService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
            {
                var instance = (IRegistry)Activator.CreateInstance(type);
                Register(instance);
            }
        }

        public void Register<T>(T instance)
        {
        if (instance is IService service)
            registry[typeof(T)] = service;
        else
            throw new InvalidOperationException($"ServiceRegistry only accepts IService, not {typeof(T)}");
        }
        public void Deregister<T>()
        {
            registry.Remove(typeof(T));
        }

        public T Resolve<T>()
        {
            if (registry.TryGetValue(typeof(T), out var config))
                return (T)(object)config;
            throw new Exception($"ServiceRegistry: No service registered for type {typeof(T).Name}");
        }

        public void Clear()
        {
            registry.Clear();
        }
    }
}

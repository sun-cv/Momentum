using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Momentum
{

    public interface IRegistry
    {
        T Resolve<T>();
        void Register<T>(T instance);
    }

    public interface IGlobalRegistry
    {
        public T Get<T>();
    }


    public class GlobalRegistry : IGlobalRegistry
    {
        private readonly Dictionary<Type, IRegistry> services = new();

        public void DiscoverSubRegistries()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(IRegistry).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
            {
                var instance = (IRegistry)Activator.CreateInstance(type);
                Register(instance);
            }
        }

        public void Register<T>(T instance) where T : IRegistry
        {
            var type = typeof(T);

            if (services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type {type} is already registered.");
            }
            services[type] = instance;
        }
        public void Deregister<T>()
        {
            services.Remove(typeof(T));
        }

        public T Get<T>()
        {
            if (!services.ContainsKey(typeof(T)))
            {
                throw new KeyNotFoundException($"Service of type {typeof(T)} is not registered.");
            }

            return (T)services[typeof(T)];
        }

        public bool TryGet<T>(out T instance)
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                instance = (T)service;
                return true;
            }

            instance = default;
            return false;
        }

        public void Clear()
        {
            services.Clear();
        }
    }
}
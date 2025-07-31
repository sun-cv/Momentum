using System;
using System.Collections.Generic;

namespace Momentum
{

    public static class Registry
    {
        private static readonly Dictionary<Type, object> services = new();

        public static void Register<T>(T instance)
        {
            var type = typeof(T);

            if (services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type {type} is already registered.");
            }

            services[type] = instance;
        }

        public static void Deregister<T>()
        {
            services.Remove(typeof(T));
        }

    public static T Get<T>()
    {
        if (!services.ContainsKey(typeof(T)))
        {
            throw new KeyNotFoundException($"Service of type {typeof(T)} is not registered.");
        }

        return (T)services[typeof(T)];
    }

        public static bool TryGet<T>(out T instance)
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                instance = (T)service;
                return true;
            }

            instance = default;
            return false;
        }

        public static void Clear()
        {
            services.Clear();
        }
    }
}
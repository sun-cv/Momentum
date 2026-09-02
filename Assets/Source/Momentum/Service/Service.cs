using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Game.Common;



namespace Game.Service
{
    
    public static class Service
    {

        public static void Initialize()
        {
            ServiceScanner.Register();
        }

        public static T Get<T>()
        { 
            return ServiceRegistry.Get<T>();
        }

        public static void Register<T>(T service)
        {
            ServiceRegistry.Register(service);
        }

        public static void Deregister<T>(T service)
        {
            ServiceRegistry.Deregister(service);
        }

        public static void Enable<t>()
        {
            // enable or disable service in dict<servicetype, bool>
        }

        public static void Disable<t>()
        {

        }

        public static class Roster
        {
            public static void Register(ServiceEntry entry) 
            { 
                ServiceRoster.Register(entry); 
            } 

            public static void Deregister(IService service) 
            { 
                ServiceRoster.Deregister(service); 
            }
        }
    }

    public static class ServiceScanner
    {
        public static void Register()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract) 
                    continue;

                if (type.GetCustomAttribute<ServiceAttribute>() == null) 
                    continue;

                var constructor = type.GetConstructor(Type.EmptyTypes);

                if (constructor == null)
                    throw new InvalidOperationException($"[Service] class {type.Name} has no public empty constructor.");

                var service = Activator.CreateInstance(type);

                ServiceRegistry.Register(type, service);
            }
        }
    }

    internal static class ServiceRegistry
    {
        private static readonly Dictionary<Type, object> services = new();

        public static T Get<T>()
        {
            if (!services.ContainsKey(typeof(T)))
                throw new KeyNotFoundException($"Service of type {typeof(T)} is not registered.");

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

        public static void Register<T>(T service)
        {
            if (services.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"Type {typeof(T)} {typeof(T).Name} is already registered.");

            services[typeof(T)] = service;
        }

        public static void Register(Type type, object service)
        {
            if (services.ContainsKey(type))
                throw new InvalidOperationException($"Type {type.Name} is already registered.");

            services[type] = service;
        }

        public static void Deregister<T>(T service)
        {
            services.Remove(typeof(T));
        }

        public static void Deregister(Type type)
        {
            if (!services.ContainsKey(type))
                throw new InvalidOperationException($"Type {type.Name} is not registered.");

            services.Remove(type);
        }

        public static void Dispose()
        {
            foreach (var (_, service) in services)
            {
                if (service is IDisposable instance)
                    instance.Dispose();
            }
        }


        public static void Clear()
        {
            services.Clear();
        }

        public static List<object> List                 => services.Values.ToList();
        public static Dictionary<Type, object> Services => services;
    }

    internal static class ServiceRoster
    {
        private static readonly Dictionary<TickRate, List<ServiceEntry>> services;

        static ServiceRoster()
        {
            services = new()
            {
                { TickRate.Base, new() },
                { TickRate.Half, new() },
                { TickRate.Step, new() },
                { TickRate.Util, new() },
                { TickRate.Late, new() }
            };
        }

        public static void Register(ServiceEntry entry)
        {
            List<ServiceEntry> list = entry.Service switch
            {
                IRateBase => services[TickRate.Base],
                IRateHalf => services[TickRate.Half],
                IRateStep => services[TickRate.Step],
                IRateUtil => services[TickRate.Util],
                IRateLate => services[TickRate.Late],
                _ => throw new ArgumentException($"{entry.Service.GetType()} does not implement a known IRate interface.")
            };

            list.Add(entry);
            list.Sort();
        }

        public static void Deregister(IService service)
        {
            foreach (var entries in services.Values)
            {
                entries.RemoveAll(entry => entry.Service == service);
            }
        }

        public static IReadOnlyDictionary<TickRate, List<ServiceEntry>> Services => services;
    }




}

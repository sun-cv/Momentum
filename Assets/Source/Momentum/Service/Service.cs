using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Game.Common;
using Game.Common.Events;
using Game.Diagnostic;



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
    }

    public static class ServiceScanner
    {
        public static void Register()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
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

                    bool ticked = service is IRateBase or IRateHalf or IRateStep or IRateUtil or IRateLate;

                    if (!ticked)
                        continue;

                    UnityEngine.Debug.Log(ticked);
                    Event.Send<RegisterService>(new(new((IService)service, ResolveSchedule(type))));
                }
            }
        }

        private static ServiceSchedule ResolveSchedule(Type serviceType)
        {
            var config = typeof(Game.Data.Config.Service).GetNestedType(serviceType.Name, BindingFlags.Public);

            if (config == null)
                throw new InvalidOperationException($"[Service] class {serviceType.Name} implements a tick-rate interface but has no matching Config.Service.{serviceType.Name} entry.");

            var phase    = config.GetField("Phase",    BindingFlags.Public | BindingFlags.Static);
            var priority = config.GetField("Priority", BindingFlags.Public | BindingFlags.Static);

            if (phase == null || priority == null)
                throw new InvalidOperationException($"Config.Service.{serviceType.Name} is missing Phase or Priority.");

            return new ServiceSchedule()
            {
                Phase    = (TickPhase)phase.GetValue(null),
                Priority = (int)priority.GetValue(null)
            };
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
}

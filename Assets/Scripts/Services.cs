using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;





[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ServiceAttribute : Attribute {  }


public static class Services
{
    readonly static Logger Log = Logging.For(LogSystem.Services);

    public static void Start()
    {
        Setup.AutoRegister();
    }

    public static void Initialize()
    {
        foreach (var service in Registry.RegisteredServicesList)
        {
            if (service is IInitialize instance)
                instance.Initialize();
        }
    }

    public static void Bind()
    {
        foreach (var service in Registry.RegisteredServicesList)
        {
            if (service is IBind instance)
                instance.Bind();
        }
    }

    public static void Dispose()
    {
        foreach (var service in Registry.RegisteredServicesList)
        {
            if (service is IDisposable instance)
                instance.Dispose();
        }
    }

    public static T Get<T>()
    { 
        return Registry.Get<T>();
    }

    public static void Register<T>(T service)
    {
        Registry.RegisterService(service);
    }

    public static void Deregister<T>(T service)
    {
        Registry.DeregisterService(service);
    }


    private static class Setup
    {

        public static void AutoRegister()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract) continue;
                if (type.GetCustomAttribute<ServiceAttribute>() == null) continue;
                
                var constructor = type.GetConstructor(Type.EmptyTypes);
                
                if (constructor == null)
                    throw new InvalidOperationException($"[Service] class {type.Name} has no public empty constructor.");
                
                var service = Activator.CreateInstance(type);
                
                Registry.RegisterService(type, service);
                
                if (service is IService tickable)
                    Registry.RegisterLanes(tickable);
            }
        }
    }


    public static class Lane
    {
        public static void Register(IService service) 
        { 
            Registry.RegisterLanes(service); 
        } 

        public static void Deregister(IService service) 
        { 
            Registry.DeregisterLanes(service); 
        }

        public static void Tick()
        {
            foreach(var service in Registry.TickServices)
                service.Tick();
        }

        public static void Loop()
        {        
            foreach(var service in Registry.LoopServices)
                service.Loop();
        }

        public static void Step()
        {        
            foreach(var service in Registry.StepServices)
                service.Step();
        }

        public static void Util()
        {        
            ServiceProcesses();
    
            foreach(var service in Registry.UtilServices)
                service.Util();
        }

        public static void Late()
        {
            foreach(var service in Registry.LateServices)
                service.Late();  
        }

        private static void ServiceProcesses()
        {
            Registry.ProcessPending();
        }

    }


    private static class Registry
    {
        private static readonly List<IService> pendingRegistrations     = new();
        private static readonly List<IService> pendingDeregistrations   = new();

        private static readonly Dictionary<Type, object> services       = new();

        private static readonly List<IServiceTick> tickServices         = new();
        private static readonly List<IServiceLoop> loopServices         = new();
        private static readonly List<IServiceStep> stepServices         = new();
        private static readonly List<IServiceUtil> utilServices         = new();
        private static readonly List<IServiceLate> lateServices         = new();


        public static void RegisterService<T>(T service)
        {
            if (services.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"Type {typeof(T)} {typeof(T).Name} is already registered.");

            services[typeof(T)] = service;
        }

        public static void RegisterService(Type type, object service)
        {
            if (services.ContainsKey(type))
                throw new InvalidOperationException($"Type {type.Name} is already registered.");

            services[type] = service;
        }

        public static void DeregisterService<T>(T service)
        {
            services.Remove(typeof(T));
        }

        public static void DeregisterService(Type type)
        {
            if (!services.ContainsKey(type))
                throw new InvalidOperationException($"Type {type.Name} is not registered.");

            services.Remove(type);
        }

        public static void RegisterLanes(IService service)
        {
            pendingRegistrations.Add(service);
        }

        public static void DeregisterLanes(IService service)
        {
            pendingDeregistrations.Add(service);
        }

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

        public static void Clear()
        {
            services.Clear();
        }

        public static void ProcessPending()
        {
            if (pendingRegistrations.Count == 0 && pendingDeregistrations.Count == 0)
                return;

            foreach (var service in pendingDeregistrations)
            {
                if (service is IServiceTick ServiceTick) tickServices.Remove(ServiceTick);
                if (service is IServiceLoop ServiceLoop) loopServices.Remove(ServiceLoop);
                if (service is IServiceStep ServiceStep) stepServices.Remove(ServiceStep);
                if (service is IServiceUtil ServiceUtil) utilServices.Remove(ServiceUtil);
            }

            pendingDeregistrations.Clear();

            foreach (var service in pendingRegistrations)
            {
                if (service is IServiceTick tickService)
                {
                    tickServices.Add(tickService);
                    tickServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                }

                if (service is IServiceLoop loopService)
                {
                    loopServices.Add(loopService);
                    loopServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                }

                if (service is IServiceStep stepService)
                {
                    stepServices.Add(stepService);
                    stepServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                }

                if (service is IServiceUtil utilService)
                {
                    utilServices.Add(utilService);
                    utilServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                }

                if (service is IServiceLate lateService)
                {
                    lateServices.Add(lateService);
                    lateServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                }

            }
            pendingRegistrations.Clear();
        }

        public static List<object> RegisteredServicesList           => services.Values.ToList();
        public static Dictionary<Type, object> RegisteredServices   => services;

        public static List<IServiceTick> TickServices => tickServices;
        public static List<IServiceLoop> LoopServices => loopServices;
        public static List<IServiceStep> StepServices => stepServices; 
        public static List<IServiceUtil> UtilServices => utilServices;
        public static List<IServiceLate> LateServices => lateServices;
    }
}

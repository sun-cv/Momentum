using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace Game.Common
{

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ServiceAttribute : Attribute {  }

    public interface IService : IDisposable   {};

    [Service]
    public abstract class RegisteredService : IService
    {
        public virtual void OnDispose() {} 

        public void Dispose()
        {
            //REWORK REQUIRED DISPOSE REGISTER AND CLEAR TICK
        }
    }
    

    public interface IRate              { }
    public interface IRateBase : IRate  { public void Tick(); };
    public interface IRateHalf : IRate  { public void Tick(); };
    public interface IRateStep : IRate  { public void Tick(); };
    public interface IRateUtil : IRate  { public void Tick(); };


    public enum TickRate 
    { 
        Base,
        Half,
        Step,
        Util,
        Late,
    }

    public enum TickPhase
    {
        System,
        Input,
        Logic,
        Physics,
        Resolve,
        Render
    }

    // needs to be moved 
    // public static class Lane
    // {
    //     public static void RegisterService(IService service) 
    //     { 
    //         ServiceRoster.RegisterServiceLanes(service); 
    //     } 
    //
    //     public static void Deregister(IService service) 
    //     { 
    //         ServiceRoster.DeregisterLanes(service); 
    //     }
    // }


    public static class Service
    {

        public static void Initialize()
        {
            ServiceScanner.Register();
            Process();
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

        public static void Process()
        {
            ServiceRegistry.Get<ServiceProcessor>().Tick();
        }


    }

    public static class ServiceHandler
    {
        public static void Initialize()
        {
            foreach (var service in ServiceRegistry.List)
            {
                if (service is IInitialize instance)
                    instance.Initialize();
            }
        }

        public static void Bind()
        {
            foreach (var service in ServiceRegistry.List)
            {
                if (service is IBind instance)
                    instance.Bind();
            }
        }        

        public static void Dispose()
        {
            foreach (var service in ServiceRegistry.List)
            {
                if (service is IDisposable instance)
                    instance.Dispose();
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

        public static void Clear()
        {
            services.Clear();
        }

        public static List<object> List                 => services.Values.ToList();
        public static Dictionary<Type, object> Services => services;
    }




    // internal static class ServiceRoster
    // {
    //     private static readonly List<IService> pendingServiceRegistrations  = new();
    //     private static readonly List<IService> pendingDeregistrations       = new();
    //
    //     private static readonly List<IServiceTick> tickServices             = new();
    //     private static readonly List<IServiceLoop> loopServices             = new();
    //     private static readonly List<IServiceUtil> utilServices             = new();
    //     private static readonly List<IServiceLate> lateServices             = new();
    //
    //     public static void RegisterServiceLanes(IService service)
    //     {
    //         pendingServiceRegistrations.Add(service);
    //     }
    //
    //     public static void DeregisterLanes(IService service)
    //     {
    //         pendingDeregistrations.Add(service);
    //     }
    //
    //     public static void ProcessPending()
    //     {
    //         if (pendingServiceRegistrations.Count == 0 && pendingDeregistrations.Count == 0 )
    //             return;
    //
    //         foreach (var service in pendingDeregistrations)
    //         {
    //             if (service is IServiceTick ServiceTick) tickServices.Remove(ServiceTick);
    //             if (service is IServiceLoop ServiceLoop) loopServices.Remove(ServiceLoop);
    //             if (service is IServiceUtil ServiceUtil) utilServices.Remove(ServiceUtil);
    //             if (service is IServiceLate ServiceLate) lateServices.Remove(ServiceLate);
    //         }
    //
    //         pendingDeregistrations.Clear();
    //
    //         // requires rework to sort on addition.
    //
    //         foreach (var service in pendingServiceRegistrations)
    //         {
    //             if (service is IServiceTick tickService)
    //             {
    //                 tickServices.Add(tickService);
    //             }
    //
    //             if (service is IServiceLoop loopService)
    //             {
    //                 loopServices.Add(loopService);
    //             }
    //
    //             if (service is IServiceUtil utilService)
    //             {
    //                 utilServices.Add(utilService);
    //             }
    //
    //             if (service is IServiceLate lateService)
    //             {
    //                 lateServices.Add(lateService);
    //             }
    //         }
    //
    //         pendingServiceRegistrations.Clear();
    //     }
    //
    //     public static List<IRateBase> TickServices => tickServices;
    //     public static List<IRateHalf> LoopServices => loopServices;
    //     public static List<IRateStep> UtilServices => utilServices;
    //     public static List<IRateUtil> LateServices => lateServices;
    // }
    //
    public class ServiceProcessor : RegisteredService
    {

        public void Tick()
        {
            Process();
        }

        private void Process()
        {
            // ServiceRoster.ProcessPending();
        }
    }

    internal readonly struct ServiceUpdatePriority : IComparable<ServiceUpdatePriority>
    {
        public TickPhase Phase          { get; }
        public int Priority             { get; }

        public ServiceUpdatePriority(TickPhase phase, int priority)
        {
            Phase    = phase;
            Priority = priority;
        }

        public int CompareTo(ServiceUpdatePriority other)
        {
            return Phase.CompareTo(other.Phase) != 0 ? Phase.CompareTo(other.Phase) : Priority.CompareTo(other.Priority);
        }
    }


}

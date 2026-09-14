using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Common;
using Game.Common.Events;



namespace Game.Core
{

    public class Scanner
    {
        internal Scanner() {}

        public void Register(object world, object data)
        {
            List<IInitialize> initialize = new();

            foreach (var type in DiscoverServiceTypes())
            {
                var service = CreateService(type, world, data);

                if (service is IInitialize init)
                    initialize.Add(init);

                RegisterTicked(service, type);
            }

            initialize.ForEach(service => service.Initialize());

            Event.Push<ServiceScanCompleted>();
        }

        private static IEnumerable<Type> DiscoverServiceTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.GetCustomAttribute<ServiceAttribute>() != null);
        }

        private object CreateService(Type type, object world, object data)
        {
            bool needsWorld = typeof(IWorld).IsAssignableFrom(type);
            bool needsData  = typeof(IData).IsAssignableFrom(type);

            ConstructorInfo constructor = (needsWorld, needsData) switch
            {
                (true,  true)  => type.GetConstructor(new[] { world.GetType(), data.GetType() }),
                (true,  false) => type.GetConstructor(new[] { world.GetType() }),
                (false, true)  => type.GetConstructor(new[] { data .GetType() }),
                (false, false) => type.GetConstructor(Type.EmptyTypes),
            };

            if (constructor == null)
                throw new InvalidOperationException($"[Service] class {type.Name} has no matching constructor for its declared dependencies.");

            object[] args = (needsWorld, needsData) switch
            {
                (true,  true)  => new object[] { world, data },
                (true,  false) => new object[] { world },
                (false, true)  => new object[] { data },
                (false, false) => Array.Empty<object>(),
            };

            return constructor.Invoke(args);
        }

        private void RegisterTicked(object service, Type type)
        {
            if (service is not IRate)
                throw new InvalidOperationException($"[Service] class {type.Name} has no IRate assigned");

            Event.Send<RegisterService>(new((IService)service, ResolveSchedule(type)));
        }

        private ServiceSchedule ResolveSchedule(Type serviceType)
        {
            var config  = typeof(Config.Service).GetNestedType(serviceType.Name, BindingFlags.Public);

            if (config == null)
                throw new InvalidOperationException($"[Service] class {serviceType.Name} implements a tick-rate interface but has no matching Config.Service.{serviceType.Name} entry.");

            var phase    = config.GetField("Phase",    BindingFlags.Public | BindingFlags.Static);
            var priority = config.GetField("Priority", BindingFlags.Public | BindingFlags.Static);

            if (phase  == null || priority == null)
                throw new InvalidOperationException($"Config.Service.{serviceType.Name} is missing Phase or Priority.");

            return new ServiceSchedule()
            {
                Phase    = (TickPhase)phase.GetValue(null),
                Priority = (int)priority.GetValue(null)
            };
        }
    }

}

using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Common;
using Game.Common.Events;



namespace Game.Core
{

    public class Scanner
    {
        internal Scanner()
        {
            Event.Push<ServiceScanCompleted>();
        }

        public void Register(object world, object data)
        {
            Assembly[] assemblies           = AppDomain.CurrentDomain.GetAssemblies();
            List<IInitialize> initialize    = new();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract) 
                        continue;

                    if (type.GetCustomAttribute<ServiceAttribute>() == null) 
                        continue;

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

                    var service = constructor.Invoke(args);

                    if (service is IInitialize init)
                        initialize.Add(init);

                    if (service is not IRate)
                        throw new InvalidOperationException($"[Service] class {type.Name} has no IRate assigned");

                    Event.Send<RegisterService>(new((IService)service, ResolveSchedule(type)));
                }
            }
            initialize.ForEach(service => service.Initialize());
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

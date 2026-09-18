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

        public void Register(object world, object data, object asset)
        {
            List<IInitialize> initialize = new();

            foreach (var type in DiscoverServiceTypes())
            {
                var service = CreateService(type, world, data, asset);

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

        private object CreateService(Type type, object world, object data, object asset)
        {
            bool needsWorld = typeof(IWorld).IsAssignableFrom(type);
            bool needsData  = typeof(IData).IsAssignableFrom(type);
            bool needsAsset = typeof(IAsset).IsAssignableFrom(type);

            ConstructorInfo constructor = (needsWorld, needsData, needsAsset) switch
            {
                (true,  true, true)     => type.GetConstructor(new[] { world.GetType(), data.GetType(), asset.GetType() }),
                (true,  true, false)    => type.GetConstructor(new[] { world.GetType(), data.GetType() }),
                (true,  false, true)    => type.GetConstructor(new[] { world.GetType(), asset.GetType() }),
                (false,  true, true)    => type.GetConstructor(new[] { data.GetType(), asset.GetType() }),
                (true,  false, false)   => type.GetConstructor(new[] { world.GetType() }),
                (false,  true, false)   => type.GetConstructor(new[] { data.GetType() }),
                (false,  false, true)   => type.GetConstructor(new[] { asset.GetType() }),
                (false, false, false)   => type.GetConstructor(Type.EmptyTypes),
            };

            if (constructor == null)
                throw new Exception($"[Service] class {type.Name} has no matching constructor for its declared dependencies.");

            object[] args = (needsWorld, needsData, needsAsset) switch
            {
                (true,  true, true)     => new object[] { world, data, asset },
                (true,  true, false)    => new object[] { world, data },
                (true,  false, true)    => new object[] { world, asset },
                (false,  true, true)    => new object[] { data, asset },
                (true,  false, false)   => new object[] { world },
                (false, true, false)    => new object[] { data },
                (false,  false, true)   => new object[] { asset },
                (false, false, false)   => Array.Empty<object>(),
            };

            return constructor.Invoke(args);
        }

        private void RegisterTicked(object service, Type type)
        {
            if (service is not IRate)
                throw new Exception($"[Service] class {type.Name} has no IRate assigned");

            Event.Send<RegisterService>(new((IService)service, ResolveSchedule(type)));
        }

        private ServiceSchedule ResolveSchedule(Type type)
        {
            var config  = typeof(Config.Service).GetNestedType(type.Name, BindingFlags.Public);

            if (config == null)
                throw new Exception($"[Service] class {type.Name} implements a tick-rate interface but has no matching Config.Service.{type.Name} entry.");

            var phase    = config.GetField("Phase",    BindingFlags.Public | BindingFlags.Static);
            var priority = config.GetField("Priority", BindingFlags.Public | BindingFlags.Static);

            if (phase  == null || priority == null)
                throw new Exception($"[Service] class {type.Name} is missing Phase or Priority.");

            return new ServiceSchedule()
            {
                Phase    = (TickPhase)phase.GetValue(null),
                Priority = (int)priority.GetValue(null)
            };
        }
    }

}

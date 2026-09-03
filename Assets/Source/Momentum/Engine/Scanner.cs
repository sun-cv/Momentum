using System;
using System.Reflection;
using Game.Common;
using Game.Common.Events;



namespace Game.Engine
{

    internal class Scanner
    {
        internal Scanner()
        {
            Register();
            Event.Push<ServiceScanCompleted>();
        }

        public void Register()
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

                    bool ticked = service is IRateBase or IRateHalf or IRateStep or IRateUtil or IRateLate;

                    if (!ticked)
                        continue;

                    Event.Send<RegisterService>(new(new((IService)service, ResolveSchedule(type))));
                }
            }
        }

        private ServiceSchedule ResolveSchedule(Type serviceType)
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

}

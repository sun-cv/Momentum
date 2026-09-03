using System;
using System.Collections.Generic;
using Game.Common;
using Game.Common.Events;
using Game.Diagnostic;


namespace Game.Engine
{
    internal class Scheduler
    {
        private readonly Engine.Execute execute;
        
        private readonly Dictionary<Type, ServiceEntry> registry        = new();
        private readonly Dictionary<TickRate, List<ServiceEntry>> lanes = new()
        {
            { TickRate.Base, new() },
            { TickRate.Half, new() },
            { TickRate.Step, new() },
            { TickRate.Util, new() },
            { TickRate.Late, new() },
        };

        private readonly List<ServiceEntry> services = new();

        public Scheduler(Execute execute)
        {
            this.execute = execute;

            this.execute.Lanes[TickRate.Base].OnFire += CollectDue;
            this.execute.Lanes[TickRate.Half].OnFire += CollectDue;
            this.execute.Lanes[TickRate.Step].OnFire += CollectDue;
            this.execute.Lanes[TickRate.Util].OnFire += CollectDue;
            this.execute.Lanes[TickRate.Late].OnFire += CollectDue;

            this.execute.OnTick += Tick;

            Event.Register<Scheduler, RegisterService>();
            Event.Register<ServiceScanCompleted>(Register);
        }

        public void Tick()
        {
            foreach (var entry in services)
            {
                switch (entry.Service)
                {
                    case IRateBase baseRate: baseRate.Tick(); break;
                    case IRateHalf halfRate: halfRate.Tick(); break;
                    case IRateStep stepRate: stepRate.Tick(); break;
                    case IRateUtil utilRate: utilRate.Tick(); break;
                    case IRateLate lateRate: lateRate.Tick(); break;
                }
            }
            services.Clear();
        }

        private void CollectDue(TickRate rate)
        {
            services.AddRange(lanes[rate]);
            services.Sort();
        }

        public void Register()
        {

            Log<Scheduler>.Debug("Register");

            var messages = Event.Read<Scheduler, RegisterService>();

            foreach (var message in messages)
            {
                var serviceEntry = message.ServiceEntry;

                List<ServiceEntry> list = serviceEntry.Service switch
                {
                    IRateBase => lanes[TickRate.Base],
                    IRateHalf => lanes[TickRate.Half],
                    IRateStep => lanes[TickRate.Step],
                    IRateUtil => lanes[TickRate.Util],
                    IRateLate => lanes[TickRate.Late],
                    _ => throw new ArgumentException($"{serviceEntry.Service.GetType()} does not implement a known IRate interface.")
                };

                list.Add(serviceEntry);
                list.Sort();

                registry[serviceEntry.Service.GetType()] = serviceEntry;
            }
        }

        public void Dispose()
        {
            //REWORK REQUIRED DISPOSE SERVICE ENTRIES;
        }

        static Scheduler() => Log<Scheduler>.Level(Diagnostic.Log.Level.Admin);                
    }
}



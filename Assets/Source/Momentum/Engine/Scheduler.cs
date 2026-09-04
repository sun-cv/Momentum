using System;
using System.Collections.Generic;
using Game.Common;
using Game.Common.Events;
using Game.Diagnostic;


namespace Game.Core
{
    internal class Scheduler
    {
        private readonly Execute execute;
        
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
                entry.Tick();
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
            var messages = Event.Read<Scheduler, RegisterService>();

            foreach (var message in messages)
            {
                var service  = message.Service;
                var schedule = message.Schedule;

                foreach (var (rate, iRate) in rates)                                 
                {                                                                    
                    if (!iRate.IsInstanceOfType(service)) 
                        continue;  

                    var tick = (Action)Delegate.CreateDelegate(typeof(Action), service, iRate.GetMethod("Tick"));                                   

                    lanes[rate].Add(new(service, schedule, tick));      
                }                                                                    
            }
        }

        public void Dispose()
        {
            //REWORK REQUIRED DISPOSE SERVICE ENTRIES;
        }

        static readonly (TickRate Rate, Type IRate)[] rates =                
        {                                                                    
            (TickRate.Base, typeof(IRateBase)), 
            (TickRate.Half, typeof(IRateHalf)),                                                  
            (TickRate.Step, typeof(IRateStep)), 
            (TickRate.Util, typeof(IRateUtil)),                                                  
            (TickRate.Late, typeof(IRateLate)),                              
        }; 

        static Scheduler() => Log<Scheduler>.Level(Diagnostic.Log.Level.Admin);          
    }
}



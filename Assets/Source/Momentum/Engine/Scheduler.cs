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
        
        private readonly Dictionary<Type, ServiceEntry> registry         = new();
        private readonly Dictionary<LaneEntry, List<ServiceEntry>> lanes = new();

        private readonly List<ServiceEntry> services = new();

        public Scheduler(Execute execute)
        {
            this.execute = execute;

            RegisterExecute();

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

        private void CollectDue(LaneEntry entry)
        {
            services.AddRange(lanes[entry]);
            services.Sort();
        }

        public void Register()
        {
            var messages = Event.Read<Scheduler, RegisterService>();

            foreach (var message in messages)
            {
                var service  = message.Service;
                var schedule = message.Schedule;

                foreach (var (entry, iRate) in rates)                                 
                {                                                                    
                    if (!iRate.IsInstanceOfType(service)) 
                        continue;  

                    var tick = (Action)Delegate.CreateDelegate(typeof(Action), service, iRate.GetMethod("Tick"));                                   

                    lanes[entry].Add(new() { Service = service, Schedule = schedule, Tick = tick });      
                }                                                                    
            }
        }

        private void RegisterExecute()
        {
            foreach (var (entry, lane) in execute.Lanes)
            {
                lane.OnFire += CollectDue;
                lanes[entry] = new(); 
            }
            
            execute.OnTick += Tick;
        }

        public void Dispose()
        {
            //REWORK REQUIRED DISPOSE SERVICE ENTRIES;
        }

        static readonly (LaneEntry entry, Type IRate)[] rates =                
        {                                                                    
            ( new() { Rate = TickRate.Base, Mode = TickMode.Real}, typeof(IRealBase)), 
            ( new() { Rate = TickRate.Half, Mode = TickMode.Real}, typeof(IRealHalf)), 
            ( new() { Rate = TickRate.Step, Mode = TickMode.Real}, typeof(IRealStep)), 
            ( new() { Rate = TickRate.Util, Mode = TickMode.Real}, typeof(IRealUtil)), 
            ( new() { Rate = TickRate.Late, Mode = TickMode.Real}, typeof(IRealLate)), 
            ( new() { Rate = TickRate.Base, Mode = TickMode.Game}, typeof(IGameBase)), 
            ( new() { Rate = TickRate.Half, Mode = TickMode.Game}, typeof(IGameHalf)), 
            ( new() { Rate = TickRate.Step, Mode = TickMode.Game}, typeof(IGameStep)), 
            ( new() { Rate = TickRate.Util, Mode = TickMode.Game}, typeof(IGameUtil)), 
        }; 

        static Scheduler() => Log<Scheduler>.Level(Diagnostic.Log.Level.Admin);          
    }
}



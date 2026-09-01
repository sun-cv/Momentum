using System;
using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;


namespace Game.Engine
{
    internal class Scheduler
    {
        private readonly Engine.Execute execute;
        
        private readonly Dictionary<TickRate, List<ServiceEntry>> lanes = new()
        {
            { TickRate.Base, new() },
            { TickRate.Half, new() },
            { TickRate.Step, new() },
            { TickRate.Util, new() },
            { TickRate.Late, new() },
        };

        public Scheduler(Execute execute)
        {
            this.execute = execute;

            this.execute.Lanes[TickRate.Base].OnFire += Fire;
            this.execute.Lanes[TickRate.Util].OnFire += Fire;
            this.execute.Lanes[TickRate.Late].OnFire += Fire;

            this.execute.OnTick += Tick;
        }

        public void Fire()
        {

        }

        public void Tick()
        {

        }

        public void Dispose()
        {

        }

        private void CollectDue()
        {
            foreach (var (tickrate, lane) in execute.Lanes)
            {
                if (lane.fired) return;
            }
        }

        static Scheduler() => Log<Scheduler>.Level(Diagnostic.Log.Level.Admin);                
    }

    readonly struct ScheduleEntry : IComparable<ScheduleEntry>
    {
        public readonly int Priority;

        public int CompareTo(ScheduleEntry entry) => Priority.CompareTo(entry.Priority);
    }
}



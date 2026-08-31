using System;
using Game.Common;
using Game.Diagnostic;



namespace Game.Engine
{
    class Scheduler
    {
        private readonly Engine.Tick tick;
        
        public Scheduler(Tick tick)
        {
            this.tick = tick;

            this.tick.Lanes[TickRate.Base].OnTick += Tick;
            this.tick.Lanes[TickRate.Half].OnTick += Half;
            this.tick.Lanes[TickRate.Step].OnTick += Step;
            this.tick.Lanes[TickRate.Util].OnTick += Util;
            this.tick.Lanes[TickRate.Late].OnTick += Late;
        }


        public void Tick()
        {

        }

        public void Half()
        {

        }

        public void Step()
        {

        }

        public void Util()
        {

        }

        public void Late()
        {

        }

        public void Dispose()
        {

        }

        static Scheduler() => Log<Scheduler>.Level(Diagnostic.Log.Level.Admin);                
    }

    readonly struct ScheduleEntry : IComparable<ScheduleEntry>
    {
        public readonly int Priority;

        public int CompareTo(ScheduleEntry entry) => Priority.CompareTo(entry.Priority);
    }
}



using System;
using System.Linq;
using System.Collections.Generic;
using Game.Data;
using Game.Common;
using Game.Diagnostic;


namespace Game.Engine
{

    internal class Lane
    {
        public TickRate rate;
        
        public int tick;
        public int count;

        public float herz;
        public float delta;
        public float accumulator;

        public bool fired;
        public bool scaled;
        public bool origin;
        public bool enabled;

        public Lane next; 

        public Action OnTick;
    }
     
    internal class Tick
    {
        private readonly Clock clock; 
        private readonly Dictionary<TickRate, Lane> lanes;
           
        public Action OnComplete;

        public Tick(Clock clock)
        {
            this.clock = clock;

            lanes = new()
            {
                { TickRate.Base, new Lane(){ rate = TickRate.Base, tick = Config.Engine.Tick.Base, delta = 1f/Config.Engine.Tick.Base, scaled = true,  origin = true  }},
                { TickRate.Half, new Lane(){ rate = TickRate.Half, tick = Config.Engine.Tick.Half, delta = 1f/Config.Engine.Tick.Half, scaled = true,  origin = false }},
                { TickRate.Step, new Lane(){ rate = TickRate.Step, tick = Config.Engine.Tick.Step, delta = 1f/Config.Engine.Tick.Step, scaled = true,  origin = false }},
                { TickRate.Util, new Lane(){ rate = TickRate.Util, tick = Config.Engine.Tick.Util, delta = 1f/Config.Engine.Tick.Util, scaled = false, origin = true  }},
                { TickRate.Late, new Lane(){ rate = TickRate.Late, tick = Config.Engine.Tick.Base, delta = 1f/Config.Engine.Tick.Base, scaled = false, origin = true  }}
            };

            AssignLanes();
        }

        public void Execute()
        {
            Drive(Lanes[TickRate.Base], clock.ScaledDelta); 
            Drive(Lanes[TickRate.Util], clock.Delta);

            MeasureHerz();
        }

        public void Late()
        {
            Drive(Lanes[TickRate.Late], clock.Delta); 
        }

        private void Drive(Lane lane, float delta)
        {
            if (lane == null ) return;

            lane.fired          = false;
            lane.accumulator   += delta;

            while (lane.accumulator >= lane.delta)
            {
                MeasureTick(lane);

                lane.accumulator -= lane.delta;

                lane.tick++;
                lane.count++;
                lane.fired  = true;

                lane.OnTick?.Invoke();

                Drive(lane.next, lane.delta);

                if (lane.origin) OnComplete?.Invoke();
            }
        }
        
        private void MeasureHerz()
        {
            Lanes.Values.ToList().ForEach(lane => lane.herz += clock.Delta);
        }

        private void MeasureTick(Lane lane)
        {
            if (lane.herz >=1f)
            {
                Log<Tick>.Debug($"{lane.rate}", () => lane.tick / lane.herz);

                lane.tick = 0;
                lane.herz = 0;
            }
        }

        private void AssignLanes()
        {
            lanes[TickRate.Base].next = Lanes[TickRate.Half];
            lanes[TickRate.Half].next = Lanes[TickRate.Step];
        }

        public IReadOnlyDictionary<TickRate, Lane> Lanes => lanes;

        static Tick() => Log<Tick>.Level(Diagnostic.Log.Level.Admin);                
    }
}

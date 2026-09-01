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

        public Action OnFire;
    }
     
    internal class Execute
    {
        private readonly Clock clock; 
        private readonly Dictionary<TickRate, Lane> lanes;
           
        public Action OnTick;

        public Execute(Clock clock)
        {
            this.clock = clock;

            lanes = new()
            {
                { TickRate.Base, new Lane(){ rate = TickRate.Base, tick = Config.Engine.Tick.Base, delta = 1f/Config.Engine.Tick.Base, scaled = true,  origin = true  }},
                { TickRate.Half, new Lane(){ rate = TickRate.Half, tick = Config.Engine.Tick.Half, delta = 1f/Config.Engine.Tick.Half, scaled = true,  origin = false }},
                { TickRate.Step, new Lane(){ rate = TickRate.Step, tick = Config.Engine.Tick.Step, delta = 1f/Config.Engine.Tick.Step, scaled = true,  origin = false }},
                { TickRate.Util, new Lane(){ rate = TickRate.Util, tick = Config.Engine.Tick.Util, delta = 1f/Config.Engine.Tick.Util, scaled = false, origin = true  }},
                { TickRate.Late, new Lane(){ rate = TickRate.Late, tick = Config.Engine.Tick.Late, delta = 1f/Config.Engine.Tick.Late, scaled = false, origin = true  }},
            };

            lanes[TickRate.Base].next = Lanes[TickRate.Half];
            lanes[TickRate.Half].next = Lanes[TickRate.Step];
        }

        public void Tick()
        {
            Drive(Lanes[TickRate.Base], clock.ScaledDelta); 
            Drive(Lanes[TickRate.Util], clock.Delta);

            MeasureHerz();
        }

        public void Late()
        {
            Drive(Lanes[TickRate.Late], clock.UnscaledDelta); 
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

                lane.fired = true;

                lane.OnFire?.Invoke();

                Drive(lane.next, lane.delta);

                if (lane.origin) OnTick?.Invoke();
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
                Log<Execute>.Debug($"{lane.rate}", () => lane.tick / lane.herz);

                lane.tick = 0;
                lane.herz = 0;
            }
        }

        public IReadOnlyDictionary<TickRate, Lane> Lanes => lanes;

        static Execute() => Log<Execute>.Level(Diagnostic.Log.Level.Admin);                
    }
}

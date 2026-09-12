using System;
using System.Linq;
using System.Collections.Generic;
using Game.Content;
using Game.Common;
using Game.Diagnostic;


namespace Game.Core
{

    internal class Lane
    {
        public LaneEntry entry; 

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

        public Action<LaneEntry> OnFire;
    }

    internal record LaneEntry
    {
        public TickRate Rate;
        public TickMode Mode;
    }
     
    internal class Execute
    {
        private readonly Clock clock; 
        private readonly Dictionary<LaneEntry, Lane> lanes;
           
        public Action OnTick;

    public Execute(Clock clock)
    {
        this.clock = clock;

        lanes = new()
        {
            { new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Game }, tick = Config.Engine.Tick.Base, delta = 1f/Config.Engine.Tick.Base, origin = true  }},
            { new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }, tick = Config.Engine.Tick.Half, delta = 1f/Config.Engine.Tick.Half, origin = false }},
            { new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }, tick = Config.Engine.Tick.Step, delta = 1f/Config.Engine.Tick.Step, origin = false }},
            { new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Game }, tick = Config.Engine.Tick.Util, delta = 1f/Config.Engine.Tick.Util, origin = false  }},

            { new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Real }, tick = Config.Engine.Tick.Base, delta = 1f/Config.Engine.Tick.Base, origin = true  }},
            { new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }, tick = Config.Engine.Tick.Half, delta = 1f/Config.Engine.Tick.Half, origin = false }},
            { new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }, tick = Config.Engine.Tick.Step, delta = 1f/Config.Engine.Tick.Step, origin = false }},
            { new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Real }, tick = Config.Engine.Tick.Util, delta = 1f/Config.Engine.Tick.Util, origin = false }},

            { new LaneEntry{ Rate = TickRate.Late, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Late, Mode = TickMode.Real }, tick = Config.Engine.Tick.Late, delta = 1f/Config.Engine.Tick.Late, origin = true  }},
        };

        ConnectLanes();
    }

        public void Tick()
        {
            Drive(Lanes[new() { Rate = TickRate.Base, Mode = TickMode.Real }], clock.RealDelta);
            Drive(Lanes[new() { Rate = TickRate.Base, Mode = TickMode.Game }], clock.GameDelta);

            MeasureHerz();
        }

        public void Late()
        {
            Drive(Lanes[new() { Rate = TickRate.Late, Mode = TickMode.Real }], clock.RealTime);
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

                lane.OnFire?.Invoke(lane.entry);

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
                Log<Execute>.Debug($"{lane.entry.Rate}", () => lane.tick / lane.herz);

                lane.tick = 0;
                lane.herz = 0;
            }
        }

        public void ConnectLanes()
        {
            lanes[new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Game }].next = lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }];
            lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }].next = lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }];
            lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }].next = lanes[new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Game }];

            lanes[new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Real }].next = lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }];
            lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }].next = lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }];
            lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }].next = lanes[new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Real }];
        }

        public IReadOnlyDictionary<LaneEntry, Lane> Lanes => lanes;

        static Execute() => Log<Execute>.Level(Diagnostic.Log.Level.Admin);                
    }
}

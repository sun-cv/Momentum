using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Diagnostic;


namespace Game.Core
{

    internal class Lane
    {
        public LaneEntry entry; 

        public int tick;
        public int count;

        public double delta;
        public double accumulator;

        public bool fired;
        public bool scaled;
        public bool origin;
        public bool enabled;

        public double herz;
        public double mark;

        public Lane parent;
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
                { new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Game }, tick = Config.Engine.Tick.Base, delta = 1d/Config.Engine.Tick.Base, origin = true  }},
                { new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }, tick = Config.Engine.Tick.Half, delta = 1d/Config.Engine.Tick.Half, origin = false }},
                { new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }, tick = Config.Engine.Tick.Step, delta = 1d/Config.Engine.Tick.Step, origin = false }},
                { new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Game }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Game }, tick = Config.Engine.Tick.Util, delta = 1d/Config.Engine.Tick.Util, origin = false }},

                { new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Real }, tick = Config.Engine.Tick.Base, delta = 1d/Config.Engine.Tick.Base, origin = true  }},
                { new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }, tick = Config.Engine.Tick.Half, delta = 1d/Config.Engine.Tick.Half, origin = false }},
                { new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }, tick = Config.Engine.Tick.Step, delta = 1d/Config.Engine.Tick.Step, origin = false }},
                { new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Real }, tick = Config.Engine.Tick.Util, delta = 1d/Config.Engine.Tick.Util, origin = false }},

                { new LaneEntry{ Rate = TickRate.Late, Mode = TickMode.Real }, new Lane(){ entry = new LaneEntry{ Rate = TickRate.Late, Mode = TickMode.Real }, tick = Config.Engine.Tick.Late, delta = 1d/Config.Engine.Tick.Late, origin = true  }},
            };

            ConnectLanes();
        }

        public void Tick()
        {
            MeasureHerz();

            Drive(Lanes[new() { Rate = TickRate.Base, Mode = TickMode.Real }], (double)clock.RealDelta);
            Drive(Lanes[new() { Rate = TickRate.Base, Mode = TickMode.Game }], (double)clock.GameDelta);

            MeasureAlpha();
        }

        public void Late()
        {
            Lanes[new() { Rate = TickRate.Late, Mode = TickMode.Real }].OnFire?.Invoke(new() { Rate = TickRate.Late, Mode = TickMode.Real }); OnTick?.Invoke();
        }

        private void Drive(Lane lane, double delta)
        {
            if (lane == null) return;

            lane.fired        = false;
            lane.accumulator += delta;

            while (lane.accumulator >= lane.delta)
            {
                lane.accumulator -= lane.delta;

                lane.tick++;
                lane.count++;
                lane.fired = true;

                MeasureTick(lane);

                lane.OnFire?.Invoke(lane.entry);

                Drive(lane.next, lane.delta);

                if (lane.origin) OnTick?.Invoke();
            }
        }

        private void MeasureHerz()
        {
            foreach (var lane in Lanes.Values) lane.herz += Time.unscaledDeltaTime;
        }

        private void MeasureAlpha()
        {
            Lane game = Lanes[new() { Rate = TickRate.Base, Mode = TickMode.Game }];

            Watch.Tick.Alpha = (float)(game.accumulator / game.delta);
        }

        private void MeasureTick(Lane lane)
        {
            double now     = lane.herz - Behind(lane);
            double elapsed = now - lane.mark;

            if (elapsed < 1d) return;

            int    ticks = lane.tick;
            double rate  = ticks / elapsed;

            Log<Execute>.Debug($"{lane.entry.Mode} : {lane.entry.Rate}", () => rate);

            lane.tick = 0;
            lane.mark = now;
        }

        private double Behind(Lane lane)
        {
            double behind = 0d;
            float  scale  = lane.entry.Mode == TickMode.Game ? clock.Scale : 1f;

            for (var current = lane; current != null; current = current.parent)
            {
                behind += current.accumulator / scale;
            }

            return behind;
        }

        public void ConnectLanes()
        {
            lanes[new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Game }].next   = lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }];
            lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }].next   = lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }];
            lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }].next   = lanes[new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Game }];

            lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }].parent = lanes[new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Game }];
            lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }].parent = lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Game }];
            lanes[new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Game }].parent = lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Game }];

            lanes[new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Real }].next   = lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }];
            lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }].next   = lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }];
            lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }].next   = lanes[new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Real }];

            lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }].parent = lanes[new LaneEntry{ Rate = TickRate.Base, Mode = TickMode.Real }];
            lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }].parent = lanes[new LaneEntry{ Rate = TickRate.Half, Mode = TickMode.Real }];
            lanes[new LaneEntry{ Rate = TickRate.Util, Mode = TickMode.Real }].parent = lanes[new LaneEntry{ Rate = TickRate.Step, Mode = TickMode.Real }];
        }

        public IReadOnlyDictionary<LaneEntry, Lane> Lanes => lanes;

        static Execute() => Log<Execute>.Level(Diagnostic.Log.Level.Admin);                
    }
}

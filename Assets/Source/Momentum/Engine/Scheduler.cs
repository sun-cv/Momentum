using Game.Diagnostic;
using UnityEngine;



namespace Game.Engine
{

    class Scheduler
    {
        private Engine.Clock clock;

        private float tick;
        private float loop;
        private float util;

        private float tickHerz;
        private float loopHerz;
        private float utilHerz;

        public void Initialize(Clock clock)
        {
            this.clock = clock;

            this.clock.OnTick += Tick;

            this.clock.OnTick += MeasureTick;
            this.clock.OnLoop += MeasureLoop;
            this.clock.OnUtil += MeasureUtil;

            this.clock.OnTick += MeasureHerz;

            this.clock.OnTick += MeasureTickHerz;
            this.clock.OnLoop += MeasureLoopHerz;
            this.clock.OnUtil += MeasureUtilHerz;

            this.clock.OnLate += Late;

            Log<Scheduler>.Level(Diagnostic.Log.Level.Admin);
        } 

        private void Tick()
        {
        }

        private void Late()
        {
        }

        private void MeasureHerz()
        {
            tickHerz += clock.Delta;
            loopHerz += clock.Delta;
            utilHerz += clock.Delta;
        }

        private void MeasureTick()
        {
            tick++;
        }
        
        private void MeasureLoop()
        {
            loop++;
        }

        private void MeasureUtil()
        {
            util++;
        }


        private void MeasureTickHerz()
        {
            if (tickHerz >=1f)
            {
                Log<Scheduler>.Debug("Tick Rate", () => tick / tickHerz);

                tick     = 0;
                tickHerz = 0;
            }
        }

        private void MeasureLoopHerz()
        {
            if (loopHerz >=1f)
            {
                Log<Scheduler>.Debug("loop Rate", () => loop / loopHerz);

                loop     = 0;
                loopHerz = 0;
            }
        }

        private void MeasureUtilHerz()
        {
            if (utilHerz >=1f)
            {
                Log<Scheduler>.Debug("Util Rate", () => util / utilHerz);

                util     = 0;
                utilHerz = 0;
            }
        }

        public void Dispose()
        {

        }
    }
}




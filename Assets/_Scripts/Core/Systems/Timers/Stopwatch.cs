using System.Collections.Generic;
using UnityEngine;

namespace Momentum
{
    public class Stopwatch : Timer
    {
        private readonly List<float> lapTimes = new();

        public IReadOnlyList<float> LapTimes => lapTimes;

        public Stopwatch() : base(0) {}

        public override void Tick()
        {
            if (IsRunning)
            {
                CurrentTime += Time.deltaTime;
            }
        }

        public override bool IsFinished => !IsRunning;
        public float LastLap => lapTimes.Count > 0 ? lapTimes[^1] : 0;
        
        public void Lap()
        {
            lapTimes.Add(CurrentTime);
        }

        public void ClearLaps()
        {
            lapTimes.Clear();
        }

        public override void Reset()
        {
            base.Reset();
            ClearLaps();
            CurrentTime = 0;
        }
    }
}

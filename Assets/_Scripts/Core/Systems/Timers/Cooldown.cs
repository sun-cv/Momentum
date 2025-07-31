using Momentum.Markers;
using UnityEngine;

namespace Momentum.Timers
{
    public class Cooldown : Timer
{
    public Cooldown(float value, StatusFlag flag) : base(value) 
    {
        OnTimerStart += ()=> flag.Set();
        OnTimerStop  += ()=> flag.Clear();
    }

        public override void Tick()
        {
            if (IsRunning && CurrentTime > 0)
            {
                CurrentTime -= Time.deltaTime;
            }

            if (IsRunning && CurrentTime <= 0)
            {
                Stop();
            }
        }

    public override bool IsFinished => CurrentTime <= 0;
}
}
using UnityEngine;

namespace Momentum.Timers
{
    public class Countdown : Timer
{
    public Countdown(float value) : base(value) {}

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
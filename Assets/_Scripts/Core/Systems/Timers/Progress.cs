
using UnityEngine;

namespace Momentum
{
    public class Progress : Timer
    {
        public Progress(float value) : base(value){}

        public override void Start()
        {
            CurrentTime = 0;

            if (!IsRunning)
            {
                IsRunning = true;
                TimerManager.RegisterTimer(this);
                OnTimerStart.Invoke();
            }
        }

        public override void Tick()
        {
            if (IsRunning)
            {
                CurrentTime += Time.deltaTime;
            }
            if (Percent >= 1)
            {
                Stop();
                Reset();
            }
        }

        public override void Reset() => CurrentTime = 0;
        public override void Reset(float newTime)
        {
            initialTime = newTime;
            Reset();
        }

        public override bool IsFinished => !IsRunning;
    }
}
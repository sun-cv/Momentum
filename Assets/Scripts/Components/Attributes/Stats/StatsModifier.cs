using System;

namespace Momentum
{
    public abstract class StatModifier : IDisposable
    {
        public bool  MarkedForRemoval { get; private set; }
        readonly     CountdownTimer timer;
        public event Action<StatModifier> OnDispose = delegate { };

        protected StatModifier(float duration)
        {
            if (duration <= 0) return;

            timer = new(duration);
            timer.OnTimerStop += () => MarkedForRemoval = true;
            timer.Start();
        }

        public abstract void Handle(object sender, Query query);
        
        public void Dispose()
        {
            OnDispose.Invoke(this);
        }


    }

}
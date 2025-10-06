using System;
using UnityEngine;



namespace Momentum
{
    [Serializable]
    public class ComboCounterRule : CooldownRule
    {
        [Header("Combo Count rules")]
        public int   ComboCount             = 3;
        public float ComboCountResetTime    = 1;

        int currentCount;


        public override IRuntimeCooldown CreateRuntime(CooldownContext context) => new Runtime(this, context);
        
        private sealed class Runtime : BaseCooldownRuntime<ComboCounterRule>
        {
            CountdownTimer timer;

            public Runtime(ComboCounterRule instance, CooldownContext context) : base(instance, context) {}

            public override void Enable()
            {            
                timer = new(rule.ComboCountResetTime);

                timer.OnTimerStart += () => Tracking();
                timer.OnTimerStop  += () => { rule.currentCount = 0; Expired(); };

                rule.currentCount ++;
                timer.Start();
            }

            public override void Trigger()
            {
                rule.currentCount++;
                timer.Reset(rule.ComboCountResetTime);
                timer.Start();
                Publish();
            }

        }
    }


}
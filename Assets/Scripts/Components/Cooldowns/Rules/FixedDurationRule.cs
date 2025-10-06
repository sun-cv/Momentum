using System;
using Unity.VisualScripting;
using UnityEngine;



namespace Momentum.Test
{
    [Serializable]
    public class FixedDurationRule : CooldownRule
    {    
        [Header("Rule specific")]
        [Range(0f, 60f)]public float duration   = 0;


        public override IRuntimeCooldown CreateRuntime(CooldownContext context) => new Runtime(this, context);

        private sealed class Runtime : BaseCooldownRuntime<FixedDurationRule>
        {
            CountdownTimer timer;

            public Runtime(FixedDurationRule rule, CooldownContext context) : base(rule, context) 
            {
                timer = new CountdownTimer(rule.duration);

                if (rule.automaticStartup) 
                    Enable();
            }


            public override void Enable()
            {
                timer.OnTimerStart += Blocking;
                timer.OnTimerStop  += Expired;

                timer.Start();
            }    
 
            public override void Trigger()
            {
                timer.Reset(rule.duration);
                timer.Start();
            }
        }

    }


}
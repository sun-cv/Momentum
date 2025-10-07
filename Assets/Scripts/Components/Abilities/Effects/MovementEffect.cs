// using System;
// using UnityEngine;


// namespace Momentum
// {
//     [EffectCategory("Movement")]
//     public class MovementEffect : Effect 
//     {
//         [Header("Effect specific")]
//         public bool test;

//         public override IRuntimeEffect CreateRuntime(Instance instance) => new Runtime(this, instance);

//         sealed class Runtime : IRuntimeEffect
//         {
//             readonly Instance ability;
//             readonly MovementEffect  effect;

//             public Runtime(MovementEffect effect, Instance instance) 
//             {
//                 this.ability = instance; this.effect = effect;
//             }

//             public void OnPhase(Phase phase) 
//             {

//                 if ((phase == Phase.CastStart) && effect.autoStart)
//                     Debug.Log("effect ability phase cast start");

//                 if ((phase == Phase.CastComplete) && effect.autoComplete)
//                     Debug.Log("effect ability phase cast complete");

//                 if ((phase == Phase.Cancel || phase == Phase.Interrupt) && effect.autoCancel)
//                     Debug.Log("effect ability phase cast cancel || interrupt.");            
//             }

//             public void OnTrigger(string label, string id)
//             {
//                 if (effect.labelRequired && effect.label != label)
//                     return;

//                 if (id == effect.triggerOn)     Debug.Log("effect trigger on");
//                 if (id == effect.triggerOff)    Debug.Log("effect trigger off");
//                 if (id == effect.triggerCancel) Debug.Log("effect trigger cancel");

//             }

//             public void Tick() {}

//         }
//     }
// }


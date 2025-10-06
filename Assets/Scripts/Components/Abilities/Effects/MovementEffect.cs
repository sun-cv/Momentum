// using System;
// using UnityEngine;


// namespace Momentum
// {
//     [EffectCategory("Movement")]
//     public class MovementEffect : AbilityEffect 
//     {
//         [Header("Effect specific")]
//         public bool test;

//         public override IRuntimeEffect CreateRuntime(AbilityInstance instance) => new Runtime(this, instance);

//         sealed class Runtime : IRuntimeEffect
//         {
//             readonly AbilityInstance ability;
//             readonly MovementEffect  effect;

//             public Runtime(MovementEffect effect, AbilityInstance instance) 
//             {
//                 this.ability = instance; this.effect = effect;
//             }

//             public void OnPhase(AbilityPhase phase) 
//             {

//                 if ((phase == AbilityPhase.CastStart) && effect.autoStart)
//                     Debug.Log("effect ability phase cast start");

//                 if ((phase == AbilityPhase.CastComplete) && effect.autoComplete)
//                     Debug.Log("effect ability phase cast complete");

//                 if ((phase == AbilityPhase.Cancel || phase == AbilityPhase.Interrupt) && effect.autoCancel)
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


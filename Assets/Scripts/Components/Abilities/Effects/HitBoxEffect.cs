// using System;
// using UnityEngine;




// namespace Momentum
// {
//     [EffectCategory("Combat/Detection")]
//     public class HitboxEffect : AbilityEffect 
//     {
//         [Header("Effect specific")]
//         public GameObject hitboxPrefab;

//         public override RuntimeEffect CreateRuntime(AbilityInstance instance) => new Runtime(this, instance);

//         sealed class Runtime : RuntimeEffect
//         {
//             readonly AbilityInstance ability;
//             readonly HitboxEffect   effect;
//             Transform   attach;
//             GameObject  instance;
//             Collider2D  hitbox;

//             public Runtime(HitboxEffect effect, AbilityInstance instance) 
//             {
//                 this.ability = instance; this.effect = effect;
//             }

//             public void OnPhase(AbilityPhase phase) 
//             {
//                 if ((phase == AbilityPhase.CastStart) && effect.autoStart)
//                     Spawn();
                
//                 if ((phase == AbilityPhase.CastComplete) && effect.autoComplete)
//                     Despawn();

//                 if ((phase == AbilityPhase.Cancel || phase == AbilityPhase.Interrupt) && effect.autoCancel)
//                     Despawn();
//             }

//             public void OnTrigger(string label, string id) 
//             {
//                 if (effect.labelRequired && effect.label != label)
//                     return;

//                 if (id == effect.triggerOn)     Spawn();
//                 if (id == effect.triggerOff)    Despawn();
//                 if (id == effect.triggerCancel) Despawn();
//             }

//             void Spawn() 
//             {
//                 if (instance != null) return;
//                 instance = UnityEngine.Object.Instantiate(effect.hitboxPrefab, attach.position, attach.rotation, attach);
//                 hitbox   = instance.GetComponent<Collider2D>();
//             }

//             void Despawn() 
//             {
//                 if (instance == null) return;

//                 UnityEngine.Object.Destroy(instance);

//                 instance  = null; 
//                 hitbox          = null;
//             }

//             void OnHit(GameObject victim) 
//             { 
//                 ability.EventBus.Publish("test");
//             }

//             public void Tick() {}

//         }
//     }
// }


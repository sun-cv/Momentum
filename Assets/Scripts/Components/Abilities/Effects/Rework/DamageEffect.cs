using System;
using UnityEngine;


namespace Momentum
{

    // [Serializable]
    // class DamageEffect : Effect 
    // {
    //     public int amount;

    //     public override void Execute(AbilityContext context)
    //     {
    //         // singleton - 
    //         // CombatSystem.Instance.ApplyDamage(
    //         //     new DamageInstance {
    //         //         Source = ctx.caster,
    //         //         Target = target,
    //         //         Amount = CalculateDamage(),
    //         //         Type = DamageType.Fire,
    //         //         CritChance = ctx.caster.GetComponent<Stats>().CritChance
    //         //     });
    //         // Debug.Log($"{caster.name} dealt {amount} damage to {target.name}");


    //     // State (block, dash, etc.)
    //     // ↓
    //     // Executor (runs phases, timers)
    //     // ↓
    //     // Effects
    //     // - DamageEffect → CombatSystem.Resolve(DamageInstance)
    //     // - KnockbackEffect → ApplyForce()
    //     // - BurnEffect → CombatSystem.ApplyStatus(StatusInstance)
    //     // - VFXEffect → Spawn particles
    //     }
    // }

}
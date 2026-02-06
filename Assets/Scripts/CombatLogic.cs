using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;





public class CombatLogic : RegisteredService, IServiceStep
{
    
    List<CombatEvent> pending = new();


    public override void Initialize()
    {
        Link.Global<Message<Request, CombatEvent>>(HandleCombatEvent);
    }

    public void Step()
    {
        ProcessPendingCombatEvents();
    }

    void ProcessPendingCombatEvents()
    {
        var ToRemove = new List<CombatEvent>(); 

        foreach (var combat in pending)
        {
            ProcessCombatEvent(combat);
            ToRemove.Add(combat);
        }

        ToRemove.ForEach(combat => pending.Remove(combat));
    }


    void ProcessCombatEvent(CombatEvent combat)
    {
        var source  = combat.Source;
        var target  = combat.Target;
        var package = combat.Package;

        var actor   = target as IDamageable;

        int damage  = ProcessComponents(source, target, package.Components);
        
        ApplyDamage(actor, damage);

        // Under consideration killing blow effect for lifecycle? 
        // target.Emit.Local(Request.Create, new KillingBlow());
    }

    int ProcessComponents(Actor source, Actor target, List<DamageComponent> components)
    {
        int damage = 0;

        foreach (var component in components)
        {            
            RequestEffects(target, component.Effects);
            ResolveDamageSource(source, target, component);

            damage += component.Amount;
        }

        return damage;
    }

    void ResolveDamageSource(Actor source, Actor target, DamageComponent component)
    {
        if (IsPhysicalSource(source) && IsKineticWeapon(component.Source))
        {
            ResolveKineticSource(source, target, component);
        }

    }

    void ResolveKineticSource(Actor source, Actor target, DamageComponent component)
    {

    }

    void ApplyKineticForce(Actor target, Vector2 direction, float force)
    {
        // target.Emit.Local(Request.Create, )
    }



    public void RequestEffects(Actor actor, List<Effect> effects)
    {
        foreach (var effect in effects)
        {
            actor.Emit.Local(Request.Create, effect);
        }
    }


    void ApplyDamage(IDamageable target, int damage)
    {
        if (target.Invulnerable)
            return;
        
        target.Health -= damage;
    }

    public void HandleCombatEvent(Message<Request, CombatEvent> message)
    {
        pending.Add(message.Payload);
    }
    // ============================================================================
    // QUERIES AND HELPERS
    // ============================================================================

    Vector2 CalculateKineticDirection(Actor source, Actor target)
    {
        return (target.Bridge.View.transform.position - source.Bridge.View.transform.position).normalized;
    }

    float CalculateKineticForce(IPhysical source, WeaponAction weapon)
    {
        return weapon.ForceCalculation switch
        {
            KineticForceCalculation.Fixed           => weapon.BaseForce,
            KineticForceCalculation.VelocityScaled  => weapon.BaseForce * source.Velocity.magnitude,
            KineticForceCalculation.MomentumBased   => weapon.BaseForce * source.Velocity.magnitude * source.Mass,
            _ => weapon.BaseForce,
        };
    }



    // ============================================================================
    // PREDICATEs
    // ============================================================================

    bool IsPhysicalSource(Actor source)
    {
        return source is IPhysical;
    }

    bool IsKineticWeapon(object source)
    {
        return source is WeaponAction instance && instance.AppliesKineticForce;
    }




    public UpdatePriority Priority => ServiceUpdatePriority.Combat;

}


public struct DamageComponent
{
    public object Source                    { get; init; }
    public int Amount                       { get; init; }
    public List<Effect> Effects             { get; init; }

    public DamageComponent(object source, int amount)
    {
        Source  = source;
        Amount  = amount;
        Effects = new();
    }
}


public readonly struct DamagePackage
{
    public List<DamageComponent> Components                 { get; init; }

    public DamagePackage(List<DamageComponent> components = null)
    {
        Components = components ?? new();
    }
}


public readonly struct CombatEvent
{
    public Actor Target                     { get; init; }
    public Actor Source                     { get; init; }
    public DamagePackage Package            { get; init; }
}




using System.Collections.Generic;
using UnityEngine;





public class CombatDamage : RegisteredService, IServiceStep
{
    
    List<CombatEvent> pending = new();

    public override void Initialize()
    {
        Link.Global<Message<Request, CombatEvent>>(HandleCombatEvent);
    }

    public void Step()
    {
        ProcessPendingEvents();
    }


    void ProcessPendingEvents()
    {
        foreach (var combat in pending)
        {
            ProcessEvent(combat);
        }

        pending.Clear();
    }


    void ProcessEvent(CombatEvent combat)
    {
        var source      = combat.Source;
        var target      = combat.Target;
        var components  = combat.Package.Components;

        int damage      = ProcessComponents(source, target, components);
        
        ApplyDamage(target, damage);

        // Under consideration killing blow effect for lifecycle? 
        // if (HasKilled(target))
            // SendKillingBlow();
        // target.Emit.Local(Request.Create, new KillingBlow());
    }

    int ProcessComponents(Actor source, Actor target, List<DamageComponent> components)
    {
        int damage = 0;

        foreach (var component in components)
        {            
            ApplyEffects(target, component.Effects);

            if (HasDynamicForce(component))
            {
                ResolveDynamicForce(source, target, component);
            }

            damage += component.Amount;
        }

        return damage;
    }


    void ResolveDynamicForce(Actor source, Actor target, DamageComponent component)
    {
        var direction   = CalculateDirection(source, target);
        var force       = direction * component.ForceMagnitude;

        ApplyDynamicForce(target, force);
    }


    void ApplyDynamicForce(Actor target, Vector2 force)
    {

        if (target is not IDynamic dynamic)
            return;

        var definition = new MovementDefinition()
        {
            MovementForce   = MovementForce.Dynamic,
            DynamicSource   = DynamicSource.Collision,

            Force           = force,
            Mass            = dynamic.Mass,
        };

        target.Emit.Local(Request.Create, definition);
    }


    public void ApplyEffects(Actor actor, List<Effect> effects)
    {
        foreach (var effect in effects)
        {
            actor.Emit.Local(Request.Create, effect);
        }
    }


    void ApplyDamage(Actor target, int damage)
    {
        if (!CanTakeDamage(target, out var actor))
            return;

        actor.Health -= damage;
    }

    public void HandleCombatEvent(Message<Request, CombatEvent> message)
    {
        pending.Add(message.Payload);
    }


    // ============================================================================
    // QUERIES AND HELPERS
    // ============================================================================

    Vector2 CalculateDirection(Actor source, Actor target)
    {
        return (target.Bridge.View.transform.position - source.Bridge.View.transform.position).normalized;
    }

    bool HasKilled(Actor target)
    {
        if (!CanTakeDamage(target, out var actor))
            return false;

        return actor.Health <= 0;
    }

    bool CanTakeDamage(Actor target, out IDamageable actor)
    {
        if (target is IDamageable damageable && !damageable.Invulnerable)
        {
            actor = damageable;
            return true;
        }

        actor = null;
        return false;
    }


    // ============================================================================
    // PREDICATEs
    // ============================================================================

    bool HasDynamicForce(DamageComponent component)
    {
        return component.ForceMagnitude > 0;
    }


    public UpdatePriority Priority => ServiceUpdatePriority.Combat;
}


public struct DamageComponent
{
    public object Source                    { get; init; }
    public int Amount                       { get; init; }
    public List<Effect> Effects             { get; init; }

    /// <summary> force * velocity.magnitude * mass </summary>
    public float ForceMagnitude             { get; init;}

    public DamageComponent(object source, int amount, float forceMagnitude = 0)
    {
        Source          = source;
        Amount          = amount;
        ForceMagnitude  = forceMagnitude;

        Effects         = new();
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




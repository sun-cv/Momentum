using System.Collections.Generic;
using UnityEngine;



public class CombatResolver : RegisteredService, IServiceStep, IInitialize
{
    readonly List<CombatContext> pendingRequests = new();

    // ===============================================================================

    public void Initialize()
    {
        Link.Global<Message<Request, CombatEvent>>(HandleCombatEvent);
    }
    
    // ===============================================================================
    
    public void Step()
    {
        ProcessPendingEvents();
    }

    void ProcessPendingEvents()
    {
        foreach (var combat in pendingRequests)
        {
            ProcessCombatEvent(combat);
        }

        pendingRequests.Clear();
    }

    void ProcessCombatEvent(CombatContext context)
    {
        var source      = context.Source;
        var target      = context.Target;
        var components  = context.Package.Components;

        float damage    = ProcessComponents(source, target, components);
        
        ApplyDamage(target, damage);

        // if (HasKilled(target))
        //     SendKillingBlow();

    }

    // ===============================================================================

    float ProcessComponents(Actor source, Actor target, List<DamageComponent> components)
    {
        float damage = 0;

        foreach (var component in components)
        {            
            ApplyEffects(target, component.Effects);

            if (ForceIsDynamic(component))
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

        target.Emit.Local(Request.Create, new MovementEvent(target, definition));
    }

    void ApplyDamage(Actor target, float damage)
    {
        if (!CanTakeDamage(target, out var actor))
            return;

        actor.Health -= damage;

        Log.Debug($"{actor.GetType().Name} has taken {damage} damage. Health: {actor.Health}");
    }

    // ===============================================================================
    //  Effect Management
    // ===============================================================================

    public void ApplyEffects(Actor actor, List<Effect> effects)
    {
        foreach (var effect in effects)
        {
            actor.Emit.Local(Request.Create, effect);
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
    
    public void HandleCombatEvent(Message<Request, CombatEvent> message)
    {
        pendingRequests.Add(message.Payload.Context);
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool HasKilled(Actor target)
    {
        if (!CanTakeDamage(target, out var actor))
        {
            return false;
        }
        return actor.Health <= 0;
    }

    bool CanTakeDamage(Actor target, out IDamageable actor)
    {
        if (target is IDamageable damageable && !damageable.Invulnerable && !damageable.Impervious)
        {
            actor = damageable;
            return true;
        }

        actor = null;
        return false;
    }

    bool ForceIsDynamic(DamageComponent component)
    {
        return component.ForceMagnitude > 0;
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    Vector2 CalculateDirection(Actor source, Actor target)
    {
        return (target.Bridge.View.transform.position - source.Bridge.View.transform.position).normalized;
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Combat);

    public override void Dispose()
    {
        // NO OP;
    }

    public UpdatePriority Priority => ServiceUpdatePriority.Combat;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

    // REWORK REQUIRED - Implementation of killing blow data, type (DeathType enum?) 
public class KillingBlow
{
    
}


public readonly struct DamageComponent
{
    public object Source                    { get; init; }
    public float Amount                     { get; init; }
    public List<Effect> Effects             { get; init; }

    /// <summary> force * velocity.magnitude * mass </summary>
    public float ForceMagnitude             { get; init;}

    public DamageComponent(object source, float amount, float forceMagnitude = 0)
    {
        Source          = source;
        Amount          = amount;
        ForceMagnitude  = forceMagnitude;

        Effects         = new();
    }
}

public readonly struct DamagePackage
{
    public List<DamageComponent> Components { get; init; }

    public DamagePackage(List<DamageComponent> components = null)
    {
        Components = components ?? new();
    }
}

public readonly struct CombatContext
{
    public Actor Target                     { get; init; }
    public Actor Source                     { get; init; }
    public DamagePackage Package            { get; init; }
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CombatEvent
{
    public CombatContext Context            { get; init; }
}




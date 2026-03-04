using System.Collections.Generic;
using UnityEngine;



public class CombatResolver : RegisteredService, IServiceStep, IInitialize
{
    readonly List<CombatContext> queue = new();

    // ===============================================================================

    public void Initialize()
    {
        Link.Global<Message<Request, CombatEvent>>(HandleCombatEvent);
    }
    
    // ===============================================================================
    
    public void Step()
    {
        ProcessQueuedEvents();
    }

    void ProcessQueuedEvents()
    {
        foreach (var combat in queue)
        {
            ProcessCombatEvent(combat);
        }

        queue.Clear();
    }

    void ProcessCombatEvent(CombatContext context)
    {
        var source      = context.Source;
        var target      = context.Target;
        var components  = context.Package.Components;

        float damage    = ProcessComponents(source, target, components);
        
        // rework required send damage request.
    }

    // ===============================================================================

    float ProcessComponents(Actor source, Actor target, List<DamageComponent> components)
    {
        float damage = 0;

        foreach (var component in components)
        {            
            damage += ProcessComponent(source, target, component);
        }

        return damage;
    }

    float ProcessComponent(Actor source, Actor target, DamageComponent component)
    {
        ApplyEffects(target, component.Effects);
        ResolveDynamicForce(source, target, component);

        return component.Amount;
    }

    void ResolveDynamicForce(Actor source, Actor target, DamageComponent component)
    {
        if (!ComponentHasForce(component))
            return;

        var direction = CalculateDirection(source, target);

        Emit.Global(Request.Create, new ForcePhysicsEvent
        {
            Owner  = source,
            Target = target,
            Phase  = CollisionPhase.Enter,
            Normal = -direction,
            Impact = component.Force
        });
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
        queue.Add(message.Payload.Context);
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================


    bool ComponentHasForce(DamageComponent component)
    {
        return component.Force > 0;
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
    public float Amount                     { get; init; }
    public object Source                    { get; init; }
    public List<Effect> Effects             { get; init; }

    public float Force             { get; init;}

    public DamageComponent(object source, float amount, float force = 0)
    {
        Source          = source;
        Amount          = amount;
        Force           = force;

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




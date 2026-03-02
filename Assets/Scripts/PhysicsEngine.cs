using System.Collections.Generic;
using UnityEngine;



public class PhysicsEngine : RegisteredService, IServiceTick, IInitialize
{
    float friction = Settings.Physics.FRICTION;

        // -----------------------------------

    readonly List<IPhysicsResolver> resolvers = new();

        // -----------------------------------

    public void Initialize()
    {
        Register(new SurfaceContactResolver(this));
        Register(new ActorContactResolver(this));
        Register(new ForceApplicationResolver(this));
    }

    // ===============================================================================

    public void Tick()
    {

        TickResolvers();
        DecayPhysicsVelocity();
    }
    // ===============================================================================

    void Register(IPhysicsResolver resolver)
    {
        resolvers.Add(resolver);
    }

    void TickResolvers()
    {
        foreach (var resolver in resolvers)
            resolver.Resolve();
    }

    void DecayPhysicsVelocity()
    {
        foreach (var actor in Actors.GetActors())
        {
            if (actor is not IPhysicsBody body) 
                continue;

            if (body.Force.magnitude < 0.001f)
            {
                body.Force = Vector2.zero;
                continue;
            }

            body.Force *= Mathf.Exp(-friction * Clock.DeltaTime);
        }
    }

    // ===============================================================================

    public override void Dispose()
    {   
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.PhysicsEngine;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IPhysicsEvent
{
    Actor           Owner                   { get; }
    CollisionPhase  Phase                   { get; }
}

public interface IPhysicsResolver
{
    void Resolve();
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Handlers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Force                                               
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ForceApplicationResolver : IPhysicsResolver
{
    readonly Queue<ForcePhysicsEvent> pending = new();

    public ForceApplicationResolver(PhysicsEngine engine)
    {
        Link.Global<Message<Request, ForcePhysicsEvent>>(HandleEvent);
    }

    public void Resolve()
    {
        while (pending.Count > 0)
        {
            ProcessForce(pending.Dequeue());
        }
    }

    void ProcessForce(ForcePhysicsEvent instance)
    {
        if (instance.Phase != CollisionPhase.Enter) 
            return;

        if (instance.Target is not IPhysicsBody body) 
            return;
            
        if (instance.Target is not IDynamic dynamic)
            return;

        var physicsDefinition = instance.Target.Definition.Physics;

        if (instance.Impact < physicsDefinition.MomentumThreshold) 
            return;

        float resistance     = physicsDefinition.PushResistance;
        Vector2 appliedForce = (1f - resistance) * instance.Impact * -instance.Normal / dynamic.Mass;

        body.Force += appliedForce;
    }

    void HandleEvent(Message<Request, ForcePhysicsEvent> message)
    {
        pending.Enqueue(message.Payload);
    }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Actor                                               
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ActorContactResolver : IPhysicsResolver
{
    readonly Queue<CollisionPhysicsEvent> pending = new();

    public ActorContactResolver(PhysicsEngine engine)
    {
        Link.Global<Message<Request, CollisionPhysicsEvent>>(HandleEvent);
    }

    public void Resolve()
    {
        while (pending.Count > 0)
        {
            ProcessCollision(pending.Dequeue());
        }
    }

    void ProcessCollision(CollisionPhysicsEvent instance)
    {
        switch (instance.Phase)
        {
            case CollisionPhase.Enter:
            case CollisionPhase.Stay:
                ApplyTransfer(instance);
                break;
        }
    }

    void ApplyTransfer(CollisionPhysicsEvent instance)
    {
        if (instance.Owner is not IDynamic ownerDynamic) return;
        if (instance.Other is not IPhysicsBody otherBody) return;
        if (instance.Other is not IDynamic otherDynamic) return;
        if (otherBody.ImmuneToForce) return;
    
        var otherPhysics = instance.Other.Definition.Physics;
    
        if (instance.Impact < otherPhysics.MomentumThreshold) return;
    
        float totalMass     = ownerDynamic.Mass + otherDynamic.Mass;
        float transferRatio = ownerDynamic.Mass / totalMass * (1f - otherPhysics.PushResistance);
    
        Vector2 targetVelocity = instance.Impact * transferRatio * -instance.Normal / otherDynamic.Mass;
    
        Debug.Log($"ApplyTransfer | impact: {instance.Impact:F2} | transferRatio: {transferRatio:F2} | targetVelocity: {targetVelocity.magnitude:F2} | currentForce before: {otherBody.Force.magnitude:F2}");
    
        otherBody.Force = targetVelocity;
    
        Debug.Log($"ApplyTransfer | force after set: {otherBody.Force.magnitude:F2}");
    }
    void HandleEvent(Message<Request, CollisionPhysicsEvent> message)
    {
        pending.Enqueue(message.Payload);
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Surface                                              
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class SurfaceContactResolver : IPhysicsResolver
{
    readonly Queue<SurfacePhysicsEvent> pending = new();

    public SurfaceContactResolver(PhysicsEngine engine)
    {
        Link.Global<Message<Request, SurfacePhysicsEvent>>(HandleSurfaceEvent);
    }

    public void Resolve()
    {
        while (pending.Count > 0)
        {
            ProcessSurface(pending.Dequeue());
        }
    }

    void ProcessSurface(SurfacePhysicsEvent instance)
    {
        switch (instance.Phase)
        {
            case CollisionPhase.Enter:
            case CollisionPhase.Stay:
                OnContact(instance);
                break;
            case CollisionPhase.Exit:
                OnExit(instance);
                break;
        }
    }

    void OnContact(SurfacePhysicsEvent instance)
    {
        if (instance.Owner is not IPhysicsBody body) 
            return;

        body.Constrained = true;
        body.Normal      = instance.Normal;

        ConstrainPhysicsVelocity(body, instance.Normal);
    }

    void OnExit(SurfacePhysicsEvent instance)
    {
        if (instance.Owner is not IPhysicsBody body) 
            return;

        body.Constrained = false;
        body.Normal      = Vector2.zero;
    }

    void ConstrainPhysicsVelocity(IPhysicsBody body, Vector2 normal)
    {
        var velocity      = body.Force;
        float penetration = Vector2.Dot(velocity, -normal);

        if (penetration <= 0 || velocity.magnitude < 0.001f) return;

        float directness = penetration / velocity.magnitude;

        if (directness > 0.7f)
        {
            body.Force = Vector2.zero;
        }
        else
        {
            Vector2 tangent      = new(-normal.y, normal.x);
            body.Force = Vector2.Dot(velocity, tangent) * tangent;
        }
    }

    void HandleSurfaceEvent(Message<Request, SurfacePhysicsEvent> message)
    {
        pending.Enqueue(message.Payload);
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct ForcePhysicsEvent : IPhysicsEvent
{
    public Actor            Owner           { get; init; }
    public Actor            Target          { get; init; }
    public CollisionPhase   Phase           { get; init; }
    public Vector2          Normal          { get; init; }
    public float            Impact          { get; init; }
}

public readonly struct CollisionPhysicsEvent : IPhysicsEvent
{
    public Actor            Owner           { get; init; }
    public Actor            Other           { get; init; }
    public CollisionPhase   Phase           { get; init; }
    public Vector2          Normal          { get; init; }
    public float            Impact          { get; init; }
}

public readonly struct SurfacePhysicsEvent : IPhysicsEvent
{
    public Actor            Owner           { get; init; }
    public CollisionPhase   Phase           { get; init; }
    public Vector2          Normal          { get; init; }
    public float            Impact          { get; init; }
}
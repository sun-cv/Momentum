using System.Collections.Generic;
using UnityEngine;



public class CollisionHandler : RegisteredService, IServiceTick
{
        // -----------------------------------

    List<ProcessedCollision> pendingCollisions    = new();

        // -----------------------------------

    public CollisionHandler()
    {
        Link.Global<Message<Request, CollisionEvent>>(HandleCollisionEvent);
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessCollisions();
    }

    // ===============================================================================

    void ProcessCollisions()
    {
        foreach( var collision in pendingCollisions)
        {
            ProcessCollision(collision);
        }

        pendingCollisions.Clear();
    }

     void ProcessCollision(ProcessedCollision collision)
    {
        switch (collision.Type)
        {
            case CollisionType.Actor:
                Emit.Global(Request.Create, new CollisionPhysicsEvent
                {
                    Owner  = collision.Owner,
                    Other  = collision.Other,
                    Phase  = collision.Phase,
                    Normal = collision.Normal,
                    Impact = collision.Impact
                });
                break;

            case CollisionType.Environment:
            case CollisionType.Prop:
                Emit.Global(Request.Create, new SurfacePhysicsEvent
                {
                    Owner  = collision.Owner,
                    Phase  = collision.Phase,
                    Normal = collision.Normal,
                    Impact = collision.Impact
                });
                break;
        }
    }

    Actor GetActor(Collision2D collision)
    {
        return collision.gameObject.GetComponent<BridgeController>().Bridge.Owner;
    }

    float CalculateImpactForce(Actor owner, Vector2 normal)
    {
        if (owner is IDynamic dynamic)
            return Mathf.Max(0, Vector2.Dot(dynamic.Momentum, -normal));

        return 0f;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================


    void HandleCollisionEvent(Message<Request, CollisionEvent> message)
    {
        var phase     = message.Payload.Phase;
        var collision = message.Payload.Collision;
        var owner     = message.Payload.Owner;
        var type      = GetType(collision);

        var normal = Vector2.zero;
        var impact = 0f;
        Actor other = null;

        if (phase != CollisionPhase.Exit && collision.contactCount > 0)
        {
            normal = collision.GetContact(0).normal;
            impact = CalculateImpactForce(owner, normal);
        }

        if (type == CollisionType.Actor)
            other = GetActor(collision);

        pendingCollisions.Add(new ProcessedCollision
        {
            Owner  = owner,
            Other  = other,
            Type   = type,
            Phase  = phase,
            Normal = normal,
            Impact = impact
        });
    }


    CollisionType GetType(Collision2D collision)
    {
        int layer = collision.gameObject.layer;

        if (layer == Layers.Player || layer == Layers.Enemy || layer == Layers.NPC)
            return CollisionType.Actor;

        if (layer == Layers.Prop)
            return CollisionType.Prop;

        return CollisionType.Environment;
    }

    // ===============================================================================

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.CollisionHandler;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
public enum CollisionPhase  
{ 
    Enter, 
    Stay, 
    Exit    
}

public enum CollisionType   
{ 
    Actor, 
    Prop, 
    Environment
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Structs                                                
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public struct ProcessedCollision
{
    public Actor          Owner;
    public Actor          Other;
    public CollisionType  Type;
    public CollisionPhase Phase;
    public Vector2        Normal;
    public float          Impact;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CollisionEvent
{
    public Actor            Owner       { get; init; }
    public CollisionPhase   Phase       { get; init; }
    public Collision2D      Collision   { get; init; }

    public CollisionEvent(Actor owner, CollisionPhase phase, Collision2D collision)
    {
        Owner       = owner;
        Phase       = phase;
        Collision   = collision;
    }
}


using UnityEngine;



public class CollisionHandler
{
    readonly Actor owner;

        // -----------------------------------

    public CollisionHandler(Actor owner)
    {
        this.owner = owner;

        owner.Emit.Link.Local<Message<Request, CollisionEvent>>(HandleCollisionEvent);
    }

    // ===============================================================================

    public void OnEnter(Collision2D collision)
    {
        var normal    = collision.GetContact(0).normal;
        var momentum  = GetMomentum();
        var impact    = CalculateImpactForce(momentum, normal);

        DispatchMovementImpact(normal, impact);
        DispatchCollisionImpact(collision, normal, impact, CollisionPhase.Enter);
    }

    public void OnStay(Collision2D collision)
    {
        var normal          = collision.GetContact(0).normal;
        var momentum        = GetMomentum();
        float penetration   = Vector2.Dot(momentum.normalized, -normal);

        if (penetration < 0.8f) 
            return;

        var impact = CalculateImpactForce(momentum, normal);
        DispatchMovementImpact(normal, impact);
    }
    
    public void OnExit(Collision2D collision)
    {
        DispatchCollisionImpact(collision, Vector2.zero, 0f, CollisionPhase.Exit);
    }

    float CalculateImpactForce(Vector2 momentum, Vector2 normal)
    {
        return Mathf.Max(0, Vector2.Dot(momentum, -normal));
    }

    Vector2 GetMomentum()
    {
        if (owner is IDynamic dynamic)
            return dynamic.Momentum;

        return Vector2.zero;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

void DispatchMovementImpact(Vector2 normal, float impact)
{    
    if (impact <= 0) 
        return;

    owner.Emit.Local(Request.Create, new CollisionImpactEvent(normal, impact));
}

    void DispatchCollisionImpact(Collision2D collision, Vector2 normal, float impact, CollisionPhase phase)
    {
        if (impact <= 0 && phase != CollisionPhase.Exit) return;

        owner.Emit.Local(Request.Create, new CollisionPublishEvent(phase, collision, normal, impact));
    }


    void HandleCollisionEvent(Message<Request, CollisionEvent> message)
    {   
        switch (message.Payload.Phase)
        {
            case CollisionPhase.Enter: 
                OnEnter(message.Payload.Collision);
                break;
            case CollisionPhase.Stay: 
                OnStay(message.Payload.Collision);
                break;
            case CollisionPhase.Exit: 
                OnExit(message.Payload.Collision);
                break;
        }        
    }

}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
public enum CollisionPhase { Enter, Stay, Exit }


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CollisionImpactEvent
{
    public Vector2 Normal   { get; init; }
    public float Impact     { get; init; }

    public CollisionImpactEvent(Vector2 normal, float impact)
    {
        Normal  = normal;
        Impact  = impact;
    }
}

public readonly struct CollisionEvent
{
    public CollisionPhase Phase     { get; init; }
    public Collision2D Collision    { get; init; }


    public CollisionEvent(CollisionPhase phase, Collision2D collision)
    {
        Phase       = phase;
        Collision   = collision;
    }
}

public readonly struct CollisionPublishEvent
{
    public CollisionPhase Phase     { get; init; }
    public Collision2D Collision    { get; init; }
    public Vector2 Normal           { get; init; }
    public float Impact             { get; init; }

    public CollisionPublishEvent(CollisionPhase phase, Collision2D collision, Vector2 normal, float impact)
    {
        Phase       = phase;
        Collision   = collision;
        Normal      = normal;
        Impact      = impact;
    }
}
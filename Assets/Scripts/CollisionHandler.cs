using System.Collections.Generic;
using UnityEngine;



public class CollisionHandler : RegisteredService, IServiceTick
{
    // -----------------------------------

    readonly List<CollisionContext> collisionQueue  = new();
    readonly List<ContactContext>   contactQueue    = new();

        // -----------------------------------

    public CollisionHandler()
    {
        Link.Global<Message<Request, CollisionHandlerEvent>>(HandleCollisionEvent);
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessCollisions();
    }

    // ===============================================================================

    void ProcessCollisions()
    {
        foreach( var context in collisionQueue)
        {
            Emit.Global(Request.Create, new CollisionEvent(context));
        }
        
        foreach( var context in contactQueue)
        {
            Emit.Global(Request.Create, new ContactEvent(context));
        }

        collisionQueue  .Clear();
        contactQueue    .Clear();
    }


    Actor GetTarget(Collision2D collision)
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


    void HandleCollisionEvent(Message<Request, CollisionHandlerEvent> message)
    {
        var instance = message.Payload;

        switch(GetType(instance.Collision))
        {
            case CollisionType.Actor:            
            case CollisionType.Prop:
                CreateContactContext(instance);
                break;
            case CollisionType.Environment:       
                CreateCollisionContext(instance);
                break;
        }

    }

    void CreateContactContext(CollisionHandlerEvent instance)
    {
        if (instance.Phase == CollisionPhase.Exit)
            return;

        var source      = instance.Source;
        var target      = GetTarget(instance.Collision);
        var phase       = instance.Phase;
        var normal      = instance.Collision.GetContact(0).normal;
        var magnitude   = CalculateImpactForce(source, normal);

        var force       = new Force(magnitude);
        var collision   = new Collision(normal);
        var contact     = new Contact(force, collision);
        var component   = new ContactComponent(contact, phase);
        var package     = new ContactPackage(new());

        package.Components.Add(component);        

        var context = new ContactContext(source, target, package);

        contactQueue.Add(context);
    }

    void CreateCollisionContext(CollisionHandlerEvent instance)
    {
        if (instance.Phase == CollisionPhase.Exit)
            return;
        
        var phase       = instance.Phase;
        var normal      = instance.Collision.GetContact(0).normal;        
        var source      = instance.Source;

        var collision   = new Collision(normal);
        var component   = new CollisionComponent(collision, phase);
        var package     = new CollisionPackage(new());

        package.Components.Add(component);        

        var context = new CollisionContext(source, package);

        collisionQueue.Add(context);
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

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CollisionHandlerEvent
{
    public Actor            Source      { get; init; }
    public CollisionPhase   Phase       { get; init; }
    public Collision2D      Collision   { get; init; }

    public CollisionHandlerEvent(Actor source, CollisionPhase phase, Collision2D collision)
    {
        Source      = source;
        Phase       = phase;
        Collision   = collision;
    }
}

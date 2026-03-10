using System.Collections.Generic;
using UnityEngine;



public class PhysicsEngine : RegisteredService, IServiceTick, IInitialize
{

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


public interface IPhysicsResolver : IResolver
{
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Structs                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct Force
{
    public float Magnitude                          { get; init; }

    public Force(float magnitude)
    {
        Magnitude  = magnitude;
    }
}

public readonly struct ForceComponent
{
    public Force Force                              { get; init; }

    public ForceComponent(Force force)
    {
        Force = force;
    }
}

public readonly struct ForcePackage
{
    public List<ForceComponent> Components      { get; init; }

    public ForcePackage(List<ForceComponent> components)
    {
        Components = components;
    }
}

public readonly struct ForceContext
{
    public Actor Source                         { get; init; }
    public Actor Target                         { get; init; }
    public ForcePackage Package                 { get; init; }

    public ForceContext(Actor source, Actor target, ForcePackage package)
    {
        Source  = source;
        Target  = target;
        Package = package;
    }
}


public readonly struct Collision
{
    public Vector2 Normal                           { get; init; }

    public Collision(Vector2 normal)
    {
        Normal = normal;
    }
}

public readonly struct CollisionComponent
{
    public Collision Collision                      { get; init; }
    public CollisionPhase Phase                     { get; init; }

    public CollisionComponent(Collision collision, CollisionPhase phase)
    {
        Collision = collision;
        Phase = phase;
    }
}

public readonly struct CollisionPackage
{
    public List<CollisionComponent> Components  { get; init; }

    public CollisionPackage(List<CollisionComponent> components)
    {
        Components = components;
    }
}

public readonly struct CollisionContext
{
    public Actor Source                         { get; init; }
    public CollisionPackage Package             { get; init; }

    public CollisionContext(Actor source, CollisionPackage package)
    {
        Source  = source;
        Package = package;
    }
}

public readonly struct Contact
{
    public Force Force                              { get; init; }
    public Collision Collision                      { get; init; }

    public Contact(Force force, Collision collision)
    {
        Force = force;
        Collision = collision;
    }
}

public readonly struct ContactPackage
{
    public List<ContactComponent> Components  { get; init; }

    public ContactPackage(List<ContactComponent> components)
    {
        Components = components;
    }
}

public readonly struct ContactComponent
{
    public Contact Contact                          { get; init; }
    public CollisionPhase Phase                     { get; init; }

    public ContactComponent(Contact contact, CollisionPhase phase)
    {
        Contact = contact;
        Phase = phase;
    }
}

public readonly struct ContactContext
{
    public Actor Source                         { get; init; }
    public Actor Target                         { get; init; }
    public ContactPackage Package               { get; init; }

    public ContactContext(Actor source, Actor target, ContactPackage package)
    {
        Source  = source;
        Target  = target;
        Package = package;
    }
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Resolvers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Force                                               
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ForceApplicationResolver : IPhysicsResolver
{
    readonly List<ForceContext> queue = new();

    public ForceApplicationResolver(PhysicsEngine engine)
    {
        Link.Global<ForceEvent>(HandleEvent);
    }

    public void Resolve()
    {
        ProcessQueue();
    }


    void ProcessQueue()
    {
        foreach(var context in queue)
        {
            ProcessForces(context);
        }

        queue.Clear();
    }

    void ProcessForces(ForceContext context)
    {
        foreach (var component in context.Package.Components)
        {
            ProcessForce(context, component.Force);
        }   
    }

    void ProcessForce(ForceContext context, Force force)
    {
        if (context.Target is not IPhysicsBody targetBody) 
            return;

        if (context.Target is not IDynamic target) 
            return;

        if (targetBody.ImmuneToForce) 
            return;

        var physicsDefinition = context.Target.Definition.Physics;

        if (force.Magnitude < physicsDefinition.MomentumThreshold) 
            return;

        var normal = (context.Target.Bridge.View.transform.position - context.Source.Bridge.View.transform.position).normalized;

        float resistance     = physicsDefinition.PushResistance;
        Vector2 appliedForce = (1f - resistance) * force.Magnitude * normal / target.Mass;

        if (context.Source is Hero) Log.Debug("Hero.Magnitude",     () => force.Magnitude);
        if (context.Source is Hero) Log.Debug("Hero.Applied",       () => appliedForce.magnitude);

        if (context.Source is not Hero) Log.Debug("Other.Mass",     () => target.Mass);

        targetBody.Force += appliedForce;
    }

    void HandleEvent(ForceEvent message)
    {
        queue.Add(message.Context);
    }

    readonly Logger Log = new(LogSystem.ContactResolver, LogLevel.Debug);
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Actor                                               
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ActorContactResolver : IPhysicsResolver
{
    readonly List<ContactContext> queue = new();

    public ActorContactResolver(PhysicsEngine engine)
    {
        Link.Global<ContactEvent>(HandleEvent);
    }

    public void Resolve()
    {
        ProcessQueue();
    }

    void ProcessQueue()
    {
        foreach(var context in queue)
        {
            ProcessCollisions(context);
        }

        queue.Clear();
    }

    void ProcessCollisions(ContactContext context)
    {
        foreach (var component in context.Package.Components)
        {
            switch (component.Phase)
            {
                case CollisionPhase.Enter:
                case CollisionPhase.Stay:
                    ApplyTransfer(context, component.Contact);
                    break;
            }
        }
    }

    void ApplyTransfer(ContactContext context, Contact contact)
    {
        if (context.Source is not IDynamic source) 
            return;

        if (context.Target is not IPhysicsBody targetBody) 
            return;

        if (context.Target is not IDynamic target) 
            return;

        if (targetBody.ImmuneToForce) 
            return;

        var targetPhysics   = context.Target.Definition.Physics;
        float impact        = contact.Force.Magnitude * source.Impact;

        if (impact < targetPhysics.MomentumThreshold) 
            return;

        float massRatio      = source.Mass / (source.Mass + target.Mass);
        float transferRatio  = massRatio * (1f - targetPhysics.PushResistance);
        Vector2 appliedForce = transferRatio * impact * -contact.Collision.Normal / target.Mass;

        if (context.Source is Hero) Log.Debug("Hero.Magnitude",     () => contact.Force.Magnitude);
        if (context.Source is Hero) Log.Debug("Hero.Impact",        () => impact);
        if (context.Source is Hero) Log.Debug("Hero.Applied",       () => appliedForce.magnitude);

        if (context.Source is not Hero) Log.Debug("Other.Mass",     () => targetPhysics.Mass);

        if (context.Source is IPhysicsBody ownerBody && !ownerBody.ImmuneToForce)
        {
            var ownerPhysics = context.Source.Definition.Physics;

            if (impact >= ownerPhysics.BleedThreshold)
            {
                ownerBody.Force += ownerPhysics.BleedRatio * impact * contact.Collision.Normal / source.Mass;
            }
        }

        targetBody.Force = appliedForce;
    }


    void HandleEvent(ContactEvent message)
    {
        queue.Add(message.Context);
    }

    // ===============================================================================

    Logger Log = new(LogSystem.ContactResolver, LogLevel.Debug);
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Surface                                              
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class SurfaceContactResolver : IPhysicsResolver
{
    readonly List<CollisionContext> queue = new();

    public SurfaceContactResolver(PhysicsEngine engine)
    {
        Link.Global<CollisionEvent>(HandleSurfaceEvent);
    }

    public void Resolve()
    {
        ProcessQueue();
    }

    void ProcessQueue()
    {
        foreach( var surface in queue)
        {
            ProcessSurface(surface);
        }

        queue.Clear();
    }

    void ProcessSurface(CollisionContext context)
    {
        foreach(var component in context.Package.Components)
        {
            switch (component.Phase)
            {
                case CollisionPhase.Enter:
                case CollisionPhase.Stay:
                    OnContact(context, component.Collision);
                    break;
                case CollisionPhase.Exit:
                    OnExit(context);
                    break;
            }
        }
    }

    void OnContact(CollisionContext context, Collision collision)
    {
        if (context.Source is not IPhysicsBody body)
            return;

        body.Constrained = true;
        body.Normal      = collision.Normal;

        ConstrainPhysicsVelocity(body, collision);
    }

    void OnExit(CollisionContext context)
    {
        if (context.Source is not IPhysicsBody body) 
            return;

        body.Constrained = false;
        body.Normal      = Vector2.zero;
    }

    void ConstrainPhysicsVelocity(IPhysicsBody body, Collision collision)
    {
        var velocity      = body.Force;
        float penetration = Vector2.Dot(velocity, -collision.Normal);

        if (penetration <= 0 || velocity.magnitude < 0.001f) return;

        float directness = penetration / velocity.magnitude;

        if (directness > 0.7f)
        {
            body.Force = Vector2.zero;
        }
        else
        {
            Vector2 tangent      = new(-collision.Normal.y, collision.Normal.x);
            body.Force = Vector2.Dot(velocity, tangent) * tangent;
        }
    }

    void HandleSurfaceEvent(CollisionEvent message)
    {
        queue.Add(message.Context);
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct ForceEvent : IMessage
{
    public ForceContext Context { get; init; }

    public ForceEvent(ForceContext context)
    {
        Context = context;
    }
}

public readonly struct ContactEvent : IMessage
{
    public ContactContext Context { get; init; }

    public ContactEvent(ContactContext context)
    {
        Context = context;
    }
}

public readonly struct CollisionEvent : IMessage
{
    public CollisionContext Context { get; init; }

    public CollisionEvent(CollisionContext context)
    {
        Context = context;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class MovementEngine : Service, IServiceTick
{
    readonly Logger Log = Logging.For(LogSystem.Movement);

    readonly float maxSpeed         = Settings.Movement.MAX_SPEED;
    readonly float acceleration     = Settings.Movement.ACCELERATION;
    readonly float friction         = Settings.Movement.FRICTION;
    readonly float retention        = Settings.Movement.MOMENTUM_RETENTION;
    readonly float inertia          = Settings.Movement.INERTIA;
    readonly float forceThreshold   = Settings.Movement.FORCE_THRESHOLD;

    readonly bool applyFriction = true;

    MovementModifierHandler modifierHandler;

    Actor           owner;
    IMovableActor   actor;
    Rigidbody2D     body;


    float modifier;

    float speed;
    float mass;

    Vector2 momentum    = Vector2.zero;
    Vector2 velocity    = Vector2.zero;

    List<MovementDirective> directives = new();

    public MovementEngine(Actor actor)
    {        
        Services.Lane.Register(this);

        this.owner          = actor;
        this.body           = actor.Bridge.Body;
        this.actor          = actor as IMovableActor;

        body.freezeRotation = true;
        body.gravityScale   = 0;
        body.interpolation  = RigidbodyInterpolation2D.Interpolate; 

        modifierHandler     = new(actor);

        owner.Emit.Link.Local<Message<Request, MovementEvent>>          (HandleMovementDirective);
        owner.Emit.Link.Local<Message<Request, ClearMovementScopeEvent>>(HandleMovementClear);
        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>     (HandlePresenceStateEvent);

        SetSpeed();
        SetMass();
    }


    public void Tick()
    {
        RemoveInactiveControllers();
    
        CalculateModifier();
        CalculateVelocity();
        CalculateMomentum();

        ApplyFriction();

        if (CanApplyVelocity())
            ApplyVelocity();

        DebugLog();
    }

    // ============================================================================
    // MOVEMENT CALCULATIONS
    // ============================================================================

    void CalculateModifier() => modifier = modifierHandler.Calculate();


    void CalculateVelocity()
    {
        if (ActorIsDisabled())
        {
            ClearMovement();
            return;
        }

        Vector2 kinematicVelocity   = CalculateKinematicForces();
        Vector2 dynamicVelocity     = CalculateDynamicForces();

        velocity                    = ApplyVelocityTransition(CombineMovementForces(kinematicVelocity, dynamicVelocity));
    }


    Vector2 CalculateKinematicForces()
    {
        Vector2 targetVelocity = Vector2.zero;

        if (ActorCanMove())
        {
            targetVelocity = BaseMovementVelocity();
        }

        foreach(var directive in FilterAndSortDescendingDirectives(IsKinematic))
        {
            Vector2 controllerVelocity = directive.Controller.CalculateVelocity(owner);

            switch(directive.Controller.Mode)
            {
                case ControllerMode.Ignore:
                    targetVelocity  = controllerVelocity;
                    goto FinishedBlending;

                case ControllerMode.Blend:
                    targetVelocity += controllerVelocity * directive.Controller.Weight;
                    break;

                case ControllerMode.AllowOverride:
                    targetVelocity  = Vector2.Lerp(targetVelocity, controllerVelocity, directive.Controller.Weight);
                    break;
            }
        }

        FinishedBlending:
        return targetVelocity;
    }


    Vector2 CalculateDynamicForces()
    {
        Vector2 forceVelocity = Vector2.zero;

        foreach(var directive in FilterAndSortDescendingDirectives(IsDynamic))
        {
            if (directive.Controller.Mode == ControllerMode.Additive)
            {
                forceVelocity += directive.Controller.CalculateVelocity(owner);
            }
        }

        return forceVelocity;
    }

    Vector2 CombineMovementForces(Vector2 directed, Vector2 external)
    {
        if (external.magnitude > forceThreshold)
        {
            return external + directed * 0.2f;
        }

        return directed;
    }

    Vector2 ApplyVelocityTransition(Vector2 targetVelocity)
    {
        bool isReversing = actor.CanMove && directives.Count == 0 && velocity.magnitude > 1f && Vector2.Dot(velocity.normalized, actor.Direction) < -0.3f;

        if (isReversing)
        {
            return Vector2.MoveTowards(velocity, Vector2.zero, acceleration * inertia * Clock.DeltaTime);
        }

        return Vector2.Lerp(targetVelocity, velocity, retention);
    }


    void CalculateMomentum()
    {
        momentum = mass * velocity;
    }

    Vector2 BaseMovementVelocity()
    {
        return Vector2.MoveTowards(velocity, Mathf.Clamp(speed * modifier, 0, maxSpeed) * actor.Direction.Vector, acceleration * Clock.DeltaTime);
    }

    void ApplyFriction()
    {   
        if (actor.Disabled || velocity.magnitude < 0.001f)
            return;
        
        if (applyFriction || (directives.Count == 0 && actor.CanMove))
        {
            velocity *= Mathf.Exp(-friction * Clock.DeltaTime);
        }
    }

    void ApplyVelocity()
    {
        body.MovePosition(body.position + velocity * Clock.DeltaTime);
    }

    // ============================================================================
    // MOVEMENT CONTROLLER
    // ============================================================================

    void RemoveInactiveControllers()
    {   
        directives.RemoveAll(directive => !directive.Controller.Active);
    }

    void RequestMovementDirective(object owner, MovementDefinition definition)
    {
        directives.Add(new() { Owner = owner, Definition = definition, Controller = CreateController(definition)});
    }

    void ClearMovementDirective(object owner, int scope)
    {
        directives.RemoveAll(directive => directive.Owner == owner && directive.Definition.Scope == scope && !directive.Definition.PersistPastScope);
    }

    void ClearAllOwnedDirectives(object owner)
    {
        directives.RemoveAll(directive => directive.Owner == owner && !directive.Definition.PersistPastSource);
    }

    // ============================================================================
    // EVENT HANDLERS
    // =====================================================a=======================
    
    void HandleMovementDirective(Message<Request, MovementEvent> message)
    {
        var owner       = message.Payload.Owner;
        var Definition  = message.Payload.Definition;

        RequestMovementDirective(owner, Definition);
    }


    void HandleMovementClear(Message<Request, ClearMovementScopeEvent> message)
    {
        var payload = message.Payload;

        switch (message.Action, payload)
        {
            case (Request.Clear, { Owner: not null, Scope: not -1 }):
                ClearMovementDirective(payload.Owner, payload.Scope);
                break;

            case (Request.Clear, { Owner: not null }):
                ClearAllOwnedDirectives(payload.Owner);
                break;
        }
    }

    void HandlePresenceStateEvent(Message<Publish, PresenceStateEvent> message)
    {
        switch(message.Payload.State)
        {
            case Presence.State.Entering:
                Enable();
            break;
            case Presence.State.Exiting:
                Disable();
            break;
            case Presence.State.Disposal:
                Dispose();
            break;
        }
    }

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    IMovementController CreateController(MovementDefinition definition)
    {
        return definition.MovementForce switch
        {
            MovementForce.Dynamic   => MovementControllerFactory.CreateDynamic  (owner, definition),
            MovementForce.Kinematic => MovementControllerFactory.CreateKinematic(owner, definition),
            _ => default,
        };
    }

    bool IsDynamic  (MovementDirective directive) => directive.Definition.MovementForce is MovementForce.Dynamic;
    bool IsKinematic(MovementDirective directive) => directive.Definition.MovementForce is MovementForce.Kinematic;

    bool CanApplyVelocity() => owner is IMovable actor && !actor.Disabled;

    bool ActorIsDisabled()  => actor.Disabled;
    bool ActorCanMove()     => actor.CanMove;

    void SetSpeed()         => speed = actor.Speed;
    void SetMass()          => mass  = actor.Mass;

    void ClearMovement()
    {
        ClearMomentum();
        ClearVelocity();
    }

    void ClearMomentum() => momentum = Vector2.zero;
    void ClearVelocity() => velocity = Vector2.zero;

    List<MovementDirective> FilterAndSortDescendingDirectives(Func<MovementDirective, bool> filter)
    {
        return directives.Where(directive => filter(directive)).OrderByDescending(directive => directive.Controller.Priority).ToList();
    }

    public Vector2 Velocity => velocity;
    public Vector2 Momentum => momentum;

    void DebugLog()
    {
        Log.Debug("Movement.Speed",     () => speed);  
        Log.Debug("Movement.Velocity",  () => velocity);
        Log.Debug("Movement.Modifier",  () => modifier);
        Log.Trace("Effect.Active",      () => $"{string.Join(", ", modifierHandler.Cache.Instances.Select(instance => instance.Effect.Name))}");
        Log.Trace("Effect.Cache",       () => modifierHandler.Cache.Instances.Count);
        Log.Trace("Directive.Count",    () => directives.Count);
        Log.Trace("Actor.CanMove",      () => actor.CanMove);
    }

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}


// ============================================================================
// EVENTS
// ============================================================================

public readonly struct MovementEvent
{
    public readonly object Owner                    { get; init; }
    public readonly MovementDefinition Definition   { get; init; }

    public MovementEvent(object owner, MovementDefinition definition)
    {
        Owner       = owner;
        Definition  = definition;
    }
}

public readonly struct ClearMovementScopeEvent
{
    public readonly object Owner                { get; init; }
    public readonly int Scope                   { get; init; }

    public ClearMovementScopeEvent(object owner, int scope)
    {
        Owner   = owner;
        Scope   = scope;
    }
}


// ============================================================================
// MOVEMENT COMMAND FACTORY
// ============================================================================

public static class MovementControllerFactory
{
    
    public static IMovementController CreateKinematic(Actor actor, MovementDefinition definition)
    {
        return (definition.MovementForce, definition.KinematicAction, actor)switch
        {
            (MovementForce.Kinematic, KinematicAction.Dash, IMovableActor movable) =>
                new DashController(definition.InputIntent.Direction, definition.InputIntent.LastDirection, definition.Speed, definition.DurationFrames),

            (MovementForce.Kinematic, KinematicAction.Lunge, IMovableActor movable) => 
                new LungeController(definition.InputIntent.Aim, definition.Speed, definition.DurationFrames, definition.SpeedCurve),

            _ => null
        };
    }

    public static IMovementController CreateDynamic(Actor actor, MovementDefinition definition)
    {
        return (definition.MovementForce, definition.DynamicSource, actor)switch
        {
            (MovementForce.Kinematic, DynamicSource.Collision, IMovableActor movable) =>
                new DynamicForceController(definition.Force, definition.Mass),
            _ => null
        };
    }

}

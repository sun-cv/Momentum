using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class MovementEngine : Service, IServiceTick
{
    readonly float maxSpeed         = Settings.Movement.MAX_SPEED;
    readonly float acceleration     = Settings.Movement.ACCELERATION;
    readonly float friction         = Settings.Movement.FRICTION;
    readonly float retention        = Settings.Movement.MOMENTUM_RETENTION;
    readonly float inertia          = Settings.Movement.INERTIA;
    readonly float forceThreshold   = Settings.Movement.FORCE_THRESHOLD;

        // -----------------------------------

    readonly Agent          owner;
    readonly IMovableActor  agent;
    readonly Rigidbody2D    body;

        // -----------------------------------

    MovementModifierHandler modifierHandler;

        // -----------------------------------

    readonly bool applyFriction = true;

    float modifier;
    float speed;
    float mass;

    Vector2 momentum            = Vector2.zero;
    Vector2 velocity            = Vector2.zero;

    List<MovementDirective> directives = new();

    // ===============================================================================

    public MovementEngine(Agent agent)
    {        
        Services.Lane.Register(this);

        this.owner          = agent;
        this.body           = agent.Bridge.Body;
        this.agent          = agent as IMovableActor;

        body.freezeRotation = true;
        body.gravityScale   = 0;
        body.interpolation  = RigidbodyInterpolation2D.Interpolate; 

        modifierHandler     = new(agent);

        owner.Emit.Link.Local<Message<Request, MovementEvent>>          (HandleMovementDirective);
        owner.Emit.Link.Local<Message<Request, ClearMovementScopeEvent>>(HandleMovementClear);
        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>     (HandlePresenceStateEvent);

        SetSpeed();
        SetMass();
    }

    // ===============================================================================

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

    // ===============================================================================

        // ===================================
        //  Calculations
        // ===================================

    void CalculateModifier() => modifier = modifierHandler.Calculate();

    void CalculateVelocity()
    {
        if (AgentIsDisabled())
        {
            ClearMovement();
            return;
        }

        Vector2 kinematicVelocity   = CalculateKinematicForces();
        Vector2 dynamicVelocity     = CalculateDynamicForces();

        velocity                    = ApplyVelocityTransition(CombineMovementForces(kinematicVelocity, dynamicVelocity));
    }

    void CalculateMomentum()
    {
        momentum = mass * velocity;
    }

    Vector2 CalculateKinematicForces()
    {
        Vector2 targetVelocity = Vector2.zero;

        if (AgentCanMove())
        {
            targetVelocity = BaseMovementVelocity();
        }

        foreach(var directive in FilterAndSortDescendingDirectives(ForceIsKinematic))
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

        foreach(var directive in FilterAndSortDescendingDirectives(ForceIsDynamic))
        {
            if (directive.Controller.Mode == ControllerMode.Additive)
            {
                forceVelocity += directive.Controller.CalculateVelocity(owner);
            }
        }

        return forceVelocity;
    }

        // ===================================
        //  Execution
        // ===================================
    
    Vector2 BaseMovementVelocity()
    {
        return Vector2.MoveTowards(velocity, Mathf.Clamp(speed * modifier, 0, maxSpeed) * agent.Direction.Vector, acceleration * Clock.DeltaTime);
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
        bool isReversing = agent.CanMove && directives.Count == 0 && velocity.magnitude > 1f && Vector2.Dot(velocity.normalized, agent.Direction) < -0.3f;

        if (isReversing)
        {
            return Vector2.MoveTowards(velocity, Vector2.zero, acceleration * inertia * Clock.DeltaTime);
        }

        return Vector2.Lerp(targetVelocity, velocity, retention);
    }

    void ApplyFriction()
    {   
        if (agent.Disabled || velocity.magnitude < 0.001f)
            return;
        
        if (applyFriction || (directives.Count == 0 && agent.CanMove))
        {
            velocity *= Mathf.Exp(-friction * Clock.DeltaTime);
        }
    }

    void ApplyVelocity()
    {
        body.MovePosition(body.position + velocity * Clock.DeltaTime);
    }

        // ===================================
        //  Registration
        // ===================================

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

    // ===============================================================================
    //  Events
    // ===============================================================================
    
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

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool CanApplyVelocity() => owner is IMovable agent && !agent.Disabled;

    bool AgentIsDisabled()  => agent.Disabled;
    bool AgentCanMove()     => agent.CanMove;

    bool ForceIsDynamic  (MovementDirective directive) => directive.Definition.MovementForce is MovementForce.Dynamic;
    bool ForceIsKinematic(MovementDirective directive) => directive.Definition.MovementForce is MovementForce.Kinematic;

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void SetSpeed() => speed = agent.Speed;
    void SetMass()  => mass  = agent.Mass;

    void ClearMovement()
    {
        ClearMomentum();
        ClearVelocity();
    }

    void ClearMomentum() => momentum = Vector2.zero;
    void ClearVelocity() => velocity = Vector2.zero;

    IMovementController CreateController(MovementDefinition definition)
    {
        return definition.MovementForce switch
        {
            MovementForce.Dynamic   => MovementControllerFactory.CreateDynamic  (owner, definition),
            MovementForce.Kinematic => MovementControllerFactory.CreateKinematic(owner, definition),
            _ => default,
        };
    }

    List<MovementDirective> FilterAndSortDescendingDirectives(Func<MovementDirective, bool> filter)
    {
        return directives.Where(directive => filter(directive)).OrderByDescending(directive => directive.Controller.Priority).ToList();
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Movement);

    void DebugLog()
    {
        Log.Debug("Movement.Speed",     () => speed);  
        Log.Debug("Movement.Velocity",  () => velocity);
        Log.Debug("Movement.Modifier",  () => modifier);
        Log.Trace("Effect.Active",      () => $"{string.Join(", ", modifierHandler.Cache.Instances.Select(instance => instance.Effect.Name))}");
        Log.Trace("Effect.Cache",       () => modifierHandler.Cache.Instances.Count);
        Log.Trace("Directive.Count",    () => directives.Count);
        Log.Trace("agent.CanMove",      () => agent.CanMove);
    }

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public Vector2 Velocity => velocity;
    public Vector2 Momentum => momentum;

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

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


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Factories
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public static class MovementControllerFactory
{
    
    public static IMovementController CreateKinematic(Agent agent, MovementDefinition definition)
    {
        return (definition.MovementForce, definition.KinematicAction, agent)switch
        {
            (MovementForce.Kinematic, KinematicAction.Dash, IMovableActor movable) =>
                new DashController(definition.InputIntent.Direction, definition.InputIntent.LastDirection, definition.Speed, definition.DurationFrames),

            (MovementForce.Kinematic, KinematicAction.Lunge, IMovableActor movable) => 
                new LungeController(definition.InputIntent.Aim, definition.Speed, definition.DurationFrames, definition.SpeedCurve),

            _ => null
        };
    }

    public static IMovementController CreateDynamic(Agent agent, MovementDefinition definition)
    {
        return (definition.MovementForce, definition.DynamicSource, agent)switch
        {
            (MovementForce.Kinematic, DynamicSource.Collision, IMovableActor movable) =>
                new DynamicForceController(definition.Force, definition.Mass),
            _ => null
        };
    }

}

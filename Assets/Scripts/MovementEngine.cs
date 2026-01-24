using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;






public class MovementEngine : IServiceTick
{

    readonly float maxSpeed     = Settings.Movement.MAX_SPEED;
    readonly float acceleration = Settings.Movement.ACCELERATION;
    readonly float friction     = Settings.Movement.FRICTION;
    readonly float retention    = Settings.Movement.MOMENTUM_RETENTION;
    readonly float inertia      = Settings.Movement.INERTIA;

    MovementModifierHandler modifierHandler = new();

    Actor           owner;
    IMovableActor   actor;
    Rigidbody2D     body;

    float speed;
    float modifier;

    Vector2 momentum;
    Vector2 velocity;

    List<MovementDirective> directives = new();


    public MovementEngine(Actor actor)
    {
        if (actor.Bridge is not ActorBridge bridge)
        {
            Log.Error(LogSystem.Movement, LogCategory.Activation, () => $"Movement Engine activation requires Actor Bridge (actor {actor.RuntimeID} failed)");
            return;
        }

        GameTick.Register(this);
        
        this.owner          = actor;
        this.body           = bridge.Body;
        this.actor          = actor as IMovableActor;

        body.freezeRotation = true;
        body.gravityScale   = 0;
        body.interpolation  = RigidbodyInterpolation2D.Interpolate; 

        EventBus<MovementRequest>.Subscribe(HandleMovementRequest);

        SetSpeed();
    }


    public void Tick()
    {
        RemoveInactiveControllers();


        CalculateModifier();
        CalculateVelocity();

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
        if (actor.Disabled)
        {
            momentum = Vector2.zero;
            velocity = Vector2.zero;
            return;
        }
    
        bool isReversing = actor.CanMove && 
                           velocity.magnitude > 1f && 
                           Vector2.Dot(velocity.normalized, actor.Direction) < -0.3f;
    
        Vector2 targetVelocity = actor.CanMove ? BaseMovementVelocity() : Vector2.zero;
    
        var sortedDirectives = directives.OrderByDescending(d => d.Controller.Priority).ToList();
    
        foreach(var directive in sortedDirectives)
        {
            Vector2 controllerVelocity = directive.Controller.CalculateVelocity(owner);
    
            switch(directive.Controller.InputMode)
            {
                case ControllerInputMode.Ignore:
                    targetVelocity = controllerVelocity;
                    goto FinishedBlending;
    
                case ControllerInputMode.Blend:
                    targetVelocity += controllerVelocity * directive.Controller.Weight;
                    break;
    
                case ControllerInputMode.AllowOverride:
                    targetVelocity = Vector2.Lerp(targetVelocity, controllerVelocity, directive.Controller.Weight);
                    break;
            }
        }
    
        FinishedBlending:
        
        if (isReversing && directives.Count == 0)
        {
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, acceleration * inertia * Clock.DeltaTime);
        }
        else
        {
            velocity = Vector2.Lerp(targetVelocity, velocity, retention);
        }
        
        momentum = velocity;
    }
    Vector2 BaseMovementVelocity()
    {
        return Vector2.MoveTowards(velocity, Mathf.Clamp(speed * modifier, 0, maxSpeed) * actor.Direction.Vector, acceleration * Clock.DeltaTime);
    }

    void ApplyFriction() => velocity *= 1 - Mathf.Clamp01(friction * Clock.DeltaTime);
    void ApplyVelocity() => body.MovePosition(body.position + velocity * Time.fixedDeltaTime);

    // ============================================================================
    // MOVEMENT CONTROLLER
    // ============================================================================

    void RemoveInactiveControllers()
    {   
        directives.RemoveAll(directive => !directive.Controller.IsActive);
    }

    void RequestMovementDirective(object owner, int scope, MovementCommand command)
    {
        directives.Add(new() { Owner = owner, Scope = scope, Definition = command.GetDefinition(), Controller = command.CreateController()});
    }

    void ClearMovementDirective(object owner, int scope)
    {
        directives.RemoveAll(directive => directive.Owner == owner && directive.Scope == scope && !directive.Definition.PersistPastScope);
    }

    void ClearAllOwnedDirectives(object owner)
    {
        directives.RemoveAll(directive => directive.Owner == owner && !directive.Definition.PersistPastSource);
    }

    // ============================================================================
    // EVENTS
    // =====================================================a=======================
    
    void HandleMovementRequest(MovementRequest evt)
    {
        var payload = evt.Payload;

        switch (evt.Action, payload)
        {
            case (Request.Create, { Owner: not null, Command: not null }):
                RequestMovementDirective(payload.Owner, payload.Scope, payload.Command);
                break;

            case (Request.Clear, { Owner: not null, Scope: not -1 }):
                ClearMovementDirective(payload.Owner, payload.Scope);
                break;

            case (Request.Clear, { Owner: not null }):
                ClearAllOwnedDirectives(payload.Owner);
                break;
        }
    }

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    public Vector2 Velocity => velocity;
    public Vector2 Momentum => momentum;

    void SetSpeed()         => speed = actor.Speed;
    bool CanApplyVelocity() => owner is IMovable actor && !actor.Disabled;

    void DebugLog()
    {
        Log.Debug(LogSystem.Movement, LogCategory.Control,"Movement", "Movement.Speed",     () => speed);  
        Log.Debug(LogSystem.Movement, LogCategory.State,  "Movement", "Movement.Velocity",  () => velocity);
        Log.Debug(LogSystem.Movement, LogCategory.State,  "Movement", "Movement.Modifier",  () => modifier);
        Log.Trace(LogSystem.Movement, LogCategory.Effect, "Movement", "Effect.Active",      () => $"{string.Join(", ", modifierHandler.Cache.Effects.Select(effect => effect.Effect.Name))}");
        Log.Trace(LogSystem.Movement, LogCategory.Effect, "Movement", "Effect.Cache",       () => modifierHandler.Cache.Effects.Count);
        Log.Trace(LogSystem.Movement, LogCategory.State,  "Movement", "Directive.Count",    () => directives.Count);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}


// ============================================================================
// EVENTS
// ============================================================================

public readonly struct MovementRequestPayload
{
    public readonly object Owner                { get; init; }
    public readonly int Scope                   { get; init; }
    public readonly MovementCommand Command     { get; init; }
}

public readonly struct MovementRequest : ISystemEvent
{
    public Guid Id                              { get; }
    public Request Action                       { get; }
    public MovementRequestPayload Payload       { get; }

    public MovementRequest(Guid id, Request action, MovementRequestPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}

// ============================================================================
// MOVEMENT COMMAND FACTORY
// ============================================================================

public static class MovementCommandFactory
{
    
    public static MovementCommand Create(Actor actor, MovementCommandDefinition definition, InputIntentSnapshot inputIntent)
    {
        return (definition.Action, actor) switch
        {
            (MovementAction.Dash, IMovableActor movable) =>
                new DashMovementCommand
                {
                    Definition      = definition,
                    InputIntent     = inputIntent,
                },

            (MovementAction.Lunge, IMovableActor movable and IAimable aim) =>
                new LungeMovementCommand
                {
                    Definition      = definition,
                    InputIntent     = inputIntent,
                },

            _ => null
        };
    }
}

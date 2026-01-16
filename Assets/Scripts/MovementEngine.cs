using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;






public class MovementEngine : IServiceTick
{

    readonly float maxSpeed     = Settings.Movement.MAX_SPEED;
    readonly float acceleration = Settings.Movement.ACCELERATION;
    readonly float friction     = Settings.Movement.FRICTION;

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

        if (CanMove())
            ApplyVelocity();

        DebugLog();
    }

    // ============================================================================
    // MOVEMENT CALCULATIONS
    // ============================================================================

    void CalculateModifier() => modifier = modifierHandler.Calculate();

void CalculateVelocity()
{
    Vector2 velocity = BaseMovementVelocity();
    
    var sortedDirectives = directives
        .OrderByDescending(d => d.Controller.Priority)
        .ToList();
    
    foreach(var directive in sortedDirectives)
    {
        Vector2 controllerVelocity = directive.Controller.CalculateVelocity(owner);

        switch(directive.Controller.InputMode)
        {
            case ControllerInputMode.Ignore:
                velocity = controllerVelocity;
                goto FinishedBlending;
                
            case ControllerInputMode.Blend:
                velocity += controllerVelocity * directive.Controller.Weight;
                break;
                
            case ControllerInputMode.AllowOverride:
                velocity = Vector2.Lerp(velocity, controllerVelocity, directive.Controller.Weight);
                break;
        }
    }
    
    FinishedBlending:
    this.momentum = velocity;
    this.velocity = velocity;
}

    Vector2 BaseMovementVelocity()
    {
        return Vector2.MoveTowards(velocity, Mathf.Clamp(speed * modifier, 0, maxSpeed) * actor.Direction, acceleration * Clock.DeltaTime);
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
        directives.Add(new() { Owner = owner, Scope = scope, Intent = command.GetIntent(), Controller = command.CreateController()});
    }

    void ClearMovementDirective(object owner, int scope)
    {
        directives.RemoveAll(directive => directive.Owner == owner && directive.Scope == scope &&!directive.Intent.PersistPastScope);
    }

    void ClearAllOwnedDirectives(object owner)
    {
        directives.RemoveAll(directive => directive.Owner == owner);
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

    void SetSpeed() => speed = actor.Speed;
    bool CanMove()  => owner is IMovable actor && actor.CanMove;

    void DebugLog()
    {
        Log.Debug(LogSystem.Movement, LogCategory.Control,"Movement", "Movement.Speed",     () => speed);  
        Log.Debug(LogSystem.Movement, LogCategory.State,  "Movement", "Movement.Velocity",  () => velocity);
        Log.Debug(LogSystem.Movement, LogCategory.State,  "Movement", "Movement.Modifier",  () => modifier);
        Log.Trace(LogSystem.Movement, LogCategory.Effect, "Movement", "Effect.Cache",       () => modifierHandler.Cache.Effects.Count);
        Log.Debug(LogSystem.Movement, LogCategory.Effect, "Movement", "Effect.Active",      () => $"{string.Join(", ", modifierHandler.Cache.Effects.Select(effect => effect.Effect.Name))}");
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
    
    public static MovementCommand Create(MovementCommandIntent intent, Actor actor)
    {
        return (intent.Action, actor) switch
        {
            (MovementAction.Dash, IMovableActor movable) =>
                new DashMovementCommand
                {
                    Actor  = movable,
                    Intent = intent,
                },

            (MovementAction.Lunge, IMovableActor movable and IHasAim aim) =>
                new LungeMovementCommand
                {
                    Actor  = aim,
                    Intent = intent,
                },

            _ => null
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;






public class MovementEngine : IServiceTick
{
    readonly Logger Log = Logging.For(LogSystem.Movement);

    readonly float maxSpeed     = Settings.Movement.MAX_SPEED;
    readonly float acceleration = Settings.Movement.ACCELERATION;
    readonly float friction     = Settings.Movement.FRICTION;
    readonly float retention    = Settings.Movement.MOMENTUM_RETENTION;
    readonly float inertia      = Settings.Movement.INERTIA;

    MovementModifierHandler modifierHandler;

    Actor           owner;
    IMovableActor   actor;
    Rigidbody2D     body;

    float speed;
    float modifier;

    Vector2 momentum;
    Vector2 velocity;

    bool wasInterruptActive = false;


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

        owner.Emit.Link.Local<Message<Request, MMovementDirective>> (HandleMovementDirective);
        owner.Emit.Link.Local<Message<Request, MClearMovement>>     (HandleMovementClear);

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

        Vector2 targetVelocity  = actor.CanMove ? BaseMovementVelocity() : Vector2.zero;
        var sortedDirectives    = directives.OrderByDescending(d => d.Controller.Priority).ToList();
        bool isReversing        = actor.CanMove && velocity.magnitude > 1f && Vector2.Dot(velocity.normalized, actor.Direction) < -0.3f;

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
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, acceleration * inertia * Clock.DeltaTime);
        else
            velocity = Vector2.Lerp(targetVelocity, velocity, retention);

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
    
    void HandleMovementDirective(Message<Request, MMovementDirective> message)
    {
        var owner   = message.Payload.Owner;
        var scope   = message.Payload.Scope;
        var command = message.Payload.Command;

        RequestMovementDirective(owner, scope, command);
    }


    void HandleMovementClear(Message<Request, MClearMovement> message)
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

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    public Vector2 Velocity => velocity;
    public Vector2 Momentum => momentum;

    void SetSpeed()         => speed = actor.Speed;
    bool CanApplyVelocity() => owner is IMovable actor && !actor.Disabled;

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

    public UpdatePriority Priority => ServiceUpdatePriority.MovementEngine;
}


// ============================================================================
// EVENTS
// ============================================================================

public readonly struct MMovementDirective
{
    public readonly object Owner                { get; init; }
    public readonly int Scope                   { get; init; }
    public readonly MovementCommand Command     { get; init; }

    public MMovementDirective(object owner, int scope, MovementCommand command)
    {
        Owner   = owner;
        Scope   = scope;
        Command = command;
    }
}

public readonly struct MClearMovement
{
    public readonly object Owner                { get; init; }
    public readonly int Scope                   { get; init; }

    public MClearMovement(object owner, int scope)
    {
        Owner   = owner;
        Scope   = scope;
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

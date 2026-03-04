using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Movement : Service, IServiceTick
{
    readonly float maxSpeed         = Settings.Movement.MAX_SPEED;
    readonly float acceleration     = Settings.Movement.ACCELERATION;

        // -----------------------------------

    readonly Agent          owner;
    readonly IMovableActor  movable;
    readonly Rigidbody2D    body;

        // -----------------------------------

    readonly MovementModifierHandler modifierHandler;

        // -----------------------------------

    float modifier;
    float speed;
    float mass;

    Vector2 momentum            = Vector2.zero;
    Vector2 control             = Vector2.zero;

    readonly List<MovementDirective> directives = new();

    // ===============================================================================

    public Movement(Agent agent)
    {        
        Services.Lane.Register(this);

        owner               = agent;
        body                = agent.Bridge.Body;
        movable             = agent as IMovableActor;

        body.freezeRotation = true;
        body.gravityScale   = 0;
        body.interpolation  = RigidbodyInterpolation2D.Interpolate; 
        body.mass           = 1;

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

        ApplyControlVelocity();

        DebugLog();
    }

    // ===============================================================================

        // ===================================
        //  Calculations
        // ===================================

    void CalculateModifier()
    {
        modifier = modifierHandler.Calculate();
    }

    void CalculateVelocity()
    {
        if (AgentIsDisabled())
        {
            ClearMovement();
            return;
        }

        control = CalculateKinematicForces();
    }
    void CalculateMomentum()
    {
        momentum = mass * control;
    }

    Vector2 CalculateKinematicForces()
    {
        Vector2 targetVelocity = Vector2.zero;

        if (AgentCanMove())
        {
            targetVelocity = BaseMovementVelocity();
        }

        foreach(var directive in directives)
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

        // ===================================
        //  Execution
        // ===================================
    
    Vector2 BaseMovementVelocity()
    {
        return Vector2.MoveTowards(control, Mathf.Clamp(speed * modifier, 0, maxSpeed) * movable.Direction.Vector, acceleration * Clock.DeltaTime);
    }

    void ApplyControlVelocity()
    {
        movable.Control = control;
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

    bool AgentIsDisabled()  => movable.Disabled;
    bool AgentCanMove()     => movable.CanMove;

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void SetSpeed() => speed = movable.Speed;
    void SetMass()  => mass  = movable.Mass;

    void ClearMovement()
    {
        ClearMomentum();
        ClearVelocity();
    }

    void ClearMomentum() => momentum = Vector2.zero;
    void ClearVelocity() => control  = Vector2.zero;

    IMovementController CreateController(MovementDefinition definition)
    {
        return MovementControllerFactory.CreateController(owner, definition);
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Movement);

    void DebugLog()
    {
        Log.Debug("Movement.Speed",     () => speed);  
        Log.Debug("Movement.Control",   () => control);
        Log.Debug("Movement.Modifier",  () => modifier);
        Log.Trace("Effect.Active",      () => $"{string.Join(", ", modifierHandler.Cache.Instances.Select(instance => instance.Effect.Name))}");
        Log.Trace("Effect.Cache",       () => modifierHandler.Cache.Instances.Count);
        Log.Trace("Directive.Count",    () => directives.Count);
        Log.Trace("agent.CanMove",      () => movable.CanMove);
    }

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public Vector2 Control  => control;
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
    public static IMovementController CreateController(Agent agent, MovementDefinition definition)
    {
        return (definition.KinematicAction, agent)switch
        {
            (KinematicAction.Dash, IMovableActor movable) =>
                new DashController(definition.InputIntent.Direction, definition.InputIntent.LastDirection, definition.Speed, definition.DurationFrames),

            (KinematicAction.Lunge, IMovableActor movable) => 
                new LungeController(definition.InputIntent.Aim, definition.Speed, definition.DurationFrames, definition.SpeedCurve),

            _ => null
        };
    }
}

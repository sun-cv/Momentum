using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Movement : ActorService, IServiceTick
{
    readonly float maxSpeed         = Settings.Movement.MAX_SPEED;
    readonly float acceleration     = Settings.Movement.ACCELERATION;

        // -----------------------------------
    readonly Agent          agent;
    readonly IMovableActor  movable;
    readonly Rigidbody2D    body;

        // -----------------------------------

    readonly MovementModifierHandler modifierHandler;

        // -----------------------------------

    readonly List<MovementDirective> directives = new();
    readonly List<Guid> movementLocks           = new();

        // -----------------------------------

    float mass;
    float speed;
    float modifier;

    Vector2 control             = Vector2.zero;
    Vector2 momentum            = Vector2.zero;


    // ===============================================================================

    public Movement(Agent agent) : base(agent)
    {        
        this.agent          = agent;
        body                = this.agent.Bridge.Body;
        movable             = this.agent as IMovableActor;

        body.freezeRotation = true;
        body.gravityScale   = 0;
        body.interpolation  = RigidbodyInterpolation2D.Interpolate; 
        body.mass           = 1;

        modifierHandler     = new(this.agent);

        owner.Bus.Link.Local<MovementEvent>     (HandleMovementDirective);
        owner.Bus.Link.Local<ClearMovementEvent>(HandleMovementClear);

        SetSpeed();
        SetMass();
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessDirectives();
        ProcessMovementLocks();

        CalculateModifier();
        CalculateVelocity();
        CalculateMomentum();

        ApplyControl();

        if (owner is Hero) DebugLog();
    }

    // ===============================================================================

    void ProcessDirectives()
    {
        var expired = AllDisabledDirectives();

        foreach (var directive in expired)
        {
            directive.Controller.Exit(this);
            directives.Remove(directive);
        }
    }

    void ProcessMovementLocks()
    {

        
        if (!movable.LockMovement && movementLocks.Count > 0)
        {
            movable.LockMovement    = true;
            return;
        }

        if (movable.LockMovement && movementLocks.Count == 0)
        {
            movable.LockMovement    = false;
            return;        
        }
    }

    void AddDirective(object source, IntentSnapshot intent, MovementDefinition definition)
    {
        var controller = CreateController(intent, definition);

        if (controller == null)
            return;

        controller.Enter(this);
        
        directives.Add(new() { Owner = source, Definition = definition, Controller = controller });
    }
    void RemoveDirective(MovementDefinition definition)
    {
        int index = directives.FindIndex(directive => directive.Definition == definition);

        if (index < 0)
            return;

        directives[index].Controller.Exit(this);
        directives.RemoveAt(index);
    }

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
            control = Vector2.zero;
            return;
        }

        control = ProcessControllers();
    }

    void CalculateMomentum()
    {
        momentum = mass * control;
    }

    Vector2 ProcessControllers()
    {
        Vector2 result = AgentCanMove() ? BaseMovementVelocity() : Vector2.zero;

        foreach (var directive in directives)
        {
            var velocity = directive.Controller.Process(this);

            switch (directive.Controller.Mode)
            {
                case ControllerMode.Ignore:
                    result = velocity;
                    goto Done;

                case ControllerMode.Blend:
                    result += velocity * directive.Controller.Weight;
                    break;

                case ControllerMode.AllowOverride:
                    result = Vector2.Lerp(result, velocity, directive.Controller.Weight);
                    break;
            }
        }

        Done:
        return result;
    }

    void AddMovementLock(Instance owner, MovementDefinition definition)
    {
        if (definition.LockMovement)
            movementLocks.Add(owner.RuntimeId);
    }

    void RemoveMovementLock(Instance owner, MovementDefinition definition)
    {
        if (definition.LockMovement)
           movementLocks.Remove(owner.RuntimeId); 
    }

        // ===================================
        //  Execution
        // ===================================
    
    Vector2 BaseMovementVelocity()
    {
        return Vector2.MoveTowards(control, Mathf.Clamp(speed * modifier, 0, maxSpeed) * movable.Direction.Vector, acceleration * Clock.DeltaTime);
    }

    void ApplyControl()
    {
        movable.Control = control;
    }


    public void SetControlSpeed(float speed)
    {
        control = control.normalized * Mathf.Min(control.magnitude, speed);
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
    

    void HandleMovementDirective(MovementEvent message)
    {
        AddDirective(message.Owner, message.Intent, message.Definition);
        AddMovementLock(message.Owner, message.Definition);
    }

    void HandleMovementClear(ClearMovementEvent message)
    {
        RemoveDirective(message.Definition);
        RemoveMovementLock(message.Owner, message.Definition);
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool AgentIsDisabled()  => movable.Disabled;
    bool AgentCanMove()     => movable.CanMove;

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void SetSpeed()         => speed = movable.Speed;
    void SetMass()          => mass  = movable.Mass;


    List<MovementDirective> AllDisabledDirectives() => directives.Where(directive => !directive.Controller.Active).ToList();

    IMovementController CreateController(IntentSnapshot intent, MovementDefinition definition)
    {
        return MovementControllerFactory.CreateController(agent, intent, definition);
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Movement);

    void DebugLog()
    {
        Log.Debug("Movement.Control",   () => control);
        Log.Debug("Movement.Modifier",  () => modifier);
        Log.Debug("Movement.Locked",    () =>  $"{!movable.CanMove}");
        Log.Trace("Effect.Active",      () => $"{string.Join(", ", modifierHandler.Cache.Instances.Select(instance => instance.Effect.Name))}");
        Log.Trace("Effect.Cache",       () => modifierHandler.Cache.Instances.Count);
        Log.Trace("Directive.Count",    () => directives.Count);
        Log.Trace("agent.CanMove",      () => movable.CanMove);
        Log.Trace("agent.CanRotate",    () => movable.CanRotate);
    }

    public Agent Agent      => agent;
    public Vector2 Control  => control;
    public Vector2 Momentum => momentum;

    public UpdatePriority Priority => ServiceUpdatePriority.Movement;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct MovementEvent : IMessage
{
    public readonly Instance Owner                  { get; init; }
    public readonly MovementDefinition Definition   { get; init; }
    public readonly IntentSnapshot Intent           { get; init; }

    public MovementEvent(Instance owner, MovementDefinition definition, IntentSnapshot intent)
    {
        Owner       = owner;
        Definition  = definition;
        Intent      = intent;
    }
}

public readonly struct ClearMovementEvent : IMessage
{
    public readonly Instance Owner                  { get; init; }
    public readonly MovementDefinition Definition   { get; init; }

    public ClearMovementEvent(Instance owner, MovementDefinition definition)
    {
        Owner       = owner;
        Definition  = definition;        
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Factories
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public static class MovementControllerFactory
{
    public static IMovementController CreateController(Agent agent, IntentSnapshot intent, MovementDefinition definition)
    {
        return (definition.KinematicAction, agent)switch
        {
            (KinematicAction.Dash, IMovableActor) =>
                new DashController(intent, definition),

            (KinematicAction.Lunge, IMovableActor) => 
                new LungeController(intent, definition),

            _ => null
        };
    }
}

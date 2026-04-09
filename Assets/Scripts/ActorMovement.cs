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

        owner.Bus.Link.Local<MovementEvent>          (HandleMovementDirective);
        owner.Bus.Link.Local<ClearMovementScopeEvent>(HandleMovementClear);

        SetSpeed();
        SetMass();
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessDirectives();

        CalculateModifier();
        CalculateVelocity();
        CalculateMomentum();

        ApplyControl();

        if (owner is Hero) DebugLog();
    }

    // ===============================================================================


    void AddDirective(object source, MovementDefinition definition)
    {
        var controller = CreateController(definition);

        if (controller == null)
            return;

        controller.Enter(this);
        
        directives.Add(new() { Owner = source, Definition = definition, Controller = controller });
    }

    void ProcessDirectives()
    {
        var expired = AllDisabledDirectives();

        foreach (var directive in expired)
        {
            directive.Controller.Exit(this);
            directives.Remove(directive);
        }
    }

    void RemoveDirective(object source, int scope)
    {
        var expired = AllScopeDirectives(source, scope);

        foreach (var directive in expired)
        {
            directive.Controller.Exit(this);
            directives.Remove(directive);
        }
    }

    void RemoveAllDirectives(object source)
    {
        var expired = AllSourceDirectives(source);

        foreach (var directive in expired)
        {
            directive.Controller.Exit(this);
            directives.Remove(directive);
        }
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
        AddDirective(message.Owner, message.Definition);
    }

    void HandleMovementClear(ClearMovementScopeEvent message)
    {;
        switch (message.Type, message)
        {
            case (Request.Clear, { Owner: not null, Scope: not -1 }):
                RemoveDirective(message.Owner, message.Scope);
                break;

            case (Request.Clear, { Owner: not null }):
                RemoveAllDirectives(message.Owner);
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


    List<MovementDirective> AllDisabledDirectives()                         => directives.Where(directive => !directive.Controller.Active).ToList();
    List<MovementDirective> AllScopeDirectives(object source, int scope)    => directives.Where(directive => directive.Owner == source && directive.Definition.Scope == scope && !directive.Definition.PersistPastScope).ToList();
    List<MovementDirective> AllSourceDirectives(object source)              => directives.Where(directive => directive.Owner == source && !directive.Definition.PersistPastSource).ToList();

    IMovementController CreateController(MovementDefinition definition)
    {
        return MovementControllerFactory.CreateController(agent, definition);
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Movement);

    void DebugLog()
    {
        Log.Debug("Movement.Control",   () => control);
        Log.Debug("Movement.Modifier",  () => modifier);
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
    public readonly object Owner                    { get; init; }
    public readonly MovementDefinition Definition   { get; init; }

    public MovementEvent(object owner, MovementDefinition definition)
    {
        Owner       = owner;
        Definition  = definition;
    }
}

public readonly struct ClearMovementScopeEvent : IMessage
{
    public readonly object Owner                { get; init; }
    public readonly int Scope                   { get; init; }
    public readonly Request Type                { get; init; }

    public ClearMovementScopeEvent(Request type, object owner, int scope)
    {
        Owner   = owner;
        Scope   = scope;
        Type    = type;
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
            (KinematicAction.Dash, IMovableActor) =>
                new DashController(definition),

            (KinematicAction.Lunge, IMovableActor) => 
                new LungeController(definition),

            _ => null
        };
    }
}

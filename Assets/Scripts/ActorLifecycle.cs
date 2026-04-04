using System.Collections.Generic;
using UnityEngine;



public class Lifecycle : ActorService, IServiceLoop
{

    public enum State { Alive, Dying, Dead, Disposal }

    // ===============================================================================

    readonly Dictionary<State, IStateHandler> stateHandlers = new();

        // -----------------------------------
    LifecycleContext context;
    State condition;

    // ===============================================================================

    public Lifecycle(Actor actor) : base(actor)
    {
        if (actor is not IMortal)
            return;

        CreateContext();
        InitializeStateHandlers();
        EnterLifecycle();

        Enable();
    }

    void InitializeStateHandlers()
    {

        Register(State.Alive,    new LifecycleAliveState(this, owner, definition));
        Register(State.Dying,    new LifecycleDyingState(this, owner, definition));
        Register(State.Dead,     new LifecycleDeadState (this, owner, definition));
    }

    // ===============================================================================

    public void Loop()
    {
        UpdateHandler();
    }

    // ===============================================================================

    void UpdateHandler()
    {
        if (stateHandlers.TryGetValue(condition, out var handler))
            handler.Update();
    }

        // ===================================
        //  State
        // ===================================

    public void TransitionTo(State newState)
    {
        ExitHandler();
        TransitionState(newState);
        EnterHandler();
    }

    void ExitHandler()
    {
        if (stateHandlers.TryGetValue(condition, out var handler))
            handler.Exit();
    }

    void TransitionState(State newState)
    {
        condition = newState;
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(condition, out var handler))
            handler.Enter();

        PublishState();
    }

    void EnterLifecycle()
    {
        if (stateHandlers.TryGetValue(State.Alive, out var handler))
            handler.Enter();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void PublishState()
    {
        owner.Bus.Emit.Local(new LifecycleEvent(owner, condition));
    }
    
    // ===============================================================================

    void Register(State state, IStateHandler handler)
    {
        stateHandlers[state] = handler;
    }

    void CreateContext()
    {
        context = new();
        context.Corpse.Actor = owner;
    }

    // ===============================================================================

    // readonly Logger Log = Logging.For(LogSystem.Lifecycle);

    public IMortal Mortal               => owner as IMortal;
    public bool IsAlive                 => condition == State.Alive;
    public bool IsDead                  => condition == State.Dead;
    public LifecycleContext Context     => context;

    public UpdatePriority Priority      => ServiceUpdatePriority.Lifecycle;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Classes                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class LifecycleContext
{
    public CorpseContext Corpse             { get; set; } = new();
    public bool KillingBlowReceived         { get; set; }

    public void Reset()
    {
        Corpse              = new();
        KillingBlowReceived = false;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Alive state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleAliveState : IStateHandler
{
    readonly Actor owner;
    readonly IMortal mortal;
    readonly Lifecycle controller;

    // ===============================================================================

    public LifecycleAliveState(Lifecycle controller, Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.mortal     = owner as IMortal;
        this.controller = controller;

        this.owner.Bus.Link.Local<KillingBlow>(HandleKillingBlow);
    }

    // ===============================================================================

    public void Enter()
    {

    }
    
    public void Update()
    {
        if (controller.Context.KillingBlowReceived)
            controller.TransitionTo(Lifecycle.State.Dying);
    }
    
    public void Exit()
    {
    }

    // ===============================================================================

    void HandleKillingBlow(KillingBlow message)
    {
        controller.Context.Corpse.KillingBlow   = message;
        controller.Context.KillingBlowReceived  = true;
    }

    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Alive;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Dying state                                       
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleDyingState : IStateHandler
{
    readonly Actor              owner;
    readonly IMortal            mortal;
    readonly ActorDefinition    definition;
    readonly Lifecycle          controller;
        // -----------------------------------

    readonly LocalEventHandler<Message<Response, AnimationAPI>> animationRequestHandler;

    // ===============================================================================

    public LifecycleDyingState(Lifecycle controller, Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.mortal     = owner as IMortal;
        this.definition = definition;
        this.controller = controller;

        animationRequestHandler = new(owner.Bus, HandleAnimationAPI);

        owner.Bus.Link.Local<AnimatorEvent>(HandleAnimatorEvent);
    }
    
    // ===============================================================================
    
    public void Enter()
    {
        DisableDamage();
        DisableControl();

        if (AlertOnDeathEnabled())
            AlertDeath();

        if (HasDeathAnimation())
            RequestDeathAnimation();

        if (HasOnDeathEffects())
            ApplyOnDeathEffects();
    }
    
    public void Update()
    {
        if (!HasDeathAnimation())
            controller.TransitionTo(Lifecycle.State.Dead);
    }
    
    public void Exit()
    {
        KillVelocity();
        RecordPosition();
    }
    
    // ===============================================================================

    void DisableControl()
    {
        if (owner is IControllable controllable)
            controllable.Inactive = true;
    }

    void DisableDamage()
    {
        mortal.Invulnerable = true;
    }

    void AlertDeath()
    {
        owner.Bus.Emit.Local(new ActorDeathEvent(owner));
    }

    void ApplyOnDeathEffects()
    {
        foreach (var effect in definition.Lifecycle.OnDeathEffects)
        {
            var API = new EffectAPI(owner, effect)
            {
                Request = Request.Create
            };

            owner.Bus.Emit.Local(API.Request, API);
        }
    }

    void KillVelocity()
    {
        if (owner is IMovableActor movable)
            movable.Velocity = Vector2.zero;
    }

    void RecordPosition()
    {
        controller.Context.Corpse.Position = owner.Bridge.View.transform.position;
    }
        // ===================================
        //  Animation
        // ===================================

    void RequestDeathAnimation()
    {
        var request = new AnimationAPI(AnimationIntent.Death) 
        {
            Request  = Request.Play,
            Settings = new() 
            { 
                AllowInterrupt      = false,
                HoldOnPlaybackEnd   = true,
            },
        };

        animationRequestHandler.Forward(request.Id, request.Request, request);
    }

    void HandleAnimationAPI(Message<Response, AnimationAPI> response)
    {
        controller.Context.Corpse.DeathAnimation = response.Payload;
    }

    void HandleAnimatorEvent(AnimatorEvent message)
    {
        if (controller.Context.Corpse.DeathAnimation == null)
            return;
 
        if (message.Type != Publish.Ended)
            return;

        if (controller.Context.Corpse.DeathAnimation.Data.Animation == message.Name)
            controller.TransitionTo(Lifecycle.State.Dead);
    }


    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool HasDeathAnimation()
    {
        return definition.Animations.Death.Enabled;
    }

    bool HasOnDeathEffects()
    {
        return definition.Lifecycle.OnDeathEffects?.Count > 0;
    }

    bool AlertOnDeathEnabled()
    {
        return definition.Lifecycle.AlertOnDeath;
    }

    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Dying;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Dead state                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleDeadState : IStateHandler
{
    readonly Actor              owner;
    readonly ActorDefinition    definition;
    readonly Lifecycle          controller;
    
    // ===============================================================================

    public LifecycleDeadState(Lifecycle controller, Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
        this.controller = controller;
    }
    
    // ===============================================================================

    public void Enter()
    {
        RequestAbsenceOrDisposal();
    }

    public void Update()
    {
        Exit();
    }

    public void Exit()
    {
        TrySpawnCorpse();

        Debug.Log("Exiting dead");
    }

    // ===============================================================================

    void TrySpawnCorpse()
    {
        if (!CanBecomeCorpse())
            return;

        RequestCorpseSpawn();
    }

    void RequestAbsenceOrDisposal()
    {
        switch(definition.Lifecycle.Respawn.Enabled)
        {   
            case true:  RequestAbsence();   break;
            case false: RequestDisposal();  break;
        }
    }
        
    void RequestCorpseSpawn()
    {
        Emit.Global(new CorpseRequest(controller.Context.Corpse));
    }

    void RequestAbsence()
    {
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Absent));
    }

    void RequestDisposal()
    {
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Disposal));
    }
    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool CanBecomeCorpse()
    {
        return definition.Lifecycle.Spawn.Corpse;
    }

    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Dead;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events                                         
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct LifecycleEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly Lifecycle.State State   { get; init; }

    public LifecycleEvent(Actor owner, Lifecycle.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

public readonly struct LifecycleTargetEvent : IMessage
{
    public readonly Lifecycle.State Target  { get; init; }

    public LifecycleTargetEvent(Lifecycle.State target)
    {
        Target = target;
    }
}

public readonly struct ActorDeathEvent : IMessage
{
    public readonly Actor Owner             { get; init; }

    public ActorDeathEvent(Actor owner)
    {
        Owner   = owner;
    }
}

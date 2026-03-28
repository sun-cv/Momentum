using System.Collections.Generic;



public class Lifecycle : Service, IServiceLoop
{

    public enum State { Alive, Dying, Dead, Disposal }

    // ===============================================================================
    
    readonly Actor              owner;
    readonly ActorDefinition    definition;

        // -----------------------------------

    readonly Dictionary<State, StateHandler<Lifecycle, State>>  stateHandlers = new();


        // -----------------------------------
       
    State condition;

    // ===============================================================================

    public Lifecycle(Actor actor)
    {
        Services.Lane.Register(this);

        if (actor is not IMortal)
            return;

        owner       = actor;
        definition  = actor.Definition;

        owner.Bus.Link.Local<PresenceStateEvent>(HandlePresenceStateEvent);


        InitializeStateHandlers();
        EnterLifecycle();

    }

    void InitializeStateHandlers()
    {
        Register(State.Alive,    new LifecycleAliveState(owner, definition));
        Register(State.Dying,    new LifecycleDyingState(owner, definition));
        Register(State.Dead,     new LifecycleDeadState (owner, definition));
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
            handler.Update(this);
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
            handler.Exit(this);
    }

    void TransitionState(State newState)
    {
        condition = newState;
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(condition, out var handler))
            handler.Enter(this);

        PublishState();
    }

    void EnterLifecycle()
    {
        if (stateHandlers.TryGetValue(State.Alive, out var handler))
            handler.Enter(this);
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

     void HandlePresenceStateEvent(PresenceStateEvent message)
    {
        switch (message.State)
        {
            case Presence.State.Entering: Enable();  break;
            case Presence.State.Exiting:  Disable(); break;
            case Presence.State.Disposal: Dispose(); break;
        }
    }

    void PublishState()
    {
        owner.Bus.Emit.Local(new LifecycleEvent(owner, condition));
    }
    
    // ===============================================================================

    void Register(State state, StateHandler<Lifecycle, State> handler)
    {
        handler.Transition += TransitionTo;
        stateHandlers[state] = handler;
    }

    // ===============================================================================

    // readonly Logger Log = Logging.For(LogSystem.Lifecycle);

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }
    
    public IMortal Mortal               => owner as IMortal;
    public bool IsAlive                 => condition == State.Alive;
    public bool IsDead                  => condition == State.Dead;

    public UpdatePriority Priority      => ServiceUpdatePriority.Lifecycle;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Alive state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleAliveState : StateHandler<Lifecycle, Lifecycle.State>
{
    readonly Actor owner;
    readonly IMortal mortal;
    
        // -----------------------------------

    bool killingBlowReceived;

    // ===============================================================================

    public LifecycleAliveState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.mortal     = owner as IMortal;

        this.owner.Bus.Link.Local<KillingBlow>(HandleKillingBlow);
    }

    // ===============================================================================

    public override void Enter(Lifecycle controller)
    {

    }
    
    public override void Update(Lifecycle controller)
    {
        if (killingBlowReceived)
        {
            controller.TransitionTo(Lifecycle.State.Dying);
        }
    }
    
    public override void Exit(Lifecycle controller)
    {
        killingBlowReceived = false;
    }

    // ===============================================================================

    void HandleKillingBlow(KillingBlow message)
    {
        killingBlowReceived = true;
    }

    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Alive;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Dying state                                       
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleDyingState : StateHandler<Lifecycle, Lifecycle.State>
{
    readonly Actor              owner;
    readonly IMortal            mortal;
    readonly ActorDefinition    definition;

        // -----------------------------------

    readonly LocalEventHandler<Message<Response, AnimationAPI>> animationRequestHandler;

        // -----------------------------------

    AnimationAPI deathAnimation;
    KillingBlow killingBlow;

    // ===============================================================================

    public LifecycleDyingState(Actor owner,ActorDefinition definition)
    {
        this.owner      = owner;
        this.mortal     = owner as IMortal;
        this.definition = definition;

        animationRequestHandler = new(owner.Bus, HandleAnimationAPI);

        owner.Bus.Link.Local<KillingBlow>  (HandleKillingBlow);
        owner.Bus.Link.Local<AnimatorEvent>(HandleAnimatorEvent);
    }
    
    // ===============================================================================
    
    public override void Enter(Lifecycle controller)
    {
        ClearDyingState();

        DisableDamage();
        DisableControl();

        if (AlertOnDeathEnabled())
            AlertDeath();

        if (HasDeathAnimation())
            RequestDeathAnimation();

        if (HasOnDeathEffects())
            ApplyOnDeathEffects();
    }
    
    public override void Update(Lifecycle controller)
    {
        if (!HasDeathAnimation())
            controller.TransitionTo(Lifecycle.State.Dead);
    }
    
    public override void Exit(Lifecycle controller)
    {
        ExitPresence();
    }
    
    // ===============================================================================


    void ClearDyingState()
    {
        deathAnimation          = null;
    }

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
        deathAnimation = response.Payload;
    }

    void HandleAnimatorEvent(AnimatorEvent message)
    {
        if (deathAnimation == null)
            return;
 
        if (message.Type != Publish.Ended)
            return;

        if (deathAnimation.Data.Animation == message.Name)
            Transition.Invoke(Lifecycle.State.Dead);
    }

    void HandleKillingBlow(KillingBlow message)
    {
        killingBlow = message;
    }

    void ExitPresence()
    {
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Absent));
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

public class LifecycleDeadState : StateHandler<Lifecycle, Lifecycle.State>
{
    readonly Actor              owner;
    readonly ActorDefinition    definition;
    
    // ===============================================================================

    public LifecycleDeadState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }
    
    // ===============================================================================

    public override void Enter(Lifecycle controller)
    {

        if (CanBecomeCorpse())
        {
            RequestCorpseSpawn();
        }

        if (CanRespawn())
        {
            // Send respawn request? Hero specific all other entities are disposed and respawned by spawner.
        }
    }

    public override void Update(Lifecycle controller)
    {
        if (CanRespawn())
            return;

        // ExitPresence();
    }

    public override void Exit(Lifecycle controller)
    {
        
    }

    // ===============================================================================

    
        // Rework required        
    void RequestCorpseSpawn()
    {
        // Emit.Global(new CorpseRequest(owner, context.KillingBlow, context.DeathAnimation));
    }

    void ExitPresence()
    {
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Absent));
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================


    bool CanRespawn()
    {
        return definition.Lifecycle.Respawn.Enabled;
    }

    bool CanBecomeCorpse()
    {
        return definition.Lifecycle.Corpse.Enabled;
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

using System.Collections.Generic;
using UnityEngine;



public class Lifecycle : Service, IServiceLoop
{

    public enum State { Alive, Dying, Dead, Disposal }

    // ===============================================================================
    
    readonly Actor                                          owner;
    readonly ActorDefinition                                definition;

        // -----------------------------------

    Dictionary<State, StateHandler<Lifecycle, State>>       stateHandlers;

        // -----------------------------------

    LifecycleInstance                                       instance;
    LifecycleInstance                                       previousInstance;

    // ===============================================================================

    public Lifecycle(Actor actor)
    {
        Services.Lane.Register(this);

        if (actor is not IDamageable)
            return;

        if (actor is not IDefined defined)
            return;

        this.owner      = actor;
        this.definition = defined.Definition;

        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>(HandlePresenceStateEvent);

        InitializeState();
        InitializeStateHandlers();
        EnterLifecycle();
    }

    void InitializeStateHandlers()
    {
        stateHandlers = new();
        Register(State.Alive,    new LifecycleAliveState(owner, definition));
        Register(State.Dying,    new LifecycleDyingState(owner, definition));
        Register(State.Dead,     new LifecycleDeadState (owner, definition));
    }

    public void InitializeState()
    {
        instance = new(owner);
    }

    // ===============================================================================

    public void Loop()
    {
        UpdateHandler();
    }

    // ===============================================================================

    void UpdateHandler()
    {
        if (stateHandlers.TryGetValue(instance.State.Condition, out var handler))
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
        if (stateHandlers.TryGetValue(instance.State.Condition, out var handler))
            handler.Exit(this);
    }

    void TransitionState(State newState)
    {
        instance.State.Condition = newState;
        PublishState();
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(instance.State.Condition, out var handler))
            handler.Enter(this);
    }

    void EnterLifecycle()
    {
        if (stateHandlers.TryGetValue(State.Alive, out var handler))
            handler.Enter(this);
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

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

    void PublishState()
    {
        owner.Emit.Local(Publish.Transitioning, new LifecycleEvent(owner, instance.State.Condition));
    }
    // ===============================================================================

    void Register(State state, StateHandler<Lifecycle, State> handler)
    {
        handler.Transition += TransitionTo;
        stateHandlers[state] = handler;
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Lifecycle);

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }
    
    public IDamageable Actor            => owner as IDamageable;
    public bool IsAlive                 => instance.State.Condition == State.Alive;
    public bool IsDead                  => instance.State.Condition == State.Dead;

    public LifecycleInstance Instance   => instance;

    public UpdatePriority Priority      => ServiceUpdatePriority.Lifecycle;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Alive state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
            // Rework required - Update TimeSinceLastDamage with hit instance.

public class LifecycleAliveState : StateHandler<Lifecycle, Lifecycle.State>
{
    readonly Actor owner;
    readonly IDamageable damageable;
    readonly ActorDefinition definition;
    
        // -----------------------------------

    float lastHealthPercent     = -1f;
    // float timeSinceLastDamage   = -1f;

    readonly HashSet<HealthThreshold> activeThresholds = new();

    // ===============================================================================

    public LifecycleAliveState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.damageable = owner as IDamageable;
        this.definition = definition;

    }

    // ===============================================================================

    public override void Enter(Lifecycle controller)
    { 
        controller.InitializeState();

        if (controller.Instance.State.Respawn)
        {
            ClearState();
            ResetActor();
            SpawnActor();
        }
        else
        {
            damageable.Health = damageable.MaxHealth;
        }
    }
    
    public override void Update(Lifecycle controller)
    {
        if (damageable.Health == 0)
            controller.TransitionTo(Lifecycle.State.Dying);

        if (definition.Lifecycle.EnableHealthThresholds)
            ProcessHealthThresholds();
        
        if (definition.Lifecycle.AlertOnHealthChange)
            ProcessHealthChangeAlerts();
        
    }
    
    public override void Exit(Lifecycle controller)
    {
        activeThresholds.Clear();
    }

    // ===============================================================================

        // ===================================
        //  State
        // ===================================

    void ClearState()
    {
        lastHealthPercent   = -1f;
        // timeSinceLastDamage = -1f;
    }

    void ResetActor()
    {
        damageable.Invulnerable = false;

        damageable.Health = damageable.MaxHealth;

        if (owner is IControllable controllable)
            controllable.Inactive = true;

        damageable.Health = damageable.MaxHealth;
    }

        // ===================================
        //  Execution
        // ===================================

    void SpawnActor()
    {            
        // owner.Emit.Local(Request.Teleport, new TeleportEvent(spawnPosition));
    }

    void ProcessHealthThresholds()
    {
        float currentPercent = damageable.Health / damageable.MaxHealth;
        
        foreach (var threshold in definition.Lifecycle.HealthThresholds)
        {
            bool wasActive   = activeThresholds.Contains(threshold);
            bool isActive    = currentPercent <= threshold.Percentage;
            
            if (isActive && !wasActive)
            {
                if (threshold.Trigger is HealthThresholdTrigger.OnEnter or HealthThresholdTrigger.OnCross)
                    TriggerThreshold(threshold);
                activeThresholds.Add(threshold);
            }
            else if (!isActive && wasActive)
            {
                if (threshold.Trigger is HealthThresholdTrigger.OnExit or HealthThresholdTrigger.OnCross)
                    TriggerThreshold(threshold);
                activeThresholds.Remove(threshold);
            }
        }
        
        lastHealthPercent = currentPercent;
    }

    void TriggerThreshold(HealthThreshold threshold)
    {
        foreach (var effect in threshold.Effects)
            owner.Emit.Local(Request.Create, new EffectDeclarationEvent(owner, effect));
    }
    
    void ProcessHealthChangeAlerts()
    {
        float currentPercent = damageable.Health / damageable.MaxHealth;

        if (Mathf.Abs(currentPercent - lastHealthPercent) > 0.01f)
        {
            owner.Emit.Local( Publish.Changed, new HealthEvent(owner, damageable.Health, damageable.MaxHealth, lastHealthPercent, currentPercent));
            lastHealthPercent = currentPercent;
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================


    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Alive;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Dying state                                       
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleDyingState : StateHandler<Lifecycle, Lifecycle.State>
{
    readonly Actor              owner;
    readonly IDamageable        damageable;
    readonly ActorDefinition    definition;

        // -----------------------------------

    readonly LocalEventHandler<Message<Response, AnimationRequestEvent>> animationRequestHandler;

        // -----------------------------------

    AnimationRequest deathAnimation;

    // ===============================================================================

    public LifecycleDyingState(Actor owner,ActorDefinition definition)
    {
        this.owner      = owner;
        this.damageable = owner as IDamageable;
        this.definition = definition;

        animationRequestHandler = new(owner.Emit, HandleAnimationPlayback);
        owner.Emit.Link.Local<Message<Publish, AnimatorPlaybackEvent>>(HandleAnimationPlaybackFinished);
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
        controller.Instance.Context.DeathAnimation = deathAnimation;
    }
    
    // ===============================================================================

        // ===================================
        //  State
        // ===================================

    void ClearDyingState()
    {
        deathAnimation          = null;
    }

        // ===================================
        //  Execution
        // ===================================

    void DisableControl()
    {
        if (owner is IControllable controllable)
            controllable.Inactive = true;
    }

    void DisableDamage()
    {
        damageable.Invulnerable = true;
    }

    void AlertDeath()
    {
        owner.Emit.Local(Publish.Triggered, new ActorDeathEvent(owner));
    }

    void ApplyOnDeathEffects()
    {
        foreach (var effect in definition.Lifecycle.OnDeathEffects)
        {
            owner.Emit.Local(Request.Create, new EffectDeclarationEvent(owner, effect));
        }
    }

        // ===================================
        //  Animation
        // ===================================

    void RequestDeathAnimation()
    {
        var request = new AnimationRequest(AnimationIntent.Death) 
        {
            options = new() 
            { 
                AllowInterrupt = false 
            },
        };

        animationRequestHandler.Send(Request.Start, new AnimationRequestEvent(request));
    }

    void HandleAnimationPlayback(Message<Response, AnimationRequestEvent> response)
    {
        deathAnimation = response.Payload.AnimationRequest;
    }

    void HandleAnimationPlaybackFinished(Message<Publish, AnimatorPlaybackEvent> message)
    {
        if (deathAnimation == null)
            return;

        if (deathAnimation.data.Animation == message.Payload.Name)
            Transition.Invoke(Lifecycle.State.Dead);
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
    readonly IDamageable        damageable;
    readonly ActorDefinition    definition;
    
        // -----------------------------------

    LifeCycleContext context;

    // ===============================================================================

    public LifecycleDeadState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.damageable = owner as IDamageable;
        this.definition = definition;
    }
    
    // ===============================================================================

    public override void Enter(Lifecycle controller)
    {
        context = controller.Instance.Context;

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

        ExitPresence();
    }

    public override void Exit(Lifecycle controller)
    {
        
    }

    // ===============================================================================

    void RequestCorpseSpawn()
    {
        Emit.Global(Request.Create, new CorpseRequestEvent(new() { Owner = owner, Animation = context.DeathAnimation, KillingBlow = context.KillingBlow}));
    }

    void ExitPresence()
    {
        owner.Emit.Local(Request.Transition, new PresenceTargetEvent(Presence.Target.Absent));
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
//                                      Declarations                                      
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleInstance
{
    public Actor Owner                      { get; init; }
    public LifecycleState State             { get; init; }
    public LifeCycleContext Context         { get; set;  }

    public LifecycleInstance(Actor actor)
    {
        Owner           = actor;
        State           = new();
        Context         = new();

        State.Condition = Lifecycle.State.Alive;
    }
}

public class LifecycleState
{
    public Lifecycle.State Condition        { get; set; }
    public bool Respawn                     { get; set; }
}

public class LifeCycleContext
{
    public KillingBlow KillingBlow          { get; set; }
    public AnimationRequest DeathAnimation  { get; set; }
}

public class HealthThreshold
{
    public string EventName                 { get; init; }
    public float Percentage                 { get; init; }
    public HealthThresholdTrigger Trigger   { get; init; }
    public List<Effect> Effects             { get; init; } = new();
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum HealthThresholdTrigger
{
    OnEnter,
    OnExit,
    OnCross,
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events                                         
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct HealthEvent
{
    public readonly Actor Owner             { get; init; }
    public readonly float Health            { get; init; }
    public readonly float MaxHealth         { get; init; }
    public readonly float CurrentPercent    { get; init; }
    public readonly float LastPercent       { get; init; }

    public HealthEvent(Actor owner, float health, float maxHealth, float current, float last)
    {
        Owner           = owner;
        Health          = health;
        MaxHealth       = maxHealth;
        CurrentPercent  = current;
        LastPercent     = last;
    }
}

public readonly struct LifecycleEvent
{
    public readonly Actor Owner             { get; init; }
    public readonly Lifecycle.State State   { get; init; }

    public LifecycleEvent(Actor owner, Lifecycle.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

public readonly struct LifecycleTargetEvent
{
    public readonly Lifecycle.State Target  { get; init; }

    public LifecycleTargetEvent(Lifecycle.State target)
    {
        Target = target;
    }
}
public readonly struct ActorDeathEvent
{
    public readonly Actor Owner             { get; init; }

    public ActorDeathEvent(Actor owner)
    {
        Owner   = owner;
    }
}



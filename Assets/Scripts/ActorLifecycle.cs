using System.Collections.Generic;
using UnityEngine;



public class Lifecycle : Service, IServiceLoop
{

    public enum State { Alive, Dying, Dead }

        // ---------------------------------
    
    readonly Actor              owner;
    readonly IDamageable        actor;
    readonly ActorDefinition    definition;

    Dictionary<State, ILifecycleStateHandler> stateHandlers;

        // ---------------------------------

    State state         = State.Alive;
    public bool respawn = false;


    // ===============================================================================
    // Initialization
    // ===============================================================================

    public Lifecycle(Actor actor)
    {
        Services.Lane.Register(this);

        if (actor is not IDamageable damageable)
            return; 

        if (actor is not IDefined defined)
            return;

        this.owner      = actor;
        this.actor      = damageable;
        this.definition = defined.Definition;

        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>(HandlePresenceStateEvent);

        InitializeStateHandlers();
        EnterHandler();
    }

    void InitializeStateHandlers()
    {
        stateHandlers = new()
        {
            { State.Alive,  new AliveStateHandler(owner, actor, definition)  },
            { State.Dying,  new DyingStateHandler(owner, actor, definition)  },
            { State.Dead,   new DeadStateHandler (owner, actor, definition)  }
        };
    }


    // ===============================================================================
    // Core
    // ===============================================================================
    
    public void Loop()
    {
        LoopHandler();
    }


    // ===============================================================================
    // Operations
    // ===============================================================================

    void LoopHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Loop(this);
    }

    public void TransitionTo(State newState)
    {
        ExitHandler();
        TransitionState(newState);
        EnterHandler();
    }

    void ExitHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Exit(this);
    }

    void TransitionState(State newState)
    {
        state = newState;
        PublishState();
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Enter(this);
    }


    // ===============================================================================
    // Event Handlers
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


        // =================================
        // Event Helpers
        // =================================

    void PublishState()
    {
        owner.Emit.Local(Publish.Transitioning, new LifecycleEvent(owner, state));
    }


    // ===============================================================================
    // Debug
    // ===============================================================================
    
    readonly Logger Log = Logging.For(LogSystem.Lifecycle);


    // ===============================================================================
    // Lifecycle
    // ===============================================================================

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }


    // ===============================================================================
    // Properties
    // ===============================================================================

    public IDamageable Actor => actor;

    public bool IsAlive => state == State.Alive;
    public bool IsDead  => !IsAlive;

    public UpdatePriority Priority => ServiceUpdatePriority.Lifecycle;
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Handlers                                        
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface ILifecycleStateHandler
{    
    void Enter(Lifecycle controller);
    void Loop (Lifecycle controller);
    void Exit (Lifecycle controller);
}



    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                  Alive state
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        
        // Rework required - Update TimeSinceLastDamage with hit instance.

public class AliveStateHandler : ILifecycleStateHandler
{
    readonly Actor owner;
    readonly IDamageable actor;
    readonly ActorDefinition definition;
    
        // -------------------------------------

    float lastHealthPercent     = -1f;
    float timeSinceLastDamage   = -1f;

    HashSet<HealthThreshold> activeThresholds = new();


    // ===============================================================================
    // Initialization
    // ===============================================================================

    public AliveStateHandler(Actor owner, IDamageable actor, ActorDefinition definition)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = definition;

    }


    // ===============================================================================
    // Core
    // ===============================================================================

    public void Enter(Lifecycle controller)
    { 
        if (controller.respawn)
        {
            ClearState();
            ResetActor();
            SpawnActor();
        }
        else
        {
            actor.Health = actor.MaxHealth;
        }
    }
    
    public void Loop(Lifecycle controller)
    {
        if (ActorShouldDie())
        {
            controller.TransitionTo(Lifecycle.State.Dying);
            return;
        }

        if (definition.Lifecycle.EnableHealthThresholds)
            ProcessHealthThresholds();
        
        if (definition.Lifecycle.AlertOnHealthChange)
            ProcessHealthChangeAlerts();
        
    }
    
    public void Exit(Lifecycle controller)
    {
        activeThresholds.Clear();
    }
    

    // ===============================================================================
    // Operations
    // ===============================================================================

    void SpawnActor()
    {            
        // owner.Emit.Local(Request.Teleport, new TeleportEvent(spawnPosition));
    }

    void ProcessHealthThresholds()
    {
        float currentPercent = actor.Health / actor.MaxHealth;
        
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
        float currentPercent = actor.Health / actor.MaxHealth;

        if (Mathf.Abs(currentPercent - lastHealthPercent) > 0.01f)
        {
            owner.Emit.Local( Publish.Changed, new HealthEvent(owner, actor.Health, actor.MaxHealth, lastHealthPercent, currentPercent));
            lastHealthPercent = currentPercent;
        }
    }


    // ===============================================================================
    // State Management
    // ===============================================================================

    void ClearState()
    {
        lastHealthPercent   = -1f;
        timeSinceLastDamage = -1f;
    }

    void ResetActor()
    {
        actor.Invulnerable = false;

        actor.Health = actor.MaxHealth;

        if (owner is IControllable controllable)
            controllable.Inactive = true;

        actor.Health = actor.MaxHealth;
    }


    // ===============================================================================
    //  PREDICATES
    // ===============================================================================

    bool ActorShouldDie()
    {
        if (actor.Impervious)
            return false;
        
        if(actor.Health > 0)
            return false;

        return true;
    }


    // ===============================================================================
    // Properties
    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Alive;
}



    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                     Dying state                                       
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class DyingStateHandler : ILifecycleStateHandler
{
    readonly Actor              owner;
    readonly IDamageable        actor;
    readonly ActorDefinition    definition;

        // -------------------------------------

    string deathAnimation;
    float  deathAnimationDuration;

    ClockTimer dyingTimer;

        // -------------------------------------

    LocalEventHandler<Message<Response, AnimationDurationEvent>> animationDurationHandler;

    // ===============================================================================
    // Initialization
    // ===============================================================================

    public DyingStateHandler(Actor owner, IDamageable actor, ActorDefinition definition)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = definition;

        animationDurationHandler = new(owner.Emit, HandleAnimationDuration);
    }
    
    
    // ===============================================================================
    // Core
    // ===============================================================================
    
    public void Enter(Lifecycle controller)
    {
        ClearDyingState();

        DisableDamage();
        DisableControl();

        if (AlertOnDeathEnabled())
            AlertDeath();

        if (HasDeathAnimation())
        {
            PlayDeathAnimation();
            GetDeathAnimationDuration();
        }

        if (HasOnDeathEffects())
            ApplyOnDeathEffects();
        
        StartDeathTimer();
    }
    
    public void Loop(Lifecycle controller)
    {
        if (dyingTimer.IsFinished)
            controller.TransitionTo(Lifecycle.State.Dead);
    }
    
    public void Exit(Lifecycle controller)
    {
        // Cleanup (like weapon's ClearMovementFromPhase)
    }
    

    // ===============================================================================
    // Operations
    // ===============================================================================

    void DisableControl()
    {
        if (owner is IControllable controllable)
            controllable.Inactive = true;
    }

    void DisableDamage()
    {
        actor.Invulnerable = true;
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


        // =================================
        //  Animation
        // =================================

    void PlayDeathAnimation()
    {
        deathAnimation = SelectDeathAnimation();
        owner.Emit.Local(Request.Start, new AnimationRequestEvent(deathAnimation) { AllowInterrupt = false });
    }

        // Rework required further Damage type animations?
    string SelectDeathAnimation()
    {
        var animations = definition.Animations.Death;
                
        if (animations.Random?.Length > 0)
            return animations.Random[Random.Range(0, animations.Random.Length)];
        
        return animations.Default;
    }

    void GetDeathAnimationDuration()
    {
        animationDurationHandler.Send(Request.Get, new AnimationDurationEvent(deathAnimation));  
    }

    void HandleAnimationDuration(Message<Response, AnimationDurationEvent> response)
    {
        deathAnimationDuration = response.Payload.Duration;
    }


    // ===============================================================================
    // State Management
    // ===============================================================================

    void ClearDyingState()
    {
        deathAnimation          = "";
        deathAnimationDuration  = 0;
    }


    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void StartDeathTimer()
    {
        dyingTimer = new ClockTimer(Mathf.Max(0f, deathAnimationDuration));
        dyingTimer.Start();
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
    // Properties
    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Dying;
}



    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                    Dead state                                      
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class DeadStateHandler : ILifecycleStateHandler
{
    readonly Actor              owner;
    readonly IDamageable        actor;
    readonly ActorDefinition    definition;
    
        // -------------------------------------

    ClockTimer respawnTimer     = new(0);
    ClockTimer corpseTimer      = new(0);
    

    // ===============================================================================
    // Initialization
    // ===============================================================================

    public DeadStateHandler(Actor owner, IDamageable actor, ActorDefinition definition)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = definition;
    }


    // ===============================================================================
    // Core
    // ===============================================================================
    
    public void Enter(Lifecycle controller)
    {

        if (CanRespawn())
        {
            respawnTimer = new(definition.Lifecycle.Respawn.RespawnDelay);
            respawnTimer.Start();
        }

        if (CanPersist())
        {
            corpseTimer = new(definition.Lifecycle.Corpse.PersistDuration);
            corpseTimer.Start();     
        }
    }

    
    public void Loop(Lifecycle controller)
    {
        if (RespawnReady())
        {
            controller.respawn = true;
            controller.TransitionTo(Lifecycle.State.Alive);
        }

        if (CorpseExpired())
        {
            ExitPresence();
        }
    }

    public void Exit(Lifecycle controller)
    {
        
    }
    

    // ===============================================================================
    // Operations
    // ===============================================================================

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

    bool CanPersist()
    {
        return definition.Lifecycle.Corpse.Persists;
    }

    bool RespawnReady()
    {
        return respawnTimer.IsFinished;
    }

    bool CorpseExpired()
    {
        return corpseTimer.IsFinished;
    }


    // ===============================================================================
    // Properties
    // ===============================================================================

    public Lifecycle.State State => Lifecycle.State.Dead;
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations                                      
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum HealthThresholdTrigger
{
    OnEnter,
    OnExit,
    OnCross,
}

public class HealthThreshold
{
    public string EventName                 { get; init; }
    public float Percentage                 { get; init; }
    public HealthThresholdTrigger Trigger   { get; init; }
    public List<Effect> Effects             { get; init; } = new();
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

public readonly struct ActorDeathEvent
{
    public readonly Actor Owner             { get; init; }

    public ActorDeathEvent(Actor owner)
    {
        Owner   = owner;
    }
}



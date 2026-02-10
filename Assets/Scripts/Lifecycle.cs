using System.Collections.Generic;
using UnityEngine;





public class Lifecycle : IServiceStep
{
    readonly Logger Log = Logging.For(LogSystem.Lifecycle);

    public enum State { Alive, Dying, Dead }

    Actor               owner;
    IDamageable         actor;
    LifecycleDefinition definition;
    
    State state         = State.Alive;
    public bool respawn = false;

    Dictionary<State, ILifecycleStateHandler> stateHandlers;

    public Lifecycle(Actor actor)
    {
        Services.Lane.Register(this);

        if (actor is not IDamageable damageable)
            return;

        if (actor is not IDefined defined)
            return;

        this.owner      = actor;
        this.actor      = damageable;
        this.definition = defined.Definition.Lifecycle;
        
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

    public void Step()
    {
        StepHandler();
    }

    void StepHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Step(this);
    }

    public void TransitionTo(State newState)
    {
        ExitHandler();
        TransitionState(newState);
        EnterHandler();
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Enter(this);
    }

    void TransitionState(State newState)
    {
        state = newState;
        PublishState();
    }

    void ExitHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Exit(this);
    }

    void PublishState()
    {
        owner.Emit.Local(Publish.StateChange, new LifecycleEvent(owner, state)) ;
    }

    public IDamageable Actor => actor;

    public bool IsAlive => state == State.Alive;
    public bool IsDying => state == State.Dying;
    public bool IsDead  => state == State.Dead;

    public UpdatePriority Priority => ServiceUpdatePriority.Lifecycle;
}

// ============================================================================
// STATE HANDLERS
// ============================================================================

public interface ILifecycleStateHandler
{    
    void Enter(Lifecycle controller);
    void Step (Lifecycle controller);
    void Exit (Lifecycle controller);
}

// ============================================================================
// ALIVE STATE HANDLER
// ============================================================================


// Rework required - Update TimeSinceLastDamage with hit instance.

public class AliveStateHandler : ILifecycleStateHandler
{
    readonly Actor owner;
    readonly IDamageable actor;
    readonly LifecycleDefinition definition;
    
    float lastHealthPercent     = -1f;
    float timeSinceLastDamage   = -1f;

    HashSet<HealthThreshold> activeThresholds = new();
        
    public AliveStateHandler(Actor owner, IDamageable actor, LifecycleDefinition definition)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = definition;
    }
    
    public void Enter(Lifecycle controller)
    { 
        Debug.Log("Enter alive");

        ResetActor();

        if (controller.respawn)
        {
            ClearState();
            SpawnActor();
        }
    }
    
    public void Step(Lifecycle controller)
    {
        Debug.Log(ActorShouldDie());
        if (ActorShouldDie())
        {
            Debug.Log("entity should die");
            controller.TransitionTo(Lifecycle.State.Dying);
            return;
        }

        if (definition.EnableHealthThresholds)
            ProcessHealthThresholds();
        
        if (definition.AlertOnHealthChange)
            ProcessHealthChangeAlerts();
        
    }
    
    public void Exit(Lifecycle controller)
    {
        activeThresholds.Clear();
    }
    
    // ========================================================================
    // FEATURE IMPLEMENTATIONS
    // ========================================================================
    
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
    }

    void SpawnActor()
    {            
        // owner.Emit.Local(Request.Teleport, new TeleportEvent(spawnPosition));
    }

    void ProcessHealthThresholds()
    {
        float currentPercent = actor.Health / actor.MaxHealth;
        
        foreach (var threshold in definition.HealthThresholds)
        {
            bool wasActive = activeThresholds.Contains(threshold);
            bool isActive  = currentPercent <= threshold.Percentage;
            
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
            owner.Emit.Local(Request.Create, new EffectDeclarationEvent(owner,effect));
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

    // ============================================================================
    //  PREDICATEs
    // ============================================================================


    bool ActorShouldDie()
    {
        if (actor.Impervious)
            return false;
        
        if(actor.Health > 0)
            return false;

        return true;
    }


    public Lifecycle.State State => Lifecycle.State.Alive;
}


// ============================================================================
// DYING STATE HANDLER
// ============================================================================


public class DyingStateHandler : ILifecycleStateHandler
{
    readonly Actor                  owner;
    readonly IDamageable            actor;
    readonly LifecycleDefinition    definition;
    
    ClockTimer dyingTimer;

    string deathAnimation;
    float  deathAnimationDuration;
    
    LocalEventHandler<Message<Response, AnimationDuration>> animationDurationHandler;

    public DyingStateHandler(Actor owner, IDamageable actor, LifecycleDefinition def)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = def;

        animationDurationHandler = new(owner.Emit, HandleAnimationDuration);
    }
    
    public void Enter(Lifecycle controller)
    {
       Debug.Log("Enter Dying");


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
    
    public void Step(Lifecycle controller)
    {
        if (dyingTimer.IsFinished)
            controller.TransitionTo(Lifecycle.State.Dead);
    }
    


    public void Exit(Lifecycle controller)
    {
        // Cleanup (like weapon's ClearMovementFromPhase)
    }
    
    // ========================================================================
    // FEATURE IMPLEMENTATIONS
    // ========================================================================
    
    void ClearDyingState()
    {
        deathAnimation          = "";
        deathAnimationDuration  = -1;
    }

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
        owner.Emit.Local( Publish.Triggered, new ActorDeathEvent(owner));
    }

    void PlayDeathAnimation()
    {
        deathAnimation = SelectDeathAnimation();
        owner.Emit.Local(Request.Start, new AnimationEvent(owner, new AnimatorRequest(deathAnimation) { AllowInterrupt = false }));
    }

        // Rework required further Damage type animations?
    string SelectDeathAnimation()
    {
        var animations = definition.DeathAnimations;
                
        if (animations.Random?.Length > 0)
            return animations.Random[UnityEngine.Random.Range(0, animations.Random.Length)];
        
        return animations.Default;
    }

    void GetDeathAnimationDuration()
    {
        animationDurationHandler.Send(Request.Get, new AnimationDuration(new AnimatorRequest(deathAnimation)));  
    }

    void HandleAnimationDuration(Message<Response, AnimationDuration> response)
    {
        deathAnimationDuration = response.Payload.Duration;
    }

    void ApplyOnDeathEffects()
    {
        foreach (var effect in definition.OnDeathEffects)
        {
            owner.Emit.Local(Request.Create, new EffectDeclarationEvent(owner, effect));
        }
    }

    void StartDeathTimer()
    {
        dyingTimer = new ClockTimer(Mathf.Max(0f, deathAnimationDuration));
        dyingTimer.Start();
    }



    // ============================================================================
    //  PREDICATEs
    // ============================================================================

    bool HasDeathAnimation()
    {
        return definition.DeathAnimations != null;
    }

    bool HasOnDeathEffects()
    {
        return definition.OnDeathEffects.Count > 0;
    }

    bool AlertOnDeathEnabled()
    {
        return definition.AlertOnDeath;
    }


    public Lifecycle.State State => Lifecycle.State.Dying;
}

// ============================================================================
// DEAD STATE HANDLER
// ============================================================================

public class DeadStateHandler : ILifecycleStateHandler
{
    readonly Actor owner;
    readonly IDamageable actor;
    readonly LifecycleDefinition definition;
    
    ClockTimer respawnTimer;
    
    public Lifecycle.State State => Lifecycle.State.Dead;
    
    public DeadStateHandler(Actor owner, IDamageable actor, LifecycleDefinition definition)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = definition;
    }
    
    public void Enter(Lifecycle controller)
    {

               Debug.Log("Enter Dead");

        if (definition.RespawnBehavior?.Enabled == true)
        {
            respawnTimer = new ClockTimer(definition.RespawnBehavior.RespawnDelay);
            respawnTimer.Start();
        }
        else
        {
            Object.Destroy(owner.Bridge.View);
        }
    }

    
    public void Step(Lifecycle controller)
    {
        if (respawnTimer?.IsFinished == true)
        {
            controller.respawn = true;
            controller.TransitionTo(Lifecycle.State.Alive);
        }
    }
    
    public void Exit(Lifecycle controller)
    {
    }
    
}


// ============================================================================
// LIFECYCLE CONFIG
// ============================================================================


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


// ============================================================================
// LIFECYCLE EVENTS
// ============================================================================



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



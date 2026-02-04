using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;








// ============================================================================
// LIFECYCLE SYSTEM
// ============================================================================


public class Lifecycle : IServiceStep
{
    readonly Logger Log = Logging.For(LogSystem.Lifecycle);

    public enum State { Alive, Dying, Dead }

    Actor               owner;
    IDamageable         actor;
    LifecycleDefinition definition;
    
    State state = State.Alive;

    Dictionary<State, ILifecycleStateHandler> stateHandlers;

    public Lifecycle(Actor actor, LifecycleDefinition definition)
    {
        if (actor is not IDamageable instance)
            return;

        this.owner      = actor;
        this.actor      = instance;
        this.definition = definition;
        
        InitializeStateHandlers();
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
        TickHandler();
    }

    void TickHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Tick(this);
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
        owner.Emit.Local(Publish.StateChange, new MLifecycleChange(owner, state)) ;
    }

    public bool IsAlive => state == State.Alive;
    public bool IsDying => state == State.Dying;
    public bool IsDead  => state == State.Dead;
    public IDamageable Actor => actor;

    public UpdatePriority Priority => ServiceUpdatePriority.Lifecycle;
}

// ============================================================================
// STATE HANDLERS
// ============================================================================

public interface ILifecycleStateHandler
{    
    void Enter(Lifecycle controller);
    void Tick(Lifecycle controller);
    void Exit(Lifecycle controller);
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
    // float timeSinceLastDamage   = -1f;

    HashSet<HealthThreshold> activeThresholds = new();
        
    public AliveStateHandler(Actor owner, IDamageable actor, LifecycleDefinition definition)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = definition;
    }
    
    public void Enter(Lifecycle controller)
    {
        ClearState();
    }
    
    public void Tick(Lifecycle controller)
    {
        if (ActorShouldDie())
        {
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
        // timeSinceLastDamage = -1f;
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
                if (threshold.Trigger is ThresholdTrigger.OnEnter or ThresholdTrigger.OnCross)
                    TriggerThreshold(threshold);
                activeThresholds.Add(threshold);
            }
            else if (!isActive && wasActive)
            {
                if (threshold.Trigger is ThresholdTrigger.OnExit or ThresholdTrigger.OnCross)
                    TriggerThreshold(threshold);
                activeThresholds.Remove(threshold);
            }
        }
        
        lastHealthPercent = currentPercent;
    }
    
    void TriggerThreshold(HealthThreshold threshold)
    {
        foreach (var effect in threshold.Effects)
            owner.Emit.Local(Request.Create, new MEffectDeclaration(owner,effect));
    }
    
    void ProcessHealthChangeAlerts()
    {
        float currentPercent = actor.Health / actor.MaxHealth;

        if (Mathf.Abs(currentPercent - lastHealthPercent) > 0.01f)
        {
            owner.Emit.Local( Publish.Changed, new MHealth(owner, actor.Health, actor.MaxHealth, lastHealthPercent, currentPercent));
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
    
    LocalEventHandler<Message<Response, MAnimationDuration>> animationDurationHandler;

    public DyingStateHandler(Actor owner, IDamageable actor, LifecycleDefinition def)
    {
        this.owner      = owner;
        this.actor      = actor;
        this.definition = def;

        animationDurationHandler = new(owner.Emit, HandleAnimationDuration);
    }
    
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
    
    public void Tick(Lifecycle controller)
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
        owner.Emit.Local( Publish.Triggered, new MActorDeathEvent(owner));
    }

    void PlayDeathAnimation()
    {
        deathAnimation = SelectDeathAnimation();
        owner.Emit.Local(Request.Start, new MAnimation(owner, new AnimatorRequest(deathAnimation) { AllowInterrupt = false }));
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
        animationDurationHandler.Send(Request.Get, new MAnimationDuration(new AnimatorRequest(deathAnimation)));  
    }

    void HandleAnimationDuration(Message<Response, MAnimationDuration> response)
    {
        deathAnimationDuration = response.Payload.Duration;
    }

    void ApplyOnDeathEffects()
    {
        foreach (var effect in definition.OnDeathEffects)
        {
            owner.Emit.Local(Request.Create, new MEffectDeclaration(owner, effect));
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
    readonly LifecycleDefinition def;
    
    ClockTimer respawnTimer;
    
    public Lifecycle.State State => Lifecycle.State.Dead;
    
    public DeadStateHandler(Actor owner, IDamageable actor, LifecycleDefinition def)
    {
        this.owner = owner;
        this.actor = actor;
        this.def = def;
    }
    
    public void Enter(Lifecycle controller)
    {
        // // Handle respawn if enabled
        // if (def.RespawnBehavior?.Enabled == true)
        // {
        //     respawnTimer = new ClockTimer(def.RespawnBehavior.RespawnDelay);
        //     respawnTimer.OnTimerStop += () => HandleRespawn(controller);
        //     respawnTimer.Start();
        // }
    }
    
    public void Tick(Lifecycle controller)
    {
        // Wait for respawn or stay dead
    }
    
    public void Exit(Lifecycle controller)
    {
    }
    
    void HandleRespawn(Lifecycle controller)
    {
        // if (def.RespawnBehavior.RestoreFullHealth && actor != null)
        //     actor.Health = actor.MaxHealth;
        
        // // Teleport to respawn location
        // // Re-enable control
        // if (owner is IControllable controllable)
        //     controllable.Inactive = false;
        
        // controller.TransitionTo(Lifecycle.State.Alive);
    }
}


// ============================================================================
// LIFECYCLE CONFIG
// ============================================================================


public enum ThresholdTrigger
{
    OnEnter,
    OnExit,
    OnCross,
}
public class HealthThreshold
{
    public string EventName                 { get; init; }
    public float Percentage                 { get; init; }
    public ThresholdTrigger Trigger         { get; init; }
    public List<Effect> Effects             { get; init; } = new();
}


// ============================================================================
// LIFECYCLE EVENTS
// ============================================================================



public readonly struct MHealth
{
    public readonly Actor Owner             { get; init; }
    public readonly float Health            { get; init; }
    public readonly float MaxHealth         { get; init; }
    public readonly float CurrentPercent    { get; init; }
    public readonly float LastPercent       { get; init; }

    public MHealth(Actor owner, float health, float maxHealth, float current, float last)
    {
        Owner           = owner;
        Health          = health;
        MaxHealth       = maxHealth;
        CurrentPercent  = current;
        LastPercent     = last;
    }
}


public readonly struct MLifecycleChange
{
    public readonly Actor Owner             { get; init; }
    public readonly Lifecycle.State State   { get; init; }

    public MLifecycleChange(Actor owner, Lifecycle.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

public readonly struct MActorDeathEvent
{
    public readonly Actor Owner             { get; init; }

    public MActorDeathEvent(Actor owner)
    {
        Owner   = owner;
    }
}



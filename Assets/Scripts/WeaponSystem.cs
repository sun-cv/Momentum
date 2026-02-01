using System;
using System.Collections.Generic;
using System.Linq;



// ============================================================================
// MAIN WEAPON CONTROLLER
// ============================================================================


public class WeaponSystem : IServiceTick
{
    readonly Logger Log = Logging.For(LogSystem.Weapons);

    Actor                                                   owner;
    WeaponLoadout                                           loadout;
    WeaponInstance                                          instance;
    WeaponAction                                            previousAction;

    WeaponCooldown                                          cooldown;
    WeaponActivationValidator                               validator;
    Dictionary<WeaponPhase, IWeaponPhaseHandler>            phaseHandlers;
    Dictionary<WeaponActivation, IActivationStrategy>       activationStrategies;

    IReadOnlyDictionary<Capability, Command>                active;
    IReadOnlyDictionary<Capability, Command>                buffer;
    IReadOnlyDictionary<Capability, IReadOnlyList<string>>  locks;

    public int NonCancelableAttackLocks { get; set; } = 0;

    EventHandler<HitboxRequest, HitboxResponse>             hitboxEvents;

    public WeaponSystem(Actor actor)
    {
        Services.Lane.Register(this);

        loadout         = new();
        cooldown        = new();
        validator       = new(this);
        
        owner           = actor;
        hitboxEvents    = new(HandleHitboxResponse);

        InitializePhaseHandlers();
        InitializeActivationStrategies();

        LinkLocal<CommandPublish>     (HandleCommandPublish);
        LinkLocal<EffectPublish>      (HandleEffectNonCancelableLockCount);
        LinkLocal<LockPublish>        (HandleLockPublish);
        LinkLocal<EquipmentPublish>   (HandleEquipmentPublish);
    }


    void InitializePhaseHandlers()
    {
        phaseHandlers = new()
        {
            { WeaponPhase.Enable,           new EnablePhaseHandler()            },
            { WeaponPhase.Charging,         new ChargingPhaseHandler()          },
            { WeaponPhase.Fire,             new FirePhaseHandler()              },
            { WeaponPhase.FireEnd,          new FireEndPhaseHandler()           },
            { WeaponPhase.Disable,          new DisablePhaseHandler()           }
        };
    }

    void InitializeActivationStrategies()
    {
        activationStrategies = new()
        {
            { WeaponActivation.OnPress,     new OnPressActivationStrategy()     },
            { WeaponActivation.OnCharge,    new OnChargeActivationStrategy()    },
            { WeaponActivation.OnRelease,   new OnReleaseActivationStrategy()   },
            { WeaponActivation.WhileHeld,   new WhileHeldActivationStrategy()   }
        };
    }

    public void Tick()
    {
        if (HasActiveWeapon())
            AdvanceWeaponState();

        ProcessWeaponActivation();

        DebugLog();
    }

    // ============================================================================
    // PHASE ADVANCEMENT
    // ============================================================================

    void AdvanceWeaponState()
    {
        if (ShouldReleaseWeapon())
        {            
            ReleaseWeapon();
            return;
        }            

        var strategy = GetActivationStrategy(instance.Action);

        if (instance.State.Phase == WeaponPhase.Charging)
        {
            if (strategy.CheckReleaseTriggersInCharging(instance, this))
                return;
        }
        else if (instance.State.Phase == WeaponPhase.Fire)
        {
            if (strategy.CheckReleaseTriggersInFire(instance, this))
                return;
        }

        if (phaseHandlers.TryGetValue(instance.State.Phase, out var handler))
            handler.Tick(instance, this);
    
    }


    public void TransitionTo(WeaponPhase newPhase)
    {
        ExitHandler();
        TransitionState(newPhase);
        EnterHandler();
    }

    void EnterHandler()
    {
        if (phaseHandlers.TryGetValue(instance.State.Phase, out var handler))
            handler.Enter(instance, this);
    }

    void TransitionState(WeaponPhase phase)
    {
        instance.State.Phase = phase;
        PublishTransition();
    }

    void ExitHandler()
    {
        if (phaseHandlers.TryGetValue(instance.State.Phase, out var handler))
            handler.Exit(instance, this);
    }


    // ============================================================================
    // WEAPON ACTIVATION
    // ============================================================================

    void ProcessWeaponActivation()
    {
        if (HasActiveWeapon())
        {
            if (TryActivateFromAvailableControls())
                return;

            if (TryActivateInterruptWeapon())
                return;
        }

        if (HasBufferCommands())
            TryActivateDefaultWeapon();
    }

    bool TryActivateDefaultWeapon()
    {
        foreach (var command in buffer.Values)
        {
            var weapon = GetDefaultWeapon(command);

            if (CanActivateFromDefaultControls(weapon))
            {
                ReplaceAndActivateWeapon(weapon);
                return true;
            }
        }
        return false;
    }

    bool TryActivateInterruptWeapon()
    {

        foreach (var command in buffer.Values)
        {
            var weapon = GetDefaultWeapon(command);

            if (CanActivateInterruptionControls(weapon))
            {
                ReplaceAndActivateWeapon(weapon);
                return true;
            }
        }

        return false;
    }

    bool TryActivateFromAvailableControls()
    {
        if (instance.State.AvailableControls.Count == 0)
            return false;

        foreach (var weaponName in instance.State.AvailableControls)
        {
            if (!loadout.TryGetAction(weaponName, out var weapon))
                continue;

            if (CanActivateFromAvailableControls(weapon))
            {
                ReplaceAndActivateWeapon(weapon);
                return true;
            }
        }

        return false;
    }

    void ReplaceAndActivateWeapon(WeaponAction weapon)
    {
        if (HasActiveWeapon())
            ReleaseWeapon();

        EquipWeapon(weapon);
        ActivateWeapon();
        EnableWeapon();
    }

    void EquipWeapon(WeaponAction Action)
    {
        instance = new WeaponInstance(Action);
        EmitLocal<WeaponPublish>(new(Guid.NewGuid(), Publish.Equipped, new() { Owner = owner, Instance = instance }));
    }

    void ActivateWeapon()
    {
        switch (instance.Action.Availability)
        {
            case WeaponAvailability.Default:
                ConsumeAllCommands(buffer, instance.Action.Trigger);
                break;
            case WeaponAvailability.OnPhase:
                ConsumeAllCommands(buffer, instance.Action.Trigger);
                break;
            case WeaponAvailability.OnHeld:
                break;  
        }
        
        StoreAllCommandIDs(active, instance.Action.Trigger);
        StoreInputSnapshot(active, instance.Action.Trigger);

        if (instance.Action.LockTriggerAction)
            LockAllCommands(active, instance.Action.Trigger);
    }


    void EnableWeapon()
    {
        EnterHandler();
    }


    void ReleaseWeapon()
    {
        TransitionTo(WeaponPhase.Disable);
            
        DisableWeapon();
        DeactivateWeapon();
        UnequipWeapon();
    }


    void DisableWeapon()
    {
        ExitHandler();
    }

    void DeactivateWeapon()
    {
        UnlockAllCommands(active, instance.Action.Trigger);
        StorePreviousAction();
        RegisterCooldown();
    }

    void UnequipWeapon()
    {
        EmitLocal<WeaponPublish>(new(Guid.NewGuid(), Publish.Released, new() { Owner = owner, Instance = instance }));
        ClearInstance();        
    }

    // ============================================================================
    // ACTIVATION PREDICATES
    // ============================================================================

    bool CanActivateFromDefaultControls(WeaponAction weapon)
    {
        if (weapon == null)
            return false;

        if (!validator.CanActivate(weapon))
            return false;

        return true;
    } 

    bool CanActivateFromAvailableControls(WeaponAction weapon)
    {
        if (!HasAllRequiredTriggers(weapon))
            return false;

        if (!validator.CanActivate(weapon))
            return false;

        if (weapon.Availability == WeaponAvailability.OnHeld)
            return true;

        if (!HasNewCommandForWeapon(weapon))
            return false;

        return true;
    }

    bool CanActivateInterruptionControls(WeaponAction weapon)
    {
        if (weapon == null)
            return false;

        if (!weapon.CanInterrupt)
            return false;

        if (!validator.CanInterrupt(weapon))
            return false;

        if (!validator.CanActivate(weapon, skipContextCheck: true))
            return false;

        return true;
    }

    // ============================================================================
    // STATE MANAGEMENT
    // ============================================================================

    void StorePreviousAction()
    {
        previousAction = instance.Action;
    }

    void ResetState()
    {
        instance.State.Reset();
    }

    void ClearInstance()
    {
        instance = null;
    }

    // ============================================================================
    //  Release Predicates
    // ============================================================================

    bool ShouldReleaseWeapon()
    {

        if (instance.Action.Activation == WeaponActivation.WhileHeld && instance.State.Phase == WeaponPhase.Fire)
            return false;

        if (instance.ShouldValidateActivationTriggers() && !HasAllRequiredTriggers(instance.Action))
            return true;

        if (ShouldTerminate())
            return true;

        if (instance.State.ReadyToRelease)
            return true;

        return false;
    }

    bool ShouldTerminate()
    {
        return instance.Action.Termination switch
        {
            WeaponTermination.OnRelease     => instance.Action.Trigger.Any(trigger => !IsTriggerActive(trigger)),
            WeaponTermination.OnRootRelease => instance.Action.RequiredHeldTriggers.Any(trigger => !IsTriggerActive(trigger)),
            _ => false,
        };
    }


    // ============================================================================
    // EFFECT MANAGEMENT
    // ============================================================================

    public void RequestEffects(WeaponAction action)
    {
        foreach (var effect in action.Effects)
        {
            if (ShouldApplyEffect(effect))
                EmitLocal<EffectRequest>(new(Guid.NewGuid(), Request.Create, new() { Effect = effect }));
        }
    }

    public void CancelEffects(WeaponAction action)
    {
        foreach (var effect in action?.Effects)
        {
            if (effect.Cancelable)
                EmitLocal<EffectRequest>(new(Guid.NewGuid(), Request.Cancel, new() { Instance = instance, Effect = effect }));
        }
    }

    public void CancelOnReleaseEffects(WeaponAction action)
    {
        foreach (var effect in action.Effects)
        {
            if (effect.Cancelable && effect is ICancelableOnRelease cancelable && cancelable.CancelOnRelease)
                EmitLocal<EffectRequest>(new(Guid.NewGuid(), Request.Cancel, new() { Instance = instance, Effect = effect }));
        }
    }

    bool ShouldApplyEffect(Effect effect)
    {
        if (effect is ITrigger trigger)
            return trigger.Trigger == instance.State.Phase;

        return instance.State.Phase == WeaponPhase.Enable;
    }


    
    // ============================================================================
    // MOVEMENT MANAGEMENT
    // ============================================================================

    public void RequestMovement(WeaponAction action)
    {
        foreach (var definition in action.MovementDefinitions)
        {
            if (definition.Phase != instance.State.Phase)
                continue;

            EmitLocal<MovementRequest>(new(Guid.NewGuid(), Request.Create, new() { Owner = owner, Scope = (int)instance.State.Phase, Command = MovementCommandFactory.Create(owner, definition, instance.State.Intent)}));
        }
    }

    public void ClearMovementFromPhase(WeaponPhase scopeToClear)
    {
        EmitLocal<MovementRequest>(new(Guid.NewGuid(), Request.Clear, new() { Owner = owner, Scope = (int)scopeToClear }));
    }
    
    public void ClearMovementFromOwner()
    {
        EmitLocal<MovementRequest>(new(Guid.NewGuid(), Request.Clear, new() { Owner = owner, Scope = -1 }));
    }

    // ============================================================================
    // HITBOX MANAGEMENT
    // ============================================================================

    public void RequestHitboxes(WeaponAction action)
    {
        foreach (var hitboxDefinition in action.Hitboxes)
        {
            if (instance.State.Phase == hitboxDefinition.Phase)
                hitboxEvents.Send(new(Guid.NewGuid(), Request.Create, new() { Owner = owner, Definition = hitboxDefinition, Input = instance.State.Intent }));
        }
    }

    public void DestroyHitboxes(WeaponInstance instance)
    {
        foreach (var (hitboxId, definition) in instance.State.OwnedHitboxes)
            EmitGlobal<HitboxRequest>(new(Guid.NewGuid(), Request.Destroy, new() { Owner = owner, Definition = definition, HitboxId = hitboxId }));
    }

    // ============================================================================
    // Animation System
    // ============================================================================

    public void RequestAnimation(WeaponAction action)
    {
        AnimatorRequest request = instance.State.Phase switch
        {
            WeaponPhase.Charging    => action.Animations.OnCharge,
            WeaponPhase.Fire        => action.Animations.OnFire,
            WeaponPhase.FireEnd     => action.Animations.OnFireEnd,
            _ => null
        };

        if (request == null)
            return;

        EmitLocal<AnimationRequest>(new(Guid.NewGuid(), Request.Start, new() { Owner = owner, Request = request }));
    }

    // ============================================================================
    // CONTROL SYSTEM
    // ============================================================================

    public void UpdateAvailableControls()
    {

        switch (instance.State.Phase)
        {
            case WeaponPhase.Charging:
                AddControls(instance.Action.AddControlOnCharge);
                RemoveControls(instance.Action.RemoveControlOnCharge);
                break;

            case WeaponPhase.Fire:
                AddControls(instance.Action.AddControlOnFire);
                RemoveControls(instance.Action.RemoveControlOnFire);
                break;

            case WeaponPhase.FireEnd:
                AddControls(instance.Action.AddControlOnFireEnd);
                RemoveControls(instance.Action.RemoveControlOnFireEnd);

                if (instance.Action.SwapOnFire?.Length > 0)
                    instance.State.AvailableControls.Add(instance.Action.SwapOnFire);
                break;
        }

        if (instance.State.AvailableControls.Count > 0)
            Log.Trace("Control.Available", () => $"{string.Join(", ", instance.State.AvailableControls)}");
    }

    void AddControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
            instance.State.AvailableControls.Add(control);
    }

    void RemoveControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
            instance.State.AvailableControls.Remove(control);
    }

    // ============================================================================
    // COMMAND MANAGEMENT
    // ============================================================================

    void ConsumeCommand(Command command)
    {
        EmitLocal<CommandRequest>(new(Guid.NewGuid(), Request.Consume, new() { Command = command }));
    }

    void ConsumeAllCommands(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var cmd))
                ConsumeCommand(cmd);
        }
    }

    void StoreCommandID(Command command)
    {
        instance.State.OwnedCommands.Add(command.RuntimeID);
    }

    void StoreAllCommandIDs(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var command))
                StoreCommandID(command);
        }
    }


    void StoreInputSnapshot(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var command))
                instance.State.Intent = command.Intent;
        }
    }


    void LockCommand(Command command)
    {
        EmitLocal<CommandRequest>(new(Guid.NewGuid(), Request.Lock, new() { Command = command }));
    }

    void LockAllCommands(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var cmd))
                LockCommand(cmd);
        }
    }

    void UnlockCommand(Command command)
    {
        EmitLocal<CommandRequest>(new(Guid.NewGuid(), Request.Unlock, new() { Command = command }));
    }

    void UnlockAllCommands(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var cmd))
                UnlockCommand(cmd);
        }
    }


    // ============================================================================
    // VALIDATION HELPERS
    // ============================================================================

    bool HasAllRequiredTriggers(WeaponAction weapon)
    {
        bool hasAll = weapon.Trigger.All(trigger => active.ContainsKey(trigger) || buffer.ContainsKey(trigger));
        return hasAll;
    }

    bool HasNewCommandForWeapon(WeaponAction weapon) => weapon.Trigger.Any(trigger => IsNewCommand(trigger));

    bool IsNewCommand(Capability trigger)
    {
        if (buffer.TryGetValue(trigger, out var command))
        {
            bool isNew = !instance.State.OwnedCommands.Contains(command.RuntimeID);
            return isNew;
        }
        return false;
    }

    public bool IsTriggerActive(Capability trigger) => active.ContainsKey(trigger) || buffer.ContainsKey(trigger);

    public bool OnlyCancelableLocksRemain() => NonCancelableAttackLocks == 0;

    // ============================================================================
    // EVENT HANDLERS
    // ============================================================================

    void HandleCommandPublish(CommandPublish evt)
    {
        active = evt.Payload.Active;
        buffer = evt.Payload.Buffer;
    }

    void HandleEffectNonCancelableLockCount(EffectPublish evt)
    {
        var effect = evt.Payload.Instance.Effect;

        if (effect is not IDisableAttack disable)
            return;

        bool isDefinitive = !effect.Cancelable;

        if (!isDefinitive || !disable.DisableAttack)
            return;

        switch (evt.Action)
        {
            case Publish.Activated:
                NonCancelableAttackLocks++;
                break;

            case Publish.Canceled:
            case Publish.Deactivated:
                NonCancelableAttackLocks--;
                break;
        }
    }

    void HandleLockPublish(LockPublish evt)
    {
        locks = evt.Payload.Locks;
    }

    void HandleEquipmentPublish(EquipmentPublish evt)
    {
        if (evt.Payload.Equipment is not Weapon weapon)
            return;

        switch(evt.Action)
        {
            case Publish.Equipped:
                foreach (var action in weapon.Definition.Actions)
                    loadout.AddAction(action.Value, owner);
                break;
                
            case Publish.Unequipped:
                foreach (var action in weapon.Definition.Actions)
                    loadout.RemoveAction(action.Key);
                break;
        }
    }
    
    void HandleHitboxResponse(HitboxRequest request, HitboxResponse response)
    {
        instance?.State.OwnedHitboxes.Add(response.Payload.HitboxId, response.Payload.Definition);
    }

    void PublishTransition()
    {
        EmitLocal<WeaponPublish>(new(Guid.NewGuid(), Publish.PhaseChange, new() { Owner = owner, Instance = instance }));
    }

    void RegisterCooldown()
    {
        if (instance.Action.Cooldown > 0)
            cooldown.RegisterWeapon(instance.Action);
    }
    // ============================================================================
    // QUERIES & ACCESSORS
    // ============================================================================

    public IActivationStrategy GetActivationStrategy(WeaponAction weapon) => activationStrategies[weapon.Activation];

    WeaponAction GetDefaultWeapon(Command command)
    {
        return loadout.DefaultWeapon(command.Action);
    }

    public bool HasActiveWeapon()   => instance != null;
    bool HasActiveCommands()        => active?.Count > 0;
    bool HasBufferCommands()        => buffer?.Count > 0;

    void LinkLocal <T>(Action<T> handler) where T : IEvent  => owner.Bus.Subscribe(handler);
    void EmitLocal <T>(T evt) where T : IEvent              => owner.Bus.Raise(evt);
    void LinkGlobal<T>(Action<T> handler) where T : IEvent  => EventBus<T>.Subscribe(handler);
    void EmitGlobal<T>(T evt) where T : IEvent              => EventBus<T>.Raise(evt);

    public Actor Owner                                                  => owner;
    public WeaponInstance CurrentWeapon                                 => instance;
    public WeaponCooldown Cooldown                                      => cooldown;
    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> Locks => locks;

    public UpdatePriority Priority => ServiceUpdatePriority.WeaponLogic;

    public WeaponAction PreviousAction => previousAction;

    void DebugLog()
    {
        Log.Debug("Weapon.Active",        () => instance?.Action.Name ?? "none" );
        Log.Debug("Weapon.Phase",         () => instance?.State.Phase.ToString() ?? "none" );
        Log.Trace("Commands.Active",      () => active?.Count > 0 ? string.Join(", ", active?.Keys) : "");
        Log.Trace("Commands.Buffered",    () => buffer?.Count > 0 ? string.Join(", ", buffer?.Keys) : "");
        Log.Trace("Locks.Active",         () => locks == null ? "<none>" : string.Join(", ", locks.Select(kvp => $"{kvp.Key}({kvp.Value.Count})")) ); 
        Log.Trace("Locks.NonCancelable",  () => NonCancelableAttackLocks );
        Log.Trace("Cooldown",             () => cooldown.IsOnCooldown(instance?.Action.Name) ?  $"Remaining: {cooldown.GetRemainingCooldown(instance?.Action.Name)}" : "Ready");
    }
}

// ============================================================================
// SUPPORTING TYPES
// ============================================================================

public readonly struct WeaponStatePayload
{
    public readonly Actor Owner             { get; init; }
    public readonly WeaponInstance Instance { get; init; }
}

public readonly struct WeaponPublish : ISystemEvent
{
    public Guid Id                          { get; }
    public Publish Action                   { get; }
    public WeaponStatePayload Payload       { get; }

    public WeaponPublish(Guid id, Publish action, WeaponStatePayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}

// ============================================================================
// COOLDOWN SYSTEM
// ============================================================================

public class WeaponCooldownInstance
{
    public string name;
    public WeaponAction weapon;
    public DualCountdown timer;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public WeaponCooldownInstance(WeaponAction instance)
    {
        name    = instance.Name;
        weapon  = instance;
        timer   = new(instance.Cooldown);
    }

    public void Initialize()
    {
        timer.OnTimerStart += OnApply;
        timer.OnTimerStop  += OnClear;
        timer.Start();
    }

    public void Cancel()
    {
        timer.Cancel();
        OnCancel?.Invoke();
    }
}

public class WeaponCooldown
{
    List<WeaponCooldownInstance> cooldowns = new();

    public void RegisterWeapon(WeaponAction weapon)
    {
        var instance = new WeaponCooldownInstance(weapon);

        instance.OnClear  += () => cooldowns.Remove(instance);
        instance.OnCancel += () => cooldowns.Remove(instance);

        cooldowns.Add(instance);
        instance.Initialize();
    }

    public bool IsOnCooldown(string weaponName) => cooldowns.Any(instance => instance.name == weaponName);
    public float GetRemainingCooldown(string weaponName) => cooldowns.FirstOrDefault(instance => instance.name == weaponName)?.timer.CurrentTime ?? 0f;
}


// ============================================================================
// PHASE HANDLERS
// ============================================================================

public interface IWeaponPhaseHandler
{
    WeaponPhase Phase { get; }

    void Enter(WeaponInstance instance, WeaponSystem controller);
    void Tick (WeaponInstance instance, WeaponSystem controller);
    void Exit (WeaponInstance instance, WeaponSystem controller);
}

public class EnablePhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Enable;

    public void Enter(WeaponInstance instance, WeaponSystem controller)
    {
        instance.State.PhaseFrames.Start();

        if (controller.PreviousAction == null)
            return;

        controller.CancelEffects(controller.PreviousAction);
    }

    public void Tick(WeaponInstance instance, WeaponSystem controller)
    {
        controller.TransitionTo(WeaponPhase.Charging);
    }

    public void Exit(WeaponInstance instance, WeaponSystem controller)
    {
        controller.ClearMovementFromPhase(Phase);
        controller.RequestEffects(instance.Action);
    }
}

public class ChargingPhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Charging;

    public void Enter(WeaponInstance instance, WeaponSystem controller)
    {        
        instance.State.PhaseFrames .Restart();
        instance.State.ActiveFrames.Start();

        controller.UpdateAvailableControls();

        controller.RequestEffects(instance.Action);
        controller.RequestMovement(instance.Action);
        controller.RequestHitboxes(instance.Action);
        controller.RequestAnimation(instance.Action);
    }

    public void Tick(WeaponInstance instance, WeaponSystem controller)
    {
        var strategy = controller.GetActivationStrategy(instance.Action);
        
        if (strategy.ShouldFireFromCharging(instance))
            controller.TransitionTo(WeaponPhase.Fire);
    }
    
    public void Exit(WeaponInstance instance, WeaponSystem controller)
    {
        controller.ClearMovementFromPhase(Phase);
    }
}

public class FirePhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Fire;

    public void Enter(WeaponInstance instance, WeaponSystem controller)
    {        
        instance.State.HasFired = true;

        instance.State.PhaseFrames .Restart();;
        instance.State.ActiveFrames.Start();

        controller.UpdateAvailableControls();

        controller.RequestEffects(instance.Action);
        controller.RequestMovement(instance.Action);
        controller.RequestHitboxes(instance.Action);
        controller.RequestAnimation(instance.Action);
    }

    public void Tick(WeaponInstance instance, WeaponSystem controller)
    {
        if (instance.Action.Activation == WeaponActivation.WhileHeld)
            return;

        if (instance.IsFireComplete())
            controller.TransitionTo(WeaponPhase.FireEnd);
    }
    
    public void Exit(WeaponInstance instance, WeaponSystem controller)
    {
        controller.ClearMovementFromPhase(Phase);
    }
}

public class FireEndPhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.FireEnd;

    public void Enter(WeaponInstance instance, WeaponSystem controller)
    {        
        instance.State.PhaseFrames.Reset();
        instance.State.PhaseFrames.Start();

        if (instance.Action.ControlWindow > 0)
        {
            instance.State.ControlWindow = new ClockTimer(instance.Action.ControlWindow);
            instance.State.ControlWindow.Start();
        }

        controller.UpdateAvailableControls();

        controller.RequestEffects(instance.Action);
        controller.RequestMovement(instance.Action);
        controller.RequestHitboxes(instance.Action);
        controller.RequestAnimation(instance.Action);
    }

    public void Tick(WeaponInstance instance, WeaponSystem controller)
    {
        if (IsComplete(instance))
            controller.TransitionTo(WeaponPhase.Disable);
    }

    bool IsComplete(WeaponInstance instance)
    {
        if (instance.State.ControlWindow != null)
            return instance.State.ControlWindow.IsFinished;
        return true;
    }
    
    public void Exit(WeaponInstance instance, WeaponSystem controller)
    {
        controller.ClearMovementFromPhase(Phase);
    }
}

public class DisablePhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Disable;

    public void Enter(WeaponInstance instance, WeaponSystem controller)
    {
        instance.State.PhaseFrames.Reset();
        instance.State.PhaseFrames.Start();
        
        controller.CancelOnReleaseEffects(instance.Action);
        controller.DestroyHitboxes(instance);

        controller.ClearMovementFromOwner();
    }
    public void Tick(WeaponInstance instance, WeaponSystem controller)
    {
        instance.State.ReadyToRelease = true;
    }

    public void Exit(WeaponInstance instance, WeaponSystem controller)
    {

    }
}


// ============================================================================
// ACTIVATION STRATEGIES
// ============================================================================

public interface IActivationStrategy
{
    bool ShouldFireFromCharging(WeaponInstance weapon);
    bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponSystem controller);
    bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponSystem controller);
}

public class OnPressActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon) => weapon.IsChargeComplete();
    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponSystem controller) => false;
    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponSystem controller) => false;
}

public class OnChargeActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon) => weapon.IsChargeComplete();
    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponSystem controller) => false;
    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponSystem controller) => false;
}

public class OnReleaseActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon)
    {
        return weapon.IsChargeComplete() && weapon.Action.ForceMaxChargeRelease;
    }

    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponSystem controller)
    {
        if (weapon.Action.Trigger.Any(trigger => !controller.IsTriggerActive(trigger)))
        {
            if (weapon.GetChargePercent() >= weapon.Action.MinimumChargeToFire)
            {
                controller.TransitionTo(WeaponPhase.Fire);
                return true;
            }
        }
        return false;
    }

    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponSystem controller) => false;
}

public class WhileHeldActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon) => weapon.IsChargeComplete();

    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponSystem controller) => false;

    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponSystem controller)
    {
        if (weapon.Action.Trigger.Any(trigger => !controller.IsTriggerActive(trigger)))
        {
            controller.TransitionTo(WeaponPhase.FireEnd);
            return true;
        }
        return false;
    }
}

// ============================================================================
// WEAPON VALIDATION SYSTEM
// ============================================================================

public readonly struct WeaponValidation
{
    public readonly Response    Response;
    public readonly string      Reason;

    private WeaponValidation(Response response, string reason)
    {
        Response    = response;
        Reason      = reason;
    }

    public static WeaponValidation Pass()               => new(Response.Success, "");
    public static WeaponValidation Fail(string reason)  => new(Response.Failure, reason);

    public bool Success() => Response == Response.Success;
    public bool Failure() => Response == Response.Failure;
}

public class WeaponActivationValidator
{
    readonly Logger Log = Logging.For(LogSystem.Weapons);

    readonly WeaponSystem controller;

    public WeaponActivationValidator(WeaponSystem controller)
    {
        this.controller = controller;
    }
 
    public bool CanActivate(WeaponAction weapon, bool skipContextCheck = false)
    {
        var result = ValidateActivation(weapon, skipContextCheck);

        if (!result.Success())
            Log.Debug("Validator.Failed", () => $"{weapon.Name} - {result.Reason}");

        return result.Success();
    }


    public WeaponValidation ValidateActivation(WeaponAction weapon, bool skipContextCheck = false)
    {
        WeaponValidation result;

        if (!(result = CheckCooldown(weapon)).Success())              return result;
        if (!(result = CheckActivationCondition(weapon)).Success())   return result;
        if (!(result = CheckActionLocks(weapon)).Success())           return result;
        if (!(result = CheckNonCancelableLocks(weapon)).Success())    return result;
        if (!skipContextCheck &&
            !(result = CheckContext(weapon)).Success())               return result;

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckCooldown(WeaponAction weapon)
    {
        if (controller.Cooldown.IsOnCooldown(weapon.Name))
        {
            float remaining = controller.Cooldown.GetRemainingCooldown(weapon.Name);
            return WeaponValidation.Fail($"Cooldown {remaining:F2}s remaining");
        }

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckActivationCondition(WeaponAction weapon)
    {
        if (weapon.Condition.Activate != null &&
            !weapon.Condition.Activate(controller.Owner))
        {
            return WeaponValidation.Fail("Activate condition returned false");
        }

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckActionLocks(WeaponAction weapon)
    {
        if (!weapon.AcceptTriggerLockRequests)
            return WeaponValidation.Pass();

        if (weapon.CanCancelDisables)
            return WeaponValidation.Pass();

        foreach (var trigger in weapon.Trigger)
        {
            if (controller.Locks != null && controller.Locks.TryGetValue(trigger, out var lockList) && lockList.Count > 0)
                return WeaponValidation.Fail($"Trigger {trigger} has {lockList.Count} lock(s)");
        }

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckNonCancelableLocks(WeaponAction incomingWeapon)
    {
        if (controller.HasActiveWeapon() && !controller.OnlyCancelableLocksRemain())
            return WeaponValidation.Fail($"{controller.NonCancelableAttackLocks} non-cancelable lock(s) active");

        return WeaponValidation.Pass();
    }


    WeaponValidation CheckContext(WeaponAction weapon)
    {
        if (!weapon.CanCancelDisables &&  controller.Owner is IAttacker actor && !actor.CanAttack)
            return WeaponValidation.Fail($"Context disallows attack (CanCancelDisables={weapon.CanCancelDisables})");

        return WeaponValidation.Pass();
    }


    public bool CanInterrupt(WeaponAction incomingWeapon)
    {
        var result = ValidateInterrupt(incomingWeapon);

        if (!result.Success())
            Log.Trace("Weapon.Interrupt", () => result.Reason);

        return result.Success();
    }

    public WeaponValidation ValidateInterrupt(WeaponAction incomingWeapon)
    {
        if (!controller.HasActiveWeapon())
            return WeaponValidation.Pass();

        if (!controller.OnlyCancelableLocksRemain() && !incomingWeapon.CanCancelDisables)
            return WeaponValidation.Fail("Non-cancelable locks remain and weapon cannot cancel disables");

        bool canCancelViaCondition  = controller.CurrentWeapon.Action.Condition.Cancel != null && controller.CurrentWeapon.Action.Condition.Cancel(controller.Owner);
        bool canCancelViaDisable    = incomingWeapon.CanCancelDisables;

        if (canCancelViaCondition || canCancelViaDisable)
            return WeaponValidation.Pass();

        return WeaponValidation.Fail("No valid cancel path");
    }
}

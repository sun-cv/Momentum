using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;




// ============================================================================
// PHASE HANDLERS
// ============================================================================

public interface IWeaponPhaseHandler
{
    void Enter(WeaponInstance weapon, WeaponSystem controller);
    void Update(WeaponInstance weapon, WeaponSystem controller);
    WeaponPhase Phase { get; }
}

public class ChargingPhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Charging;

    public void Enter(WeaponInstance weapon, WeaponSystem controller)
    {        
        weapon.State.PhaseFrames.Reset();
        weapon.State.PhaseFrames.Start();
        weapon.State.ActiveFrames.Start();

        controller.UpdateAvailableControls();
        controller.PushEffects();
    }

    public void Update(WeaponInstance weapon, WeaponSystem controller)
    {
        var strategy = controller.GetActivationStrategy(weapon.Action);
        
        if (strategy.ShouldFireFromCharging(weapon))
            controller.TransitionTo(WeaponPhase.Fire);
    }
}

public class FirePhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Fire;

    public void Enter(WeaponInstance weapon, WeaponSystem controller)
    {        
        weapon.State.HasFired = true;
        weapon.State.PhaseFrames.Reset();
        weapon.State.PhaseFrames.Start();

        controller.UpdateAvailableControls();
        controller.PushEffects();
    }

    public void Update(WeaponInstance weapon, WeaponSystem controller)
    {
        if (weapon.Action.Activation == WeaponActivation.WhileHeld)
            return;

        if (weapon.IsFireComplete())
            controller.TransitionTo(WeaponPhase.FireEnd);
    }
}

public class FireEndPhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.FireEnd;

    public void Enter(WeaponInstance weapon, WeaponSystem controller)
    {        
        weapon.State.PhaseFrames.Reset();
        weapon.State.PhaseFrames.Start();

        if (weapon.Action.ControlWindow > 0)
        {
            weapon.State.ControlWindow = new ClockTimer(weapon.Action.ControlWindow);
            weapon.State.ControlWindow.Start();
            Log.Trace(LogSystem.Weapon, LogCategory.Control, "Weapon Trace", "Weapon.Window", () => $"Started {weapon.Action.ControlWindow}s window");
        }

        controller.UpdateAvailableControls();
        controller.PushEffects();
    }

    public void Update(WeaponInstance weapon, WeaponSystem controller)
    {
        if (IsComplete(weapon))
            weapon.State.ReadyToRelease = true;
    }

    bool IsComplete(WeaponInstance weapon)
    {
        if (weapon.State.ControlWindow != null)
            return weapon.State.ControlWindow.IsFinished;
        return true;
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
    readonly WeaponSystem controller;

    public WeaponActivationValidator(WeaponSystem controller)
    {
        this.controller = controller;
    }
 
    public bool CanActivate(WeaponAction weapon, bool skipContextCheck = false)
    {
        var result = ValidateActivation(weapon, skipContextCheck);

        if (!result.Success())
            Log.Debug(LogSystem.Weapon, LogCategory.Validation, "Weapon Debug", "Weapon.Validator.Failed", () => $"{weapon.Name} - {result.Reason}");

        return result.Success();
    }


    public WeaponValidation ValidateActivation(WeaponAction weapon, bool skipContextCheck = false)
    {
        WeaponValidation result;

        if (!(result = CheckCooldown(weapon)).Success())              return result;
        if (!(result = CheckActivationCondition(weapon)).Success())   return result;
        if (!(result = CheckActionLocks(weapon)).Success())           return result;
        if (!(result = CheckNonCancelableLocks()).Success())          return result;
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

        foreach (var trigger in weapon.Trigger)
        {
            if (controller.Locks != null && controller.Locks.TryGetValue(trigger, out var lockList) && lockList.Count > 0)
                return WeaponValidation.Fail($"Trigger {trigger} has {lockList.Count} lock(s)");
        }

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckNonCancelableLocks()
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
            Log.Debug(LogSystem.Weapon, LogCategory.Validation, "Weapon Trace", "Weapon.Interrupt", () => result.Reason);

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

// ============================================================================
// MAIN WEAPON CONTROLLER
// ============================================================================


public class WeaponSystem : IServiceTick
{
    Actor                                                   owner;
    WeaponLoadout                                           loadout;
    WeaponInstance                                          instance;

    WeaponCooldown                                          cooldown;
    WeaponActivationValidator                               validator;
    Dictionary<WeaponPhase, IWeaponPhaseHandler>            phaseHandlers;
    Dictionary<WeaponActivation, IActivationStrategy>       activationStrategies;

    IReadOnlyDictionary<Capability, Command>                active;
    IReadOnlyDictionary<Capability, Command>                buffer;
    IReadOnlyDictionary<Capability, IReadOnlyList<string>>  locks;

    public int NonCancelableAttackLocks { get; set; } = 0;

    EventCache<HitboxRequest, HitboxResponse>               hitboxEventCache;

    public WeaponSystem(Actor actor)
    {
        GameTick.Register(this);

        loadout     = new();
        cooldown    = new();
        validator   = new(this);
        
        owner = actor;

        InitializePhaseHandlers();
        InitializeActivationStrategies();

        hitboxEventCache = new(HandleHitboxResponse);

        EventBus<CommandPublish>        .Subscribe(HandleCommandPublish);
        EventBus<EffectPublish>         .Subscribe(HandleEffectNonCancelableLockCount);
        EventBus<LockPublish>           .Subscribe(HandleLockPublish);
        EventBus<EquipmentPublish>      .Subscribe(HandleEquipmentPublish);
    }


    void InitializePhaseHandlers()
    {
        phaseHandlers = new()
        {
            { WeaponPhase.Charging,         new ChargingPhaseHandler() },
            { WeaponPhase.Fire,             new FirePhaseHandler() },
            { WeaponPhase.FireEnd,          new FireEndPhaseHandler() }
        };
    }

    void InitializeActivationStrategies()
    {
        activationStrategies = new()
        {
            { WeaponActivation.OnPress,     new OnPressActivationStrategy() },
            { WeaponActivation.OnCharge,    new OnChargeActivationStrategy() },
            { WeaponActivation.OnRelease,   new OnReleaseActivationStrategy() },
            { WeaponActivation.WhileHeld,   new WhileHeldActivationStrategy() }
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
        {
            handler.Update(instance, this);
        }
    }

    bool ShouldReleaseWeapon()
    {

        if (instance.Action.Activation == WeaponActivation.WhileHeld && instance.State.Phase == WeaponPhase.Fire)
        {
            return false;
        }

        if (instance.ShouldValidateActivationTriggers() && !HasAllRequiredTriggers(instance.Action))
        {
            Log.Debug(LogSystem.Weapon, LogCategory.State, "Weapon Trace", "Weapon.State.Release", () => "Missing required actions");
            return true;
        }

        if (ShouldTerminate())
        {
            Log.Debug(LogSystem.Weapon, LogCategory.State, "Weapon Trace", "Weapon.State.Release", () => "Termination condition met");
            return true;
        }

        if (instance.State.ReadyToRelease)
            return true;

        return false;
    }

    bool ShouldTerminate()
    {
        return instance.Action.Termination switch
        {
            WeaponTermination.OnRelease => instance.Action.Trigger.Any(trigger => !IsTriggerActive(trigger)),
            WeaponTermination.OnRootRelease => instance.Action.RequiredHeldTriggers.Any(trigger => !IsTriggerActive(trigger)),
            _ => false,
        };
    }

    // REWORK REQUIRED ONEVENT HITBOX CREATION

    public void TransitionTo(WeaponPhase newPhase)
    {
        instance.State.Phase = newPhase;

        PublishWeaponTransition();
        RequestHitboxes();
        EnterHandler();
    }

    void EnterHandler()
    {
        if (phaseHandlers.TryGetValue(instance.State.Phase, out var handler))
            handler.Enter(instance, this);
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
            var newWeapon = GetDefaultWeapon(command);

            if (newWeapon == null)
                continue;

            if (!validator.CanActivate(newWeapon))
                continue;

            ReplaceAndActivateWeapon(newWeapon);
            return true;
        }

        return false;
    }

    bool TryActivateInterruptWeapon()
    {

        foreach (var command in buffer.Values)
        {
            var newWeapon = GetDefaultWeapon(command);

            if (newWeapon == null)
                continue;

            if (!newWeapon.CanInterrupt)
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Activation.Interrupt", () => $"{newWeapon.Name} - not an interrupt weapon");
                continue;
            }

            if (!validator.CanInterrupt(newWeapon))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Activation.Interrupt", () => $"{newWeapon.Name} - cannot interrupt current weapon");
                continue;
            }

            if (!validator.CanActivate(newWeapon, skipContextCheck: true))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Activation.Interrupt", () => $"{newWeapon.Name} - cannot activate");
                continue;
            }

            Log.Debug(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Activation.Interrupt", () => $"SUCCESS - {newWeapon.Name}");
            ReplaceAndActivateWeapon(newWeapon);
            return true;
        }

        return false;
    }

    bool TryActivateFromAvailableControls()
    {
        if (instance.State.AvailableControls.Count == 0)
            return false;

        foreach (var weaponName in instance.State.AvailableControls)
        {
            if (!loadout.TryGetAction(weaponName, out var availableAction))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Weapon.controls", () => $"{weaponName} - not found in weapon loadout");
                continue;
            }

            bool isOnHeld = availableAction.Availability == WeaponAvailability.OnHeld;
            bool isChained = weaponName == instance.Action.SwapOnFire;

            if (!HasAllRequiredTriggers(availableAction))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Weapon.controls", () => $"{weaponName} - missing required inputs");
                continue;
            }

            if (!validator.CanActivate(availableAction))
                continue;

            if (isOnHeld)
            {
                Log.Debug(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Weapon.controls", () => $"SUCCESS - {weaponName} (OnHeld)");
                ReplaceAndActivateWeapon(availableAction);
                return true;
            }

            if (!HasNewCommandForWeapon(availableAction))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon Trace", "Weapon.controls", () => $"{weaponName} - no new press");
                continue;
            }

            string mode = isChained ? "Chained" : "Control";
            ReplaceAndActivateWeapon(availableAction);
            return true;
        }

        return false;
    }



    void ReplaceAndActivateWeapon(WeaponAction weapon)
    {
        if (HasActiveWeapon())
            ReleaseWeapon();

        EquipWeapon(weapon);

        if (weapon.Availability == WeaponAvailability.OnHeld)
            ActivateHeldWeapon();
        else
            ActivateWeapon();

        EnableWeapon();
    }

    void EquipWeapon(WeaponAction Action)
    {
        instance = new WeaponInstance(Action);
        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.Equipped, new() { Owner = owner, Instance = instance }));
    }

    void ActivateWeapon()
    {
        ConsumeAllCommands(buffer, instance.Action.Trigger);
        StoreAllCommandIDs(active, instance.Action.Trigger);

        if (instance.Action.LockTriggerAction)
            LockAllCommands(active, instance.Action.Trigger);
    }

    void ActivateHeldWeapon()
    {
        StoreAllCommandIDs(active, instance.Action.Trigger);
    }



    void EnableWeapon()
    {
        Log.Trace(LogSystem.Weapon, LogCategory.State, "Weapon Trace", "Weapon.Status.Enable", () => $"{instance.Action.Name}");

        PushEffects();

        instance.State.PhaseFrames.Start();
        instance.State.ActiveFrames.Start();

        TransitionTo(WeaponPhase.Charging);
    }

    void ReleaseWeapon()
    {
        Log.Trace(LogSystem.Weapon, LogCategory.State, "Weapon Trace", "Weapon.Status.Release", () => $"{instance.Action.Name}");

        CancelEffects();
        DestroyHitboxes();

        if (instance.Action.Cooldown > 0)
            cooldown.RegisterWeapon(instance.Action);

        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.Released, new() { Owner = owner, Instance = instance }));

        instance.State.Reset();
        instance = null;
    }

    // ============================================================================
    // EFFECT MANAGEMENT
    // ============================================================================

    public void PushEffects()
    {
        int pushed = 0;
        foreach (var effect in instance.Action.Effects)
        {
            if (ShouldApplyEffect(effect))
            {
                OnEvent<EffectRequest>(new(Guid.NewGuid(), Request.Create, new() { Effect = effect }));
                pushed++;
            }
        }
    }

    bool ShouldApplyEffect(Effect effect)
    {
        if (effect is ITrigger trigger)
            return trigger.Trigger == instance.State.Phase;

        return instance.State.Phase == WeaponPhase.Idle;
    }

    void CancelEffects()
    {
        foreach (var effect in instance.Action.Effects)
        {
            if (effect.Cancelable && effect is ICancelableOnRelease cancelable && cancelable.CancelOnRelease)
                OnEvent<EffectRequest>(new(Guid.NewGuid(), Request.Cancel, new() { Instance = instance, Effect = effect }));
        }
    }
    
    // ============================================================================
    // HITBOX MANAGEMENT
    // ============================================================================

    void RequestHitboxes()
    {
        foreach (var hitboxDefinition in instance.Action.Hitboxes)
        {
            if (instance.State.Phase == hitboxDefinition.Phase)
                OnEvent<HitboxRequest>(new(Guid.NewGuid(), Request.Create, new() { Owner = owner, Definition = hitboxDefinition }));
        }
    }

    void DestroyHitboxes()
    {
        foreach (var (hitboxId, definition) in instance.State.OwnedHitboxes)
            OnEvent<HitboxRequest>(new(Guid.NewGuid(), Request.Destroy, new() { Owner = owner, Definition = definition, HitboxId = hitboxId }));
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
            Log.Trace(LogSystem.Weapon, LogCategory.Control,"Weapon Trace", "Control.Available", () => $"{string.Join(", ", instance.State.AvailableControls)}");
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
        OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Consume, new() { Command = command }));
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

    void LockCommand(Command command)
    {
        OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Lock, new() { Command = command }));
    }

    void LockAllCommands(IReadOnlyDictionary<Capability, Command> commands, List<Capability> actions)
    {
        foreach (var action in actions)
        {
            if (commands.TryGetValue(action, out var cmd))
                LockCommand(cmd);
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
        instance.State.OwnedHitboxes.Add(response.Payload.HitboxId, response.Payload.Definition);
    }

    void PublishWeaponTransition()
    {
        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.PhaseChange, new() { Owner = owner, Instance = instance }));
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

    void OnEvent<T>(T evt) where T : IEvent => EventBus<T>.Raise(evt);

    public Actor Owner                                                  => owner;
    public WeaponInstance CurrentWeapon                                 => instance;
    public WeaponCooldown Cooldown                                      => cooldown;
    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> Locks => locks;

    public UpdatePriority Priority => ServiceUpdatePriority.WeaponLogic;


    void DebugLog()
    {
        Log.Debug(LogSystem.Weapon, LogCategory.State,          "Weapon Debug", "Weapon.Active",        () => instance?.Action.Name ?? "none" );
        Log.Trace(LogSystem.Weapon, LogCategory.State,          "Weapon Debug", "Weapon.Phase",         () => instance?.State.Phase.ToString() ?? "none" );
        Log.Trace(LogSystem.Weapon, LogCategory.Input,          "Weapon Trace", "Commands.Active",      () => active?.Count > 0 ? string.Join(", ", active?.Keys) : "");
        Log.Trace(LogSystem.Weapon, LogCategory.Input,          "Weapon Trace", "Commands.Buffered",    () => buffer?.Count > 0 ? string.Join(", ", buffer?.Keys) : "");
        Log.Trace(LogSystem.Weapon, LogCategory.Validation,     "Weapon Trace", "Locks.NonCancelable",  () => NonCancelableAttackLocks );
        Log.Trace(LogSystem.Weapon, LogCategory.Validation,     "Weapon Trace", "Locks.Active",         () => locks == null ? "<none>" : string.Join(", ", locks.Select(kvp => $"{kvp.Key}({kvp.Value.Count})")) ); 
        Log.Trace(LogSystem.Weapon, LogCategory.Validation,     "Weapon Trace", "Cooldown",             () => cooldown.IsOnCooldown(instance?.Action.Name) ?  $"Remaining: {cooldown.GetRemainingCooldown(instance.Action.Name)}" : "Ready");
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
    public Guid Id                      { get; }
    public Publish Action               { get; }
    public WeaponStatePayload Payload   { get; }

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
        timer.OnTimerStop += OnClear;
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

        instance.OnClear += () => cooldowns.Remove(instance);
        instance.OnCancel += () => cooldowns.Remove(instance);

        cooldowns.Add(instance);
        instance.Initialize();
    }

    public bool IsOnCooldown(string weaponName) => cooldowns.Any(instance => instance.name == weaponName);
    public float GetRemainingCooldown(string weaponName) => cooldowns.FirstOrDefault(instance => instance.name == weaponName)?.timer.CurrentTime ?? 0f;
}

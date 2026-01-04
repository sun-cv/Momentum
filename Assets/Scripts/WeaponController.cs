using System;
using System.Collections.Generic;
using System.Linq;


// ============================================================================
// WEAPON INSTANCE & STATE
// ============================================================================
public class WeaponInstance
{
    public Weapon Data          { get; init; }
    public WeaponState State    { get; init; }
    public WeaponInstance(Weapon data)
    {
        Data = data;
        State = new();
    }

    public int GetChargeFrames()
    {
        if (Data.ChargeTimeFrames > 0)
            return Data.ChargeTimeFrames;
        if (Data.ChargeTime > 0)
            return (int)(Data.ChargeTime * 60);
        return 0;
    }

    public int GetFireDurationFrames()
    {
        if (Data.FireDurationFrames > 0)
            return Data.FireDurationFrames;
        if (Data.FireDuration > 0)
            return (int)(Data.FireDuration * 60);
        return 0;
    }

    public float GetChargePercent()
    {
        int chargeFrames = GetChargeFrames();
        if (chargeFrames == 0)
            return 1.0f;
        return (float)State.PhaseFrames.CurrentFrame / chargeFrames;
    }

    public bool IsChargeComplete()  => State.PhaseFrames.CurrentFrame >= GetChargeFrames();
    public bool IsFireComplete()    => State.PhaseFrames.CurrentFrame >= GetFireDurationFrames();
    public bool ShouldValidateActions()
    {
        return Data.Activation switch
        {
            WeaponActivation.WhileHeld => true,
            WeaponActivation.OnRelease => State.Phase == WeaponPhase.Charging,
            _ => false,
        };
    }
}

public class WeaponState
{
    public WeaponPhase Phase                    { get; set; } = WeaponPhase.Idle;

    public FrameWatch PhaseFrames               { get; set; } = new();
    public FrameWatch ActiveFrames              { get; set; } = new();
    public ClockTimer ControlWindow             { get; set; }

    public HashSet<Guid> OwnedCommands          { get; set; } = new();
    public HashSet<string> AvailableControls    { get; set; } = new();
    
    public bool HasFired                        { get; set; }
    public bool ReadyToRelease                  { get; set; }

    public void Reset()
    {
        PhaseFrames.Reset();
        ActiveFrames.Reset();
        ControlWindow = null;

        Phase = WeaponPhase.Idle;
        HasFired = false;
        ReadyToRelease = false;

        OwnedCommands.Clear();
        AvailableControls.Clear();
    }
}

// ============================================================================
// PHASE HANDLERS
// ============================================================================

public interface IWeaponPhaseHandler
{
    void Enter(WeaponInstance weapon, WeaponController controller);
    void Update(WeaponInstance weapon, WeaponController controller);
    WeaponPhase Phase { get; }
}

public class ChargingPhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Charging;

    public void Enter(WeaponInstance weapon, WeaponController controller)
    {        
        weapon.State.PhaseFrames.Reset();
        weapon.State.PhaseFrames.Start();
        weapon.State.ActiveFrames.Start();

        controller.UpdateAvailableControls();
        controller.PushEffects();
    }

    public void Update(WeaponInstance weapon, WeaponController controller)
    {
        var strategy = controller.GetActivationStrategy(weapon.Data);
        
        if (strategy.ShouldFireFromCharging(weapon))
            controller.TransitionTo(WeaponPhase.Fire);
    }
}

public class FirePhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.Fire;

    public void Enter(WeaponInstance weapon, WeaponController controller)
    {        
        weapon.State.HasFired = true;
        weapon.State.PhaseFrames.Reset();
        weapon.State.PhaseFrames.Start();

        controller.UpdateAvailableControls();
        controller.PushEffects();
    }

    public void Update(WeaponInstance weapon, WeaponController controller)
    {
        if (weapon.Data.Activation == WeaponActivation.WhileHeld)
            return;

        if (weapon.IsFireComplete())
            controller.TransitionTo(WeaponPhase.FireEnd);
    }
}

public class FireEndPhaseHandler : IWeaponPhaseHandler
{
    public WeaponPhase Phase => WeaponPhase.FireEnd;

    public void Enter(WeaponInstance weapon, WeaponController controller)
    {        
        weapon.State.PhaseFrames.Reset();
        weapon.State.PhaseFrames.Start();

        if (weapon.Data.ControlWindow > 0)
        {
            weapon.State.ControlWindow = new ClockTimer(weapon.Data.ControlWindow);
            weapon.State.ControlWindow.Start();
            Log.Trace(LogSystem.Weapon, LogCategory.Control, "Weapon.Window", () => $"Started {weapon.Data.ControlWindow}s window");
        }

        controller.UpdateAvailableControls();
        controller.PushEffects();
    }

    public void Update(WeaponInstance weapon, WeaponController controller)
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
    bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponController controller);
    bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponController controller);
}

public class OnPressActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon) => weapon.IsChargeComplete();
    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponController controller) => false;
    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponController controller) => false;
}

public class OnChargeActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon) => weapon.IsChargeComplete();
    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponController controller) => false;
    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponController controller) => false;
}

public class OnReleaseActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon)
    {
        return weapon.IsChargeComplete() && weapon.Data.ForceMaxChargeRelease;
    }

    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponController controller)
    {
        if (weapon.Data.Action.Any(action => !controller.IsActionActive(action)))
        {
            if (weapon.GetChargePercent() >= weapon.Data.MinimumChargeToFire)
            {
                controller.TransitionTo(WeaponPhase.Fire);
                return true;
            }
        }
        return false;
    }

    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponController controller) => false;
}

public class WhileHeldActivationStrategy : IActivationStrategy
{
    public bool ShouldFireFromCharging(WeaponInstance weapon) => weapon.IsChargeComplete();

    public bool CheckReleaseTriggersInCharging(WeaponInstance weapon, WeaponController controller) => false;

    public bool CheckReleaseTriggersInFire(WeaponInstance weapon, WeaponController controller)
    {
        if (weapon.Data.Action.Any(action => !controller.IsActionActive(action)))
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
    readonly WeaponController controller;

    public WeaponActivationValidator(WeaponController controller)
    {
        this.controller = controller;
    }
 
    public bool CanActivate(Weapon weapon, bool skipContextCheck = false)
    {
        var result = ValidateActivation(weapon, skipContextCheck);

        if (!result.Success())
            Log.Debug( LogSystem.Weapon, LogCategory.Validation, "Weapon.Validator Failed", () => result.Reason);

        return result.Success();
    }


    public WeaponValidation ValidateActivation(Weapon weapon, bool skipContextCheck = false)
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

    WeaponValidation CheckCooldown(Weapon weapon)
    {
        if (controller.Cooldown.IsOnCooldown(weapon.Name))
        {
            float remaining = controller.Cooldown.GetRemainingCooldown(weapon.Name);
            return WeaponValidation.Fail($"Cooldown {remaining:F2}s remaining");
        }

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckActivationCondition(Weapon weapon)
    {
        if (weapon.Condition.Activate != null &&
            !weapon.Condition.Activate(controller.Context))
        {
            return WeaponValidation.Fail("Activate condition returned false");
        }

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckActionLocks(Weapon weapon)
    {
        if (!weapon.AcceptTriggerLockRequests)
            return WeaponValidation.Pass();

        foreach (var action in weapon.Action)
        {
            if (controller.Locks != null && controller.Locks.TryGetValue(action, out var lockList) && lockList.Count > 0)
                return WeaponValidation.Fail($"Action {action} has {lockList.Count} lock(s)");
        }

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckNonCancelableLocks()
    {
        if (controller.HasActiveWeapon() && !controller.OnlyCancelableLocksRemain())
            return WeaponValidation.Fail($"{controller.NonCancelableAttackLocks} non-cancelable lock(s) active");

        return WeaponValidation.Pass();
    }

    WeaponValidation CheckContext(Weapon weapon)
    {
        if (!weapon.CanCancelDisables && !controller.Context.CanAttack)
            return WeaponValidation.Fail($"Context disallows attack (CanCancelDisables={weapon.CanCancelDisables})");

        return WeaponValidation.Pass();
    }


    public bool CanInterrupt(Weapon incomingWeapon)
    {
        var result = ValidateInterrupt(incomingWeapon);

        if (!result.Success())
            Log.Debug(LogSystem.Weapon, LogCategory.Validation, "Weapon.Interrupt", () => result.Reason);

        return result.Success();
    }

    public WeaponValidation ValidateInterrupt(Weapon incomingWeapon)
    {
        if (!controller.HasActiveWeapon())
            return WeaponValidation.Pass();

        if (!controller.OnlyCancelableLocksRemain() && !incomingWeapon.CanCancelDisables)
            return WeaponValidation.Fail("Non-cancelable locks remain and weapon cannot cancel disables");

        bool canCancelViaCondition  = controller.CurrentWeapon.Data.Condition.Cancel != null && controller.CurrentWeapon.Data.Condition.Cancel(controller.Context);
        bool canCancelViaDisable    = incomingWeapon.CanCancelDisables;

        if (canCancelViaCondition || canCancelViaDisable)
            return WeaponValidation.Pass();

        return WeaponValidation.Fail("No valid cancel path");
    }
}

// ============================================================================
// MAIN WEAPON CONTROLLER
// ============================================================================



public class WeaponController : RegisteredService, IServiceTick
{
    Context                                                 context;
    WeaponSet                                               weaponSet;
    WeaponInstance                                          weapon;

    WeaponCooldown                                          cooldown;
    WeaponActivationValidator                               validator;
    Dictionary<WeaponPhase, IWeaponPhaseHandler>            phaseHandlers;
    Dictionary<WeaponActivation, IActivationStrategy>       activationStrategies;

    IReadOnlyDictionary<Capability, Command>                active;
    IReadOnlyDictionary<Capability, Command>                buffer;
    IReadOnlyDictionary<Capability, IReadOnlyList<string>>  locks;

    public int NonCancelableAttackLocks { get; set; } = 0;

    public override void Initialize()
    {
        cooldown = new();
        validator = new(this);

        InitializePhaseHandlers();
        InitializeActivationStrategies();

        EventBus<CommandPublish>.Subscribe(HandleCommandPublish);
        EventBus<EffectPublish>.Subscribe(HandleEffectNonCancelableLockCount);
        EventBus<LockPublish>.Subscribe(HandleLockPublish);
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

        var strategy = GetActivationStrategy(weapon.Data);

        if (weapon.State.Phase == WeaponPhase.Charging)
        {
            if (strategy.CheckReleaseTriggersInCharging(weapon, this))
                return;
        }
        else if (weapon.State.Phase == WeaponPhase.Fire)
        {
            if (strategy.CheckReleaseTriggersInFire(weapon, this))
                return;
        }

        if (phaseHandlers.TryGetValue(weapon.State.Phase, out var handler))
        {
            handler.Update(weapon, this);
        }
    }

    bool ShouldReleaseWeapon()
    {

        if (weapon.ShouldValidateActions() && !HasAllRequiredActions(weapon.Data))
        {
            Log.Debug(LogSystem.Weapon, LogCategory.State, "State.Release", () => "Missing required actions");
            return true;
        }

        if (ShouldTerminate())
        {
            Log.Debug(LogSystem.Weapon, LogCategory.State, "State.Release", () => "Termination condition met");
            return true;
        }

        if (weapon.State.ReadyToRelease)
            return true;

        return false;
    }

    bool ShouldTerminate()
    {
        return weapon.Data.Termination switch
        {
            WeaponTermination.OnRelease => weapon.Data.Action.Any(action => !IsActionActive(action)),
            WeaponTermination.OnRootRelease => weapon.Data.RequiredHeldActions.Any(action => !IsActionActive(action)),
            _ => false,
        };
    }

    public void TransitionTo(WeaponPhase newPhase)
    {
        
        weapon.State.Phase = newPhase;
        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.PhaseChange, new() { Weapon = weapon.Data, Phase = newPhase }));

        if (phaseHandlers.TryGetValue(newPhase, out var handler))
            handler.Enter(weapon, this);
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
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Activation.Interrupt", () => $"{newWeapon.Name} - not an interrupt weapon");
                continue;
            }

            if (!validator.CanInterrupt(newWeapon))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Activation.Interrupt", () => $"{newWeapon.Name} - cannot interrupt current weapon");
                continue;
            }

            if (!validator.CanActivate(newWeapon, skipContextCheck: true))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Activation.Interrupt", () => $"{newWeapon.Name} - cannot activate");
                continue;
            }

            Log.Debug(LogSystem.Weapon, LogCategory.Activation, "Activation.Interrupt", () => $"SUCCESS - {newWeapon.Name}");
            ReplaceAndActivateWeapon(newWeapon);
            return true;
        }

        return false;
    }

    bool TryActivateFromAvailableControls()
    {
        if (weapon.State.AvailableControls.Count == 0)
            return false;

        foreach (var weaponName in weapon.State.AvailableControls)
        {
            if (!weaponSet.weapons.TryGetValue(weaponName, out var availableWeapon))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon.Activation.Available", () => $"{weaponName} - not found in weaponset");
                continue;
            }

            bool isOnHeld = availableWeapon.Availability == WeaponAvailability.OnHeld;
            bool isChained = weaponName == weapon.Data.SwapOnFire;

            if (!HasAllRequiredActions(availableWeapon))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon.Activation.Available", () => $"{weaponName} - missing required inputs");
                continue;
            }

            if (!validator.CanActivate(availableWeapon))
                continue;

            if (isOnHeld)
            {
                Log.Debug(LogSystem.Weapon, LogCategory.Activation, "Weapon.Activation.Available", () => $"SUCCESS - {weaponName} (OnHeld)");
                ReplaceAndActivateWeapon(availableWeapon);
                return true;
            }

            if (!HasNewCommandForWeapon(availableWeapon))
            {
                Log.Trace(LogSystem.Weapon, LogCategory.Activation, "Weapon.Activation.Available", () => $"{weaponName} - no new press");
                continue;
            }

            string mode = isChained ? "Chained" : "Control";
            ReplaceAndActivateWeapon(availableWeapon);
            return true;
        }

        return false;
    }



    void ReplaceAndActivateWeapon(Weapon weaponData)
    {
        if (HasActiveWeapon())
            ReleaseWeapon();

        EquipWeapon(weaponData);

        if (weaponData.Availability == WeaponAvailability.OnHeld)
            ActivateHeldWeapon();
        else
            ActivateWeapon();

        EnableWeapon();
    }

    void EquipWeapon(Weapon weaponData)
    {
        Log.Debug(LogSystem.Weapon, LogCategory.State, "Weapon.Equip", () => weaponData.Name);
        weapon = new WeaponInstance(weaponData);
        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.Equipped, new() { Weapon = weapon.Data, Phase = WeaponPhase.Idle }));
    }

    void ActivateWeapon()
    {
        ConsumeAllCommands(buffer, weapon.Data.Action);
        StoreAllCommandIDs(active, weapon.Data.Action);

        if (weapon.Data.LockTriggerAction)
            LockAllCommands(active, weapon.Data.Action);
    }

    void ActivateHeldWeapon()
    {
        StoreAllCommandIDs(active, weapon.Data.Action);
    }

    void EnableWeapon()
    {
        Log.Debug(LogSystem.Weapon, LogCategory.State, "Weapon.Status.Enable", () => $"{weapon.Data.Name}");

        PushEffects();

        weapon.State.PhaseFrames.Start();
        weapon.State.ActiveFrames.Start();

        TransitionTo(WeaponPhase.Charging);
    }

    void ReleaseWeapon()
    {
        Log.Debug(LogSystem.Weapon, LogCategory.State, "Weapon.Status.Release", () => $"{weapon.Data.Name}");

        CancelEffects();

        if (weapon.Data.Cooldown > 0)
        {
            cooldown.RegisterWeapon(weapon.Data);
            Log.Trace(LogSystem.Weapon, LogCategory.Cooldown, () => $"Weapon.Cooldown.Register {weapon.Data.Name} - {weapon.Data.Cooldown}s");
        }

        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.Released, new() { Weapon = weapon.Data, Phase = weapon.State.Phase }));

        weapon.State.Reset();
        weapon = null;
    }

    // ============================================================================
    // EFFECT MANAGEMENT
    // ============================================================================

    public void PushEffects()
    {
        int pushed = 0;
        foreach (var effect in weapon.Data.Effects)
        {
            if (ShouldApplyEffect(effect))
            {
                OnEvent<EffectRequest>(new(Guid.NewGuid(), EffectAction.Create, new() { Effect = effect }));
                pushed++;
            }
        }
    }

    bool ShouldApplyEffect(Effect effect)
    {
        if (effect is ITrigger trigger)
            return trigger.Trigger == weapon.State.Phase;

        return weapon.State.Phase == WeaponPhase.Idle;
    }

    void CancelEffects()
    {
        foreach (var effect in weapon.Data.Effects)
        {
            if (effect.Cancelable && effect is ICancelableOnRelease instance && instance.CancelOnRelease)
                OnEvent<EffectRequest>(new(Guid.NewGuid(), EffectAction.Cancel, new() { Entity = weapon.Data, Effect = effect }));
        }
    }

    // ============================================================================
    // CONTROL SYSTEM
    // ============================================================================

    public void UpdateAvailableControls()
    {

        switch (weapon.State.Phase)
        {
            case WeaponPhase.Charging:
                AddControls(weapon.Data.AddControlOnCharge);
                RemoveControls(weapon.Data.RemoveControlOnCharge);
                break;

            case WeaponPhase.Fire:
                AddControls(weapon.Data.AddControlOnFire);
                RemoveControls(weapon.Data.RemoveControlOnFire);
                break;

            case WeaponPhase.FireEnd:
                AddControls(weapon.Data.AddControlOnFireEnd);
                RemoveControls(weapon.Data.RemoveControlOnFireEnd);

                if (weapon.Data.SwapOnFire?.Length > 0)
                    weapon.State.AvailableControls.Add(weapon.Data.SwapOnFire);
                break;
        }

        if (weapon.State.AvailableControls.Count > 0)
            Log.Debug(LogSystem.Weapon, LogCategory.Control, "Control.Available", () => $"{string.Join(", ", weapon.State.AvailableControls)}");
    }

    void AddControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
            weapon.State.AvailableControls.Add(control);
    }

    void RemoveControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
            weapon.State.AvailableControls.Remove(control);
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
        weapon.State.OwnedCommands.Add(command.RuntimeID);
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

    bool HasAllRequiredActions(Weapon weapon)
    {
        bool hasAll = weapon.Action.All(action => active.ContainsKey(action) || buffer.ContainsKey(action));
        return hasAll;
    }

    bool HasNewCommandForWeapon(Weapon weapon) => weapon.Action.Any(action => IsNewCommand(action));

    bool IsNewCommand(Capability action)
    {
        if (buffer.TryGetValue(action, out var command))
        {
            bool isNew = !weapon.State.OwnedCommands.Contains(command.RuntimeID);
            return isNew;
        }
        return false;
    }

    public bool IsActionActive(Capability action) => active.ContainsKey(action) || buffer.ContainsKey(action);

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

    // ============================================================================
    // QUERIES & ACCESSORS
    // ============================================================================

    public IActivationStrategy GetActivationStrategy(Weapon weapon) => activationStrategies[weapon.Activation];

    Weapon GetDefaultWeapon(Command command) => weaponSet?.DefaultWeapon(command.Action);

    bool HasWeaponSet() => weaponSet != null;
    public bool HasActiveWeapon() => weapon != null;
    bool HasActiveCommands() => active?.Count > 0;
    bool HasBufferCommands() => buffer?.Count > 0;

    void OnEvent<T>(T evt) where T : IEvent => EventBus<T>.Raise(evt);

    public void AssignHero(Hero hero)
    {
        context = hero.Context;
        if (!HasWeaponSet())
            weaponSet = context.weaponSet;
    }

    public void AssignWeaponSet(WeaponSet set) => weaponSet = set;

    public Context Context                                              => context;
    public WeaponInstance CurrentWeapon                                 => weapon;
    public WeaponCooldown Cooldown                                      => cooldown;
    public IReadOnlyDictionary<Capability, IReadOnlyList<string>> Locks => locks;

    public UpdatePriority Priority => ServiceUpdatePriority.WeaponLogic;


    void DebugLog()
    {
        Log.Debug(LogSystem.Weapon, LogCategory.State,          "Weapon.Active",        () => weapon?.Data.Name ?? "none" );
        Log.Trace(LogSystem.Weapon, LogCategory.State,          "Weapon.Phase",         () => weapon?.State.Phase.ToString() ?? "none" );
        Log.Trace(LogSystem.Weapon, LogCategory.Input,          "Commands.Active",      () => active?.Count > 0 ? string.Join(", ", active?.Keys) : "");
        Log.Trace(LogSystem.Weapon, LogCategory.Input,          "Commands.Buffered",    () => buffer?.Count > 0 ? string.Join(", ", buffer?.Keys) : "");
        Log.Trace(LogSystem.Weapon, LogCategory.Validation,     "Locks.NonCancelable",  () => NonCancelableAttackLocks );
        Log.Trace(LogSystem.Weapon, LogCategory.Validation,     "Locks.Active",         () => locks == null ? "<none>" : string.Join(", ", locks.Select(kvp => $"{kvp.Key}({kvp.Value.Count})")) ); 
        Log.Trace(LogSystem.Weapon, LogCategory.Validation,     "Cooldown",             () => cooldown.IsOnCooldown(weapon?.Data.Name) ?  $"Remaining: {cooldown.GetRemainingCooldown(weapon.Data.Name)}" : "Ready");
    }
}

// ============================================================================
// SUPPORTING TYPES
// ============================================================================

public readonly struct WeaponStatePayload
{
    public Weapon Weapon { get; init; }
    public WeaponPhase Phase { get; init; }
}

public readonly struct WeaponPublish : IEvent
{
    public Guid Id { get; }
    public Publish Action { get; }
    public WeaponStatePayload Payload { get; }

    public WeaponPublish(Guid id, Publish action, WeaponStatePayload payload)
    {
        Id = id;
        Action = action;
        Payload = payload;
    }
}

// ============================================================================
// COOLDOWN SYSTEM
// ============================================================================

public class WeaponCooldownInstance
{
    public string name;
    public Weapon weapon;
    public DualCountdown timer;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public WeaponCooldownInstance(Weapon instance)
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

    public void RegisterWeapon(Weapon weapon)
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

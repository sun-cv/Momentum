using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class WeaponState
{
    public HashSet<Guid>     CommandIDS     { get; set; } = new();
    public FrameCounter      PhaseFrames    { get; set; } = new();
    
    public DurationCountdown ControlWindow  { get; set; }
    public WeaponPhase       Phase          { get; set; } = WeaponPhase.Idle;

    public List<Capability> OwnedTriggers   { get; set; } = new();
    public HashSet<string>AvailableControls { get; set; } = new();
    public HashSet<Guid> CountedEffects     { get; set; } = new();

    public bool HasFired                    { get; set; }
    public bool IsLocked                    { get; set; }
    public bool ReadyToRelease              { get; set; }
    
    public void Reset()
    {
        PhaseFrames.Reset();
        ControlWindow = null;

        Phase           = WeaponPhase.Idle;
        HasFired        = false;
        IsLocked        = false;
        ReadyToRelease  = false;

        OwnedTriggers.Clear();
        CommandIDS.Clear();
        AvailableControls.Clear();
        CountedEffects.Clear();
    }

    public bool OwnsTrigger(Capability action)
    {
        return OwnedTriggers.Contains(action);
    }
}


public class WeaponSystem : RegisteredService, IServiceTick
{
    Context         context;
    WeaponSet       weaponSet;
    Weapon          weapon;
    WeaponState     state;
    WeaponCooldown  cooldown;

    IReadOnlyDictionary<Capability, Command> active;
    IReadOnlyDictionary<Capability, Command> buffer;
    IReadOnlyDictionary<Capability, IReadOnlyList<string>> locks;

    public int NonCancelableAttackLocks     { get; set; } = 0;

    public override void Initialize()
    {
        state       = new();
        cooldown    = new();

        EventBus<CommandPublish>.Subscribe(HandleCommandPublish);
        EventBus<EffectPublish> .Subscribe(HandleEffectNonCancelableLockCount);
        EventBus<LockPublish>   .Subscribe(HandleLockPublish);
    }

    public void Tick()
    {
        if (HasActiveWeapon())
            AdvanceWeaponState();

        ProcessWeaponActivation();
        
        if (state.ReadyToRelease && weapon != null)
        {
            ReleaseWeapon();
        }
        
        Logwin.Log("phase", state.Phase);
        Logwin.Log("Weapon", weapon?.Name);
        Logwin.Log("Lock count", NonCancelableAttackLocks);
    }
    
    // ============================================================================
    // PHASE ADVANCEMENT
    // ============================================================================
    
    void AdvanceWeaponState()
    {        
        if (ShouldValidateActions())
        {
            if (!HasAllRequiredActions(weapon))
            {
                state.ReadyToRelease = true;
                return;
            }
        }

        if (CheckActionReleaseTriggers())
            return;

        if (ShouldTerminate())
        {
            state.ReadyToRelease = true;
            return;
        }

        switch (state.Phase)
        {
            case WeaponPhase.Charging:
                AdvanceCharging();
                break;

            case WeaponPhase.Fire:
                AdvanceFire();
                break;

            case WeaponPhase.FireEnd:
                AdvanceFireEnd();
                break;
        }
    }
    
    bool ShouldValidateActions()
    {
        return weapon.Activation switch
        {
            WeaponActivation.WhileHeld => true,
            WeaponActivation.OnRelease => state.Phase == WeaponPhase.Charging,
            _ => false,
        };
    }

    bool ShouldTerminate()
    {
        return weapon.Termination switch
        {
            WeaponTermination.OnRelease => weapon.Action.Any(action => !IsActionHeld(action)),
            WeaponTermination.OnRootRelease => weapon.RequiredHeldActions.Any(action => !IsActionHeld(action)),
            _ => false,
        };
    }

    bool CheckActionReleaseTriggers()
    {
        if (weapon.Activation == WeaponActivation.OnRelease && state.Phase == WeaponPhase.Charging)
        {
            if (weapon.Action.Any(action => !IsActionHeld(action)))
            {
                if (GetChargePercent() >= weapon.MinimumChargeToFire)
                {
                    FireWeapon();
                    return true;
                }
            }
        }

        if (weapon.Activation == WeaponActivation.WhileHeld && state.Phase == WeaponPhase.Fire)
        {
            if (weapon.Action.Any(action => !IsActionHeld(action)))
            {
                TransitionToFireEnd();
                return true;
            }
        }

        return false;
    }

    void AdvanceCharging()
    {
        int chargeFrames    = GetChargeFrames(weapon);
        bool chargeComplete = state.PhaseFrames.CurrentFrame >= chargeFrames;

        switch (weapon.Activation)
        {
            case WeaponActivation.OnPress:
                if (chargeComplete)
                    FireWeapon();
                break;
            

            case WeaponActivation.OnCharge:
                if (chargeComplete)
                    FireWeapon();
                break;

            case WeaponActivation.OnRelease:
                if (chargeComplete && weapon.ForceMaxChargeRelease)
                    FireWeapon();
                break;

            case WeaponActivation.WhileHeld:
                if (chargeComplete)
                    FireWeapon();
                break;
        }
    }

    void AdvanceFire()
    {
        bool shouldEnd = false;

        // WhileHeld weapons stay in Fire until released (termination handles it)
        if (weapon.Activation != WeaponActivation.WhileHeld)
        {
            if (state.PhaseFrames.CurrentFrame >= GetFireDurationFrames(weapon))
                shouldEnd = true;
        }


        if (shouldEnd)
            TransitionToFireEnd();
    }

    void AdvanceFireEnd()
    {
        if (!FireEndComplete())
            return;

        // Debug.Log($"FireEnd complete for {weapon.Name}, marking ready to release");
        // Debug.Log($"Available controls: {string.Join(", ", state.AvailableControls)}");
        
        // Mark ready to release
        // ProcessWeaponActivation will check for swaps/transitions first
        state.ReadyToRelease = true;
    }

    bool FireEndComplete()
    {
        if (state.ControlWindow != null)
            return state.ControlWindow.IsFinished;
        
        return true;
    }

    // ============================================================================
    // PHASE TRANSITIONS
    // ============================================================================

    void FireWeapon()
    {
        // Debug.Log("Fire weapon");
        state.Phase = WeaponPhase.Fire;
        state.HasFired = true;

        state.PhaseFrames.Reset();
        state.PhaseFrames.Start();

        UpdateAvailableControls();
        SetWeaponPhase(WeaponPhase.Fire);
    }

    void TransitionToFireEnd()
    {
        Debug.Log("Transition to fire end");
        state.Phase = WeaponPhase.FireEnd;

        state.PhaseFrames.Reset();
        state.PhaseFrames.Start();

        if (weapon.ControlWindow > 0)
        {
            state.ControlWindow = new DurationCountdown(weapon.ControlWindow);
            state.ControlWindow.Start();
        }

        UpdateAvailableControls();
        SetWeaponPhase(WeaponPhase.FireEnd);
    }

    void ReleaseWeapon()
    {
        if (weapon != null && OnlyCancelableLocksRemain())
            CancelEffects();

        if (weapon.Cooldown > 0)
            cooldown.RegisterWeapon(weapon);

        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.Released, new() { Weapon = weapon, Phase = state.Phase }));

        weapon = null;
        state.Reset();

    }

    // ============================================================================
    // WEAPON ACTIVATION
    // ============================================================================

    void EquipWeapon(Weapon newWeapon, bool isChainedWeapon = false)
    {
        bool isOnHeldTransition = newWeapon.Availability == WeaponAvailability.OnHeld;
        
        // Debug.Log($"Activate weapon: {newWeapon.Name} (OnHeld: {isOnHeldTransition}, Chained: {isChainedWeapon})");
        
        // Release old weapon first
        if (weapon != null)
            ReleaseWeapon();
            
        weapon = newWeapon;

        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.Equipped, new() { Weapon = weapon, Phase = WeaponPhase.Idle }));

        EnableWeapon(isOnHeldTransition, isChainedWeapon);
    }

    void EnableWeapon(bool isOnHeldTransition = false, bool isChainedWeapon = false)
    {

        if (!isOnHeldTransition && !isChainedWeapon)
        {
            ConsumeNewCommands(weapon.Action);
            StoreAllCommandIDs(weapon.Action);

            if (weapon.LockTrigger)
                LockAllCommands(weapon.Action);
        }
        else
        {
            // For chains and OnHeld transitions, just track the command IDs
            StoreAllCommandIDs(weapon.Action);
        }


        PushEffects();

        if (weapon.LockTrigger)
            LockActionsToState();

        UpdateAvailableControls();

        state.Phase = WeaponPhase.Charging;
        state.PhaseFrames.Reset();
        state.PhaseFrames.Start();
        
        SetWeaponPhase(WeaponPhase.Charging);
    }

    // ============================================================================
    // CONTROL SYSTEM
    // ============================================================================

    void UpdateAvailableControls()
    {
        state.AvailableControls.Clear();

        if (weapon == null)
            return;

        switch (state.Phase)
        {
            case WeaponPhase.Charging:
                AddControls(weapon.AddControlOnCharge);
                RemoveControls(weapon.RemoveControlOnCharge);
                break;

            case WeaponPhase.Fire:
                AddControls(weapon.AddControlOnFire);
                RemoveControls(weapon.RemoveControlOnFire);
                break;

            case WeaponPhase.FireEnd:
                AddControls(weapon.AddControlOnFireEnd);
                RemoveControls(weapon.RemoveControlOnFireEnd);
                
                if (!string.IsNullOrEmpty(weapon.SwapOnFire))
                    state.AvailableControls.Add(weapon.SwapOnFire);
                break;
        }
    }

    void AddControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
        {
            // Debug.Log($"Adding control {control}");
            state.AvailableControls.Add(control);
        }
    }

    void RemoveControls(List<string> controls)
    {
        if (controls == null) return;
        foreach (var control in controls)
            state.AvailableControls.Remove(control);
    }

    // ============================================================================
    // WEAPON ACTIVATION LOGIC
    // ============================================================================

    void ProcessWeaponActivation()
    {
        if (HasActiveWeapon())
        {
            // Try available controls (includes OnHeld and SwapOnFire weapons)
            if (TryActivateFromAvailableControls())
            {
                state.ReadyToRelease = false; // Cancel pending release
                return;
            }

            // Try interrupt weapons
            if (TryActivateInterruptWeapon())
            {
                state.ReadyToRelease = false;
                return;
            }
        }
        else if (HasBufferCommands())
        {
            TryActivateDefaultWeapon();
        }
    }

    bool TryActivateFromAvailableControls()
    {
        if (state.AvailableControls.Count == 0)
            return false;

        // Debug.Log($"Checking {state.AvailableControls.Count} available controls");

        foreach (var weaponName in state.AvailableControls)
        {
            if (!weaponSet.weapons.TryGetValue(weaponName, out var availableWeapon))
            {
                // Debug.Log($"  {weaponName}: Not found in weaponSet");
                continue;
            }

            // Debug.Log($"  Checking {weaponName}:");

            bool isOnHeld = availableWeapon.Availability == WeaponAvailability.OnHeld;
            bool isChained = weaponName == weapon.SwapOnFire;
            
            // Check iActions
            if (!HasAllRequiredActions(availableWeapon))
            {
                // Debug.Log($"    Missing required inputs");
                continue;
            }

            // OnHeld weapons auto-activate when inputs are held
            // Other weapons need a new press
            if (!isOnHeld && !HasNewPressForWeapon(availableWeapon))
            {
                // Debug.Log($"    No new press (Availability: {availableWeapon.Availability})");
                continue;
            }

            if (!CanActivateWeapon(availableWeapon))
            {
                // Debug.Log($"    Cannot activate");
                continue;
            }
            
            // Debug.Log($"  ✓ Activating {weaponName} (OnHeld: {isOnHeld}, Chained: {isChained})");
            EquipWeapon(availableWeapon, isChained);
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
                continue;

            if (!CanInterruptCurrentWeapon(newWeapon))
                continue;

            if (!CanActivateWeapon(newWeapon, skipContextCheck: true))
                continue;

            Debug.Log("Interrupting weapon");

            EquipWeapon(newWeapon);
            return true;
        }

        return false;
    }

    bool TryActivateDefaultWeapon()
    {
        Debug.Log("Try active default");
        foreach (var command in buffer.Values)
        {
            var newWeapon = GetDefaultWeapon(command);

            if (newWeapon == null)
                continue;

            if (!CanActivateWeapon(newWeapon))
                continue;

            Debug.Log("Can Activate weapon");

            EquipWeapon(newWeapon);
            return true;
        }

        return false;
    }

    // ============================================================================
    // ACTIVATION VALIDATION
    // ============================================================================

    bool HasAllRequiredActions(Weapon weapon)
    {
        bool hasAll = weapon.Action.All(action => 
            active.ContainsKey(action) || buffer.ContainsKey(action)
        );
        
        if (!hasAll)
        {
            // Debug.Log($"      Missing inputs. Active: {string.Join(",", active.Keys)}, Buffer: {string.Join(",", buffer.Keys)}, Required: {string.Join(",", weapon.Input)}");
        }
        
        return hasAll;
    }

    bool HasNewPressForWeapon(Weapon weapon)
    {
        // OnHeld weapons auto-activate if input held
        if (weapon.Availability == WeaponAvailability.OnHeld)
        {
            // Debug.Log($"      OnHeld weapon - auto-activates");
            return true;
        }

        // Others need new press (at least one input must be new)
        bool hasNew = weapon.Action.Any(action => IsNewPress(action));
        // Debug.Log($"      Checking for new press: {hasNew}");
        if (hasNew)
        {
            foreach (var action in weapon.Action)
            {
                if (IsNewPress(action)) {}
                    // Debug.Log($"        New press on {input}");
            }
        }
        return hasNew;
    }

    bool CanInterruptCurrentWeapon(Weapon incomingWeapon)
    {
        if (weapon == null)
            return true;

        if (!OnlyCancelableLocksRemain())
            return false;

        if (weapon.Condition.Cancel != null && weapon.Condition.Cancel(context))
            return true;

        if (incomingWeapon.CanCancelDisables)
            return true;

        return false;
    }

bool CanActivateWeapon(Weapon weapon, bool skipContextCheck = false)
{
    // Cooldown check
    if (cooldown.IsOnCooldown(weapon.Name))
    {
        Debug.Log($"Weapon is on cooldown");
        return false;
    }

    // Custom condition check
    if (weapon.Condition.Activate != null)
    {
        if (!weapon.Condition.Activate(context))
        {
            Debug.Log($"Condition.Activate failed");
            return false;
        }
    }

    // Input lock check
    if (IsActionLocked(weapon))
    {
        Debug.Log($"Input is locked");
        return false;
    }

    // Non-cancelable lock check
    if (this.weapon != null && !OnlyCancelableLocksRemain())
    {
        Debug.Log($"Non-cancelable locks remain");
        return false;
    }

    // Context check (skipped for interrupt weapons)
    if (!skipContextCheck)
    {
        bool result = weapon.CanCancelDisables || context.CanAttack;
        Debug.Log($"Final check: CanCancelDisables={weapon.CanCancelDisables}, CanAttack={context.CanAttack}, Result={result}");
        return result;
    }

    return true;
}

    // ============================================================================
    // EFFECT MANAGEMENT
    // ============================================================================

    void SetWeaponPhase(WeaponPhase phase)
    {
        state.Phase = phase;
        OnEvent<WeaponPublish>(new(Guid.NewGuid(), Publish.PhaseChange, new() { Weapon = weapon, Phase = phase }));
        PushEffects();
    }

    void PushEffects()
    {
        foreach (var effect in weapon.Effects)
        {
            if (ShouldApplyEffect(effect))
                OnEvent<EffectRequest>(new(Guid.NewGuid(), EffectAction.Create, new() { Effect = effect }));
        }
    }

    bool ShouldApplyEffect(Effect effect)
    {
        if (effect is ITrigger trigger)
            return trigger.Trigger == state.Phase;

        return state.Phase == WeaponPhase.Idle;
    }

    void CancelEffects()
    {
        foreach (var effect in weapon.Effects)
        {
            if (effect.Cancelable)
                OnEvent<EffectRequest>(new(Guid.NewGuid(), EffectAction.Cancel, new() { Entity = weapon, Effect = effect }));
        }
    }

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    int GetChargeFrames(Weapon weapon)
    {
        if (weapon.ChargeTimeFrames > 0)
            return weapon.ChargeTimeFrames;

        if (weapon.ChargeTime > 0)
            return (int)(weapon.ChargeTime * 60);

        return 0;
    }

    float GetChargePercent()
    {
        int chargeFrames = GetChargeFrames(weapon);

        if (chargeFrames == 0)
            return 1.0f;

        return (float)state.PhaseFrames.CurrentFrame / chargeFrames;
    }

    bool IsNewPress(Capability action)
    {
        if (buffer.TryGetValue(action, out var cmd))
        {
            bool isNew = !state.CommandIDS.Contains(cmd.RuntimeID);
            // Debug.Log($"          IsNewPress({input}): RuntimeID={cmd.RuntimeID}, IsNew={isNew}, TrackedIDs={string.Join(",", state.CommandIDS)}");
            return isNew;
        }
        // Debug.Log($"          IsNewPress({input}): Not in buffer");
        return false;
    }    

    bool IsActionHeld(Capability action)
    {
        return active.ContainsKey(action) || buffer.ContainsKey(action);
    }

    bool OnlyCancelableLocksRemain()
    {
        return NonCancelableAttackLocks == 0;
    }

    bool IsActionLocked(Weapon weapon)
    {
        if (!weapon.AcceptTriggerLockRequests)
            return false;


        foreach (var action in weapon.Action)
        {
            if (locks != null && locks.TryGetValue(action, out var lockList) && lockList.Count > 0)
            {
                // Debug.Log($"Action Lock action {action}count {lockList.Count }");
                return true;
            }

        }
        return false;
    }

    int GetFireDurationFrames(Weapon weapon)
    {
        if (weapon.FireDurationFrames > 0)
            return weapon.FireDurationFrames;

        if (weapon.FireDuration > 0)
            return (int)(weapon.FireDuration * 60);

        return 0;
    }

    // ============================================================================
    // COMMAND MANAGEMENT - FIXED for hybrid active/buffer inputs
    // ============================================================================

    void ConsumeNewCommands(List<Capability> actions)
    {
        foreach (var action in actions)
        {
            // Only consume if in buffer (new press)
            if (buffer.TryGetValue(action, out var cmd))
                ConsumeCommand(cmd);
        }
    }

    void ConsumeCommand(Command command) 
        => OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Consume, new() { Command = command }));

    void LockAllCommands(List<Capability> actions)
    {
        foreach (var action in actions)
        {
            // Lock from active or buffer, whichever has it
            if (active.TryGetValue(action, out var cmd))
                LockCommand(cmd);
            else if (buffer.TryGetValue(action, out var cmd2))
                LockCommand(cmd2);
        }
    }

    void LockCommand(Command command) 
        => OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Lock, new() { Command = command }));

    void UnlockCommand(Command command) 
        => OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Unlock, new() { Command = command }));

    void StoreAllCommandIDs(List<Capability> actions)
    {
        foreach (var action in actions)
        {
            // Store from active first, then buffer
            if (active.TryGetValue(action, out var cmd))
                state.CommandIDS.Add(cmd.RuntimeID);
            else if (buffer.TryGetValue(action, out var cmd2))
                state.CommandIDS.Add(cmd2.RuntimeID);
        }
    }

    void LockActionsToState() => weapon.Action.ForEach(action => state.OwnedTriggers.Add(action));

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

        bool IsDefinitive = !effect.Cancelable;

        // Debug.Log($"Event publish {evt.Action}, Is definitive {IsDefinitive}");

        switch (evt.Action)
        {
            case Publish.Activated:
                if (IsDefinitive && disable.DisableAttack) NonCancelableAttackLocks++;
                break;

            case Publish.Canceled:
            case Publish.Deactivated:
                if (IsDefinitive && disable.DisableAttack) NonCancelableAttackLocks--;
                break;
        }    

    }

    void HandleLockPublish(LockPublish evt)
    {
        Debug.Log("Updating locks");
        locks = evt.Payload.Locks;
    }

    // ============================================================================
    // QUERIES
    // ============================================================================

    Weapon GetDefaultWeapon(Command command) => weaponSet?.DefaultWeapon(command.Action);
    void SetContextWeaponSet() => weaponSet = context.weaponSet;

    bool HasWeaponSet()      => weaponSet != null;
    bool HasActiveWeapon()   => weapon != null;
    bool HasActiveCommands() => active?.Count > 0;
    bool HasBufferCommands() => buffer?.Count > 0;

    void OnEvent<T>(T evt) where T : IEvent             => EventBus<T>.Raise(evt);
    public void AssignHero(Hero hero)   { context = hero.Context; if (!HasWeaponSet()) SetContextWeaponSet(); }
    public void AssignWeaponSet(WeaponSet set)          => weaponSet = set;

    public UpdatePriority Priority  => ServiceUpdatePriority.WeaponLogic;
}

public readonly struct WeaponStatePayload
{
    public Weapon Weapon       { get; init; }
    public WeaponPhase Phase   { get; init; }
}

public readonly struct WeaponPublish : IEvent 
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



public class WeaponCooldownInstance
{
    public string name;
    public Weapon weapon;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public DualCountdown timer;

    public WeaponCooldownInstance(Weapon instance)
    {
        name   = instance.Name;
        weapon = instance;
        timer  = new(instance.Cooldown);
    }

    public void Initialize()
    {
        timer.OnTimerStart  += OnApply;
        timer.OnTimerStop   += OnClear;

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

        // instance.OnApply   += () => EventBus<EffectPublish>.Raise(new(Guid.NewGuid(), Publish.Activated,   new() { Instance = instance}));
        // instance.OnClear   += () => EventBus<EffectPublish>.Raise(new(Guid.NewGuid(), Publish.Deactivated, new() { Instance = instance}));
        // instance.OnCancel  += () => EventBus<EffectPublish>.Raise(new(Guid.NewGuid(), Publish.Canceled,    new() { Instance = instance}));
        instance.OnClear   += () => cooldowns.Remove(instance);
        instance.OnCancel  += () => cooldowns.Remove(instance);

        cooldowns.Add(instance);
        instance.Initialize();
    }

    public bool IsOnCooldown(string weaponName)          => cooldowns.Any(instance => instance.name == weaponName);
    public float GetRemainingCooldown(string weaponName) => cooldowns.FirstOrDefault(instance => instance.name == weaponName)?.timer.CurrentTime ?? 0f;
}


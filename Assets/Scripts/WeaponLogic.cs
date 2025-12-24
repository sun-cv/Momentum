using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WeaponState
{
    public HashSet<Guid>     CommandIDS     { get; set; } = new();
    public FrameCounter      PhaseFrames    { get; set; } = new();
    
    public DurationCountdown ControlWindow  { get; set; }
    public WeaponPhase       Phase          { get; set; } = WeaponPhase.Idle;

    public int NonCancelableAttackLocks     { get; set; } = 0;

    public List<InputIntent> OwnedTriggers  { get; set; } = new();
    public HashSet<string>AvailableControls { get; set; } = new();

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

        NonCancelableAttackLocks = 0;

        OwnedTriggers.Clear();
        CommandIDS.Clear();
        AvailableControls.Clear();
    }

    public bool OwnsTrigger(InputIntent input)
    {
        return OwnedTriggers.Contains(input);
    }
}


public class WeaponSystem : RegisteredService, IServiceTick
{
    Context     context;
    WeaponSet   weaponSet;
    Weapon      weapon;
    WeaponState state;

    IReadOnlyDictionary<InputIntent, Command> active;
    IReadOnlyDictionary<InputIntent, Command> buffer;
    IReadOnlyDictionary<InputIntent, IReadOnlyList<string>> locks;

    public override void Initialize()
    {
        state = new();

        EventBus<CommandPublish>.Subscribe(HandleCommandPublish);
        EventBus<EffectPublish> .Subscribe(HandleEffectNonCancelableLockCount);
        EventBus<LockPublish>   .Subscribe(HandleLockPublish);

        Services.RegisterTick(this);
    }

    public void Tick()
    {
        // Advance active weapon timing
        if (HasActiveWeapon())
            AdvanceWeaponState();
        
        // Try to activate weapons (checks available controls)
        ProcessWeaponActivation();
        
        // Release weapon if ready and nothing new activated
        if (state.ReadyToRelease && weapon != null)
        {
            ReleaseWeapon();
        }
        
        Logwin.Log("phase", state.Phase);
        Logwin.Log("Weapon", weapon?.Name);
    }
    
    // ============================================================================
    // PHASE ADVANCEMENT
    // ============================================================================
    
    void AdvanceWeaponState()
    {        
        if (ShouldValidateInputs())
        {
            if (!HasAllRequiredInputs(weapon))
            {
                // Debug.Log($"{weapon.Name} inputs no longer satisfied, releasing");
                state.ReadyToRelease = true;
                return;
            }
        }

        if (ShouldTerminate())
        {
            HandleTermination();
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
    
    bool ShouldValidateInputs()
    {
        switch (weapon.Activation)
        {
            case WeaponActivation.WhileHeld:
                return true;
                
            case WeaponActivation.OnRelease:
                // Only during charging - once fired, we don't care
                return state.Phase == WeaponPhase.Charging;
                
            case WeaponActivation.OnPress:
            case WeaponActivation.OnCharge:
            default:
                return false;
        }
    }

    bool ShouldTerminate()
    {
        switch (weapon.Termination)
        {
            case WeaponTermination.OnRelease:
                return weapon.Input.Any(input => !IsInputHeld(input));
                
            case WeaponTermination.OnRootRelease:
                return weapon.RequiredHeldInputs.Any(input => !IsInputHeld(input));
                
            case WeaponTermination.AfterFire:
            case WeaponTermination.Manual:
            default:
                return false;
        }
    }

    void HandleTermination()
    {
        switch (weapon.Activation)
        {
            case WeaponActivation.OnRelease:
                if (state.Phase == WeaponPhase.Charging)
                {
                    bool minChargeMet = GetChargePercent() >= weapon.MinimumChargeToFire;
                    if (minChargeMet)
                    {
                        FireWeapon();
                        return; // Don't terminate yet
                    }
                }
                break;
                
            case WeaponActivation.WhileHeld:
                // End Fire phase when released
                if (state.Phase == WeaponPhase.Fire)
                {
                    TransitionToFireEnd();
                    return;
                }
                break;
        }
        
        state.ReadyToRelease = true;
    }

    void AdvanceCharging()
    {
        int chargeFrames    = GetChargeFrames(weapon);
        bool chargeComplete = state.PhaseFrames.CurrentFrame >= chargeFrames;
        bool minChargeMet   = GetChargePercent() >= weapon.MinimumChargeToFire;

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
                // Check for early release (handled by termination)
                // Check for force release at max charge
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

        weapon = null;
        state.Reset();
    }

    // ============================================================================
    // WEAPON ACTIVATION
    // ============================================================================

    void ActivateWeapon(Weapon newWeapon, bool isChainedWeapon = false)
    {
        bool isOnHeldTransition = newWeapon.Availability == WeaponAvailability.OnHeld;
        
        // Debug.Log($"Activate weapon: {newWeapon.Name} (OnHeld: {isOnHeldTransition}, Chained: {isChainedWeapon})");
        
        // Release old weapon first
        if (weapon != null)
            ReleaseWeapon();
            
        weapon = newWeapon;
        EnableWeapon(isOnHeldTransition, isChainedWeapon);
    }

    void EnableWeapon(bool isOnHeldTransition = false, bool isChainedWeapon = false)
    {
        // Consume commands only for fresh activations
        // Don't consume for chains (command already consumed) or OnHeld transitions
        if (!isOnHeldTransition && !isChainedWeapon)
        {
            ConsumeNewCommands(weapon.Input);
            StoreAllCommandIDs(weapon.Input);

            if (weapon.LockTrigger)
                LockAllCommands(weapon.Input);
        }
        else
        {
            // For chains and OnHeld transitions, just track the command IDs
            StoreAllCommandIDs(weapon.Input);
        }


        PushEffects();

        if (weapon.LockTrigger)
            LockTriggersToState();

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
            
            // Check inputs
            if (!HasAllRequiredInputs(availableWeapon))
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
            ActivateWeapon(availableWeapon, isChained);
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

            ActivateWeapon(newWeapon);
            return true;
        }

        return false;
    }

    bool TryActivateDefaultWeapon()
    {
        foreach (var command in buffer.Values)
        {
            var newWeapon = GetDefaultWeapon(command);

            if (newWeapon == null)
                continue;

            if (!CanActivateWeapon(newWeapon))
                continue;

            ActivateWeapon(newWeapon);
            return true;
        }

        return false;
    }

    // ============================================================================
    // ACTIVATION VALIDATION
    // ============================================================================

    bool HasAllRequiredInputs(Weapon weapon)
    {
        bool hasAll = weapon.Input.All(input => 
            active.ContainsKey(input) || buffer.ContainsKey(input)
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
        bool hasNew = weapon.Input.Any(input => IsNewPress(input));
        // Debug.Log($"      Checking for new press: {hasNew}");
        if (hasNew)
        {
            foreach (var input in weapon.Input)
            {
                if (IsNewPress(input)) {}
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

    bool CanActivateWeapon(Weapon weapon)
    {
        if (weapon.Condition.Activate != null)
        {
            if (!weapon.Condition.Activate(context))
            {
                // Debug.Log($"      Condition.Activate failed (game state requirement not met)");
                return false;
            }
        }

        if (IsInputLocked(weapon))
        {
            // Debug.Log($"      Input is locked");
            return false;
        }

        if (this.weapon != null && !OnlyCancelableLocksRemain())
        {
            // Debug.Log($"      Non-cancelable locks remain");
            return false;
        }

        bool result = weapon.CanCancelDisables || context.CanAttack;
        // Debug.Log($"      Final check: CanCancelDisables={weapon.CanCancelDisables}, CanAttack={context.CanAttack}, Result={result}");
        return result;
    }

    // ============================================================================
    // EFFECT MANAGEMENT
    // ============================================================================

    void SetWeaponPhase(WeaponPhase phase)
    {
        state.Phase = phase;
        OnEvent<PublishWeaponTrigger>(new(weapon, phase));
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

    bool IsNewPress(InputIntent input)
    {
        if (buffer.TryGetValue(input, out var cmd))
        {
            bool isNew = !state.CommandIDS.Contains(cmd.RuntimeID);
            // Debug.Log($"          IsNewPress({input}): RuntimeID={cmd.RuntimeID}, IsNew={isNew}, TrackedIDs={string.Join(",", state.CommandIDS)}");
            return isNew;
        }
        // Debug.Log($"          IsNewPress({input}): Not in buffer");
        return false;
    }    

    bool IsInputHeld(InputIntent input)
    {
        return active.ContainsKey(input) || buffer.ContainsKey(input);
    }

    bool OnlyCancelableLocksRemain()
    {
        return state.NonCancelableAttackLocks == 0;
    }

    bool IsInputLocked(Weapon weapon)
    {
        if (!weapon.AcceptTriggerLockRequests)
            return false;

        foreach (var input in weapon.Input)
        {
            if (locks != null && locks.TryGetValue(input, out var lockList) && lockList.Count > 0)
                return true;
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

    void ConsumeNewCommands(List<InputIntent> inputs)
    {
        foreach (var input in inputs)
        {
            // Only consume if in buffer (new press)
            if (buffer.TryGetValue(input, out var cmd))
                ConsumeCommand(cmd);
        }
    }

    void ConsumeCommand(Command command) 
        => OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Consume, new() { Command = command }));

    void LockAllCommands(List<InputIntent> inputs)
    {
        foreach (var input in inputs)
        {
            // Lock from active or buffer, whichever has it
            if (active.TryGetValue(input, out var cmd))
                LockCommand(cmd);
            else if (buffer.TryGetValue(input, out var cmd2))
                LockCommand(cmd2);
        }
    }

    void LockCommand(Command command) 
        => OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Lock, new() { Command = command }));

    void UnlockCommand(Command command) 
        => OnEvent<CommandRequest>(new(Guid.NewGuid(), CommandAction.Unlock, new() { Command = command }));

    void StoreAllCommandIDs(List<InputIntent> inputs)
    {
        foreach (var input in inputs)
        {
            // Store from active first, then buffer
            if (active.TryGetValue(input, out var cmd))
                state.CommandIDS.Add(cmd.RuntimeID);
            else if (buffer.TryGetValue(input, out var cmd2))
                state.CommandIDS.Add(cmd2.RuntimeID);
        }
    }

    void LockTriggersToState() => weapon.Input.ForEach(input => state.OwnedTriggers.Add(input));

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

        switch (evt.Action)
        {
            case Publish.Activated:
                if (IsDefinitive) state.NonCancelableAttackLocks++;
                break;

            case Publish.Deactivated:
            case Publish.Canceled:
                if (IsDefinitive) state.NonCancelableAttackLocks--;
                break;
        }    
    }

    void HandleLockPublish(LockPublish evt)
    {
        locks = evt.Payload.Locks;
    }

    // ============================================================================
    // QUERIES
    // ============================================================================

    Weapon GetDefaultWeapon(Command command) => weaponSet?.DefaultWeapon(command.Input);



    bool HasActiveWeapon()   => weapon != null;
    bool HasActiveCommands() => active?.Count > 0;
    bool HasBufferCommands() => buffer?.Count > 0;

    void OnEvent<T>(T evt) where T : IEvent => EventBus<T>.Raise(evt);
    public void AssignContext(Context context) => this.context = context;
    public void AssignWeaponSet(WeaponSet set) => this.weaponSet = set;

    public UpdatePriority Priority  => ServiceUpdatePriority.WeaponLogic;
}

public readonly struct PublishWeaponTrigger : IEvent 
{ 
    public Weapon Weapon { get; } 
    public WeaponPhase Phase { get; } 
    
    public PublishWeaponTrigger(Weapon weapon, WeaponPhase phase) 
    { 
        Weapon = weapon; 
        Phase = phase; 
    }
}
using System;
using System.Collections.Generic;
using System.Linq;





public enum WeaponPhase
{
    Idle,
    Charging,
    Fire,
    FireEnd,
}

/// <summary>What causes this weapon to fire?</summary>
public enum WeaponActivation
{
    OnPress,          // Fires immediately on activation
    OnCharge,         // Fires when charge completes
    OnRelease,        // Fires when trigger input released (after min charge)
    WhileHeld,        // Continuously active while held
}

/// <summary>What causes this weapon to terminate?</summary>
public enum WeaponTermination
{
    AfterFire,        // Ends after fire phase completes
    OnRelease,        // Ends when any trigger input released
    OnRootRelease,    // Ends when RequiredHeldInputs released
    Manual,           // Only ends when explicitly interrupted
}

/// <summary>How does this weapon become available/activatable?</summary>
public enum WeaponAvailability
{
    Default,          // Available as default weapon for an input
    OnPhase,          // Available during parent weapon phase (via AddControlOnX lists)
    OnHeld,           // Auto-activates when parent active + input held
}


public class DamagingWeapon     : WeaponAction { }
public class MovementWeapon     : WeaponAction { }

public class WeaponAction       : Definition
{
    // ============================================================================
    // IDENTITY
    // ============================================================================
    
    /// <summary>The capabilities required to activate this weapon</summary>
    public List<Capability> Trigger             { get; init; } = new();
    /// <summary>What causes this weapon to fire</summary>
    public WeaponActivation Activation          { get; init; } = WeaponActivation.OnPress;
    /// <summary>What causes this weapon to terminate </summary>
    public WeaponTermination Termination        { get; init; } = WeaponTermination.AfterFire;
    /// <summary>How this weapon becomes available for activation</summary>
    public WeaponAvailability Availability      { get; init; } = WeaponAvailability.OnPhase;
    /// <summary>Default weapon mapping for this action (used for Availability.Default)</summary>
    public Capability DefaultWeapon             { get; init; } = Capability.None;

    // ============================================================================
    // CONTROL SYSTEM - What weapons become available
    // ============================================================================
    
    /// <summary>Weapons that become available during Charging phase</summary>
    public List<string> AddControlOnCharge      { get; init; } = new();
    /// <summary>Weapons that become available during Fire phase</summary>
    public List<string> AddControlOnFire        { get; init; } = new();
    /// <summary>Weapons that become available during FireEnd phase</summary>
    public List<string> AddControlOnFireEnd     { get; init; } = new();
    /// <summary>Weapons that are removed from availability during Charging phase</summary>
    public List<string> RemoveControlOnCharge   { get; init; } = new();
    /// <summary>Weapons that are removed from availability during Fire phase</summary>
    public List<string> RemoveControlOnFire     { get; init; } = new();
    /// <summary>Weapons that are removed from availability during FireEnd phase</summary>
    public List<string> RemoveControlOnFireEnd  { get; init; } = new();
    /// <summary>Automatic weapon chain - available during FireEnd/ControlWindow</summary>
    public string SwapOnFire                    { get; init; } = "";

    public bool ForceReleaseOnSwap              { get; init; } = false;
    // ============================================================================
    // CHAIN ANCHORING
    // ============================================================================
    
    /// <summary> Root inputs that anchor this weapon chain. If any of these are released, the weapon terminates</summary>
    public List<Capability> RequiredHeldTriggers { get; init; } = new();

    // ============================================================================
    // INTERRUPTION & CANCELING
    // ============================================================================
    
    /// <summary>Can this weapon interrupt other weapons?</summary>
    public bool CanInterrupt                    { get; init; } = false;
    /// <summary>Can this weapon cancel through disable effects?</summary>
    public bool CanCancelDisables               { get; init; } = false;

    // ============================================================================
    // TRIGGER LOCKS
    // ============================================================================
    
    /// <summary>Should this weapon lock its trigger inputs while active?</summary>
    public bool LockTriggerAction               { get; init; } = false;
    /// <summary>Should this weapon respect trigger lock requests from effects?</summary>
    public bool AcceptTriggerLockRequests       { get; init; } = false;

    // ============================================================================
    // TIMING
    // ============================================================================
    
    /// <summary>Control window duration (in seconds) for combo continuation</summary>
    public float ControlWindow                  { get; init; } = 0.0f;
    /// <summary>Cooldown after weapon completes</summary>
    public float Cooldown                       { get; init; } = 0.0f;
    /// <summary>Charge time in seconds</summary>
    public float ChargeTime                     { get; init; } = 0.0f;
    /// <summary>Charge time in frames (takes priority over ChargeTime if set)</summary>
    public int ChargeTimeFrames                 { get; init; } = 0;
    /// <summary>Fire phase duration in Seconds</summary>
    public float FireDuration                   { get; init; } = 0;
    /// <summary>Fire phase duration in frames (takes priority over ChargeTime if set)</summary>
    public int FireDurationFrames               { get; init; } = 0;

    // ============================================================================
    // CHARGE BEHAVIOR
    // ============================================================================
    
    /// <summary>OnRelease triggers: minimum charge percentage required to fire (0.0 to 1.0)</summary>
    public float MinimumChargeToFire            { get; init; } = 0;
    /// <summary>For OnRelease triggers: force fire when max charge is reached </summary>
    public bool ForceMaxChargeRelease           { get; init; } = false;

    /// ============================================================================
    /// PLAYER MODIFIERS
    /// ============================================================================

    public bool WeaponOverridesMovement         { get; init; } = false;
    /// <summary>Lock players current movement direction in all phases</summary>
    public bool LockDirection                   { get; init; } = false;
    /// <summary>Lock players current movement direction when charging</summary>
    public bool LockDirectionOnCharge           { get; init; } = false;
    /// <summary>Cancel movement in all phases</summary>
    public bool CancelMovement                  { get; init; } = false;
    /// <summary>Cancel movement when charging</summary>
    public bool ChargeCancelMovement            { get; init; } = false;
    /// <summary>Set velocity</summary>
    public int Velocity                         { get; init; } = -1;
    /// <summary>Set velocity when charging</summary>
    public int ChargeVelocity                   { get; init; } = -1;
    /// <summary>Set Modifier %/summary>
    public float Modifier                       { get; init; } = -1;

    /// ============================================================================
    /// WEAPON CONFIGURATION
    /// ============================================================================

    /// <summary>Number of times weapon can be fired</summary>
    public int ClipSize                         { get; init; } = 0;
    /// <summary>Duration in seconds regen of clip</summary>
    public float ClipRegenInterval              { get; init; } = 0;
    /// <summary>Regen full clip size?</summary>
    public bool FullClipRegen                   { get; init; } = false;
    /// <summary>Custom predicates - intended for player context</summary>
    public WeaponTriggerCondition Condition     { get; init; } = new();
    /// <summary>List of effects applied by weapon</summary>
    public List<Effect> Effects                 { get; init; } = new();
}

public class WeaponTriggerCondition
{
    public Func<Entity, bool> Activate         { get; init; }
    public Func<Entity, bool> Cancel           { get; init; }
}

public class WeaponDefinition : Definition
{
    public Dictionary<string, WeaponAction> actions;
    public Dictionary<string, WeaponAction> Actions => actions;
}


// ============================================================================
// WEAPON INSTANCE & STATE
// ============================================================================
public class WeaponInstance : Instance
{
    public WeaponAction Action  { get; init; }
    public WeaponState  State   { get; init; }
    public WeaponInstance(WeaponAction action)
    {
        Action  = action;
        State   = new();
    }

    public int GetChargeFrames()
    {
        if (Action.ChargeTimeFrames > 0)
            return Action.ChargeTimeFrames;
        if (Action.ChargeTime > 0)
            return (int)(Action.ChargeTime * 60);
        return 0;
    }

    public int GetFireDurationFrames()
    {
        if (Action.FireDurationFrames > 0)
            return Action.FireDurationFrames;
        if (Action.FireDuration > 0)
            return (int)(Action.FireDuration * 60);
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
    public bool ShouldValidateActivationTriggers()
    {
        return Action.Activation switch
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


public class WeaponLoadout
{
    private Dictionary<string, (WeaponAction action, Entity source)> actions = new();
    
    public void AddAction(WeaponAction action, Entity source)
    {
        actions[action.Name] = (action, source);
    }
    
    public void RemoveAction(string actionName)
    {
        actions.Remove(actionName);
    }
    
    public WeaponAction GetAction(string name)
    {
        return actions.TryGetValue(name, out var tuple) ? tuple.action : null;
    }
    
    public Entity GetSource(string name)
    {
        return actions.TryGetValue(name, out var tuple) ? tuple.source : null;
    }
    
    public bool TryGetAction(string name, out WeaponAction action)
    {
        action = null;
        if (!actions.TryGetValue(name, out var tuple))
            return false;

        action = tuple.action;
        return true;
    }

    public WeaponAction DefaultWeapon(Capability capability)
        => actions.Values
            .Select(t => t.action)
            .FirstOrDefault(a => 
                a.Trigger.SequenceEqual(new List<Capability>() { capability }) && 
                a.Availability == WeaponAvailability.Default);
}
using System;
using System.Collections.Generic;



public class Ability    : Definition
{
    public AbilityLifecycle Lifecycle               { get; init; } = new();
    public AbilityPermissions Permission            { get; init; } = new();
    public AbilityTiming Timing                     { get; init; } = new();
    public AbilityControls Control                  { get; init; } = new();
    public AbilityDamage Damage                     { get; init; } = new();
    public AbilityMovement Movement                 { get; init; } = new(); 
    public AbilityFacing Facing                     { get; init; } = new();
    public AbilityAnimation Animation               { get; init; } = new();

    public List<HitboxDefinition> Hitboxes          { get; init; } = new();
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AbilityLifecycle
{
    public List<Trigger> Triggers                   { get; init; } = new();
    public List<Trigger> SustainTriggers            { get; init; } = new();

    public bool HoldActivatingCommand               { get; init; } = true;
    public bool ActivatesFromHeldCommand            { get; init; }

    public AbilityActivation Activation             { get; init; }
}   

public class AbilityPermissions
{
    public AbilityTag Tag                           { get; init; }
    public Dictionary<AbilityPhase, AbilityPermissionsEntry> Phase 
                                                    { get; init; } = new();
}

public class AbilityPermissionsEntry
{
    public List<AbilityTag> CoexistWith             { get; init; } = new();
    public List<AbilityTag> CancelableBy            { get; init; } = new();
}

public class AbilityTiming
{
    public Dictionary<AbilityPhase, AbilityTimingEntry> Phase 
                                                    { get; init; } = new();
    public Dictionary<string, int> Cooldown         { get; init; } = new();
    public int ControlWindow                        { get; init; }
}

public class AbilityTimingEntry
{
    public int Frames                               { get; init; }
    public int Minimum                              { get; init; }
    public int CancelFrameOffset                    { get; init; }
}
        
public class AbilityControls
{
    public List<string> AddControlOnCharge          { get; init; } = new();
    public List<string> AddControlOnFire            { get; init; } = new();
    public List<string> AddControlOnFireEnd         { get; init; } = new();

    public List<string> RemoveControlOnCharge       { get; init; } = new();
    public List<string> RemoveControlOnFire         { get; init; } = new();
    public List<string> RemoveControlOnFireEnd      { get; init; } = new();

    public List<string> BlockedControl              { get; init; } = new(); 

    public bool ForceRelease                        { get; init; }
    public bool ForceReleaseOnSwap                  { get; init; }
}

public class AbilityDamage
{
    public List<DamageComponent> Components         { get; init; } = new();
    public List<ForceComponent> Forces              { get; init; } = new();
}

public class AbilityMovement           
{   
    public List<MovementDefinition> Actions         { get; init; } = new();
}

public class AbilityAnimation           
{       
    public List<AbilityAnimationEntry> Entries             { get; init; }
}

public class AbilityAnimationEntry
{
    public string Animation                         { get; init; }                          

    public AbilityPhase EnterPhase                  { get; init; }
    public AbilityPhase ClearPhase                  { get; init; }

    public bool RequireManualRelease                { get; init; }

    public bool LockAimDuringPlayback               { get; init; }
    public bool LockDirectionDuringPlayback         { get; init; }
}           
// REWORK REQUIRED - Does this actually require a list? 


public class AbilityFacing
{
    public DirectionMode DirectionMode              { get; init; }
    public DirectionSource DirectionSource          { get; init; }
    public DirectionConstraint DirectionConstraint  { get; init; }
    public AbilityPhase EnterPhase                  { get; init; }
    public AbilityPhase ClearPhase                  { get; init; }
}



public class AbilitySet : Definition
{
    public Dictionary<string, Ability> Abilities    { get; init; } = new();
    public Dictionary<Trigger, string> Bindings     { get; init; } = new();
}

public class AbilityInstance : Instance
{
    public Ability Ability                          { get; init; }
    public IntentSnapshot Intent                    { get; init; }

        // -----------------------------------

    public AbilityPhase Phase                       { get; set; } = AbilityPhase.Enable;
    public FrameWatch FrameCount                    { get; set; } = new();

        // -----------------------------------

    public HashSet<AnimationAPI> Animations         { get; set; } = new();
    public HashSet<Guid> Commands                   { get; set; } = new();
    public HashSet<HitboxAPI> Hitboxes              { get; set; } = new();
    public HashSet<string> ComboAbilities           { get; set; } = new();
    public FrameTimer ComboControlWindow            { get; set; }
    
        // -----------------------------------

    public bool Cancelable                          { get; set; }
    public bool Canceled                            { get; set; }

    // ===============================================================================

    public AbilityInstance(Ability ability)
    {
        Ability = ability;
    }
}

public class AbilityLoadout
{
    private readonly Dictionary<string, Ability> abilities  = new();
    private readonly Dictionary<Trigger, string> bindings   = new();
        

    public void AddSet(AbilitySet set)
    {
        foreach(var (name, ability) in set.Abilities)
        {
            abilities[name]   = ability;
        }

        foreach(var (Trigger, name) in set.Bindings)
        {
            Bindings[Trigger] = name;
        }
    }

    public void RemoveSet(AbilitySet set)
    {
        foreach(var (name, _) in set.Abilities)
        {
            abilities.Remove(name);
        }

        foreach(var (name, _) in set.Bindings)
        {
            Bindings.Remove(name);
        }
    }

    public Dictionary<string, Ability> Abilities   => abilities;
    public Dictionary<Trigger, string> Bindings    => bindings;

}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum AbilityPhase
{
    None,
    Enable,
    Charging,
    Fire,
    FireEnd,
    Disable,
    Release,
}

public enum AbilityTag
{
    Action,
    Instant,
    Movement
}

public enum AbilityActivation
{
    OnPress,          // Fires immediately on activation
    OnCharge,         // Fires when charge completes
    OnRelease,        // Fires when trigger input released (after min charge)
    WhileHeld,        // Continuously active while held
}




using System.Collections.Generic;







public class Effect     : Instance
{
    public string Name                          { get; init; }
    public bool Active                          { get; init; }
    public bool Cancelable                      { get; init; }
}

public enum EffectType
{
    Speed,
    Grip
}


public interface ITrigger                   { public WeaponPhase Trigger            { get; init; }}
public interface IType                      { public EffectType Type                { get; init; }}
public interface IDuration                  { public float Duration                 { get; init; }}
public interface IDurationFrames            { public int DurationFrames             { get; init; }}
public interface IDisableMove               { public bool DisableMove               { get; init; }}
public interface IDisableAttack             { public bool DisableAttack             { get; init; }}
public interface IDisableRotate             { public bool DisableRotate             { get; init; }}

public interface IActionLock                { public bool RequestActionLock         { get; init; } 
                                              public List<Capability> ActionLocks   { get; init; }}


public interface IModifiable                { public float Modifier                 { get; init; } 
                                              public float ModifierTarget           { get; init; } 
                                              public float ModifierSpeed            { get; init; }}

public interface INoUnitCollision           { public bool NoUnitCollision           { get; init; }}
public interface IImmuneToForce             { public bool ImmuneToForce             { get; init; }}
public interface ICanAffectInvulnerable     { public bool CanAffectInvulnerable     { get; init; }}


public interface IDisableRules : IDisableAttack, IDisableMove, IDisableRotate {}
public interface ICollisionRules : INoUnitCollision, IImmuneToForce {}


public interface IEffectCallback
{
    public string OnApplyFunctionName           { get; init; }
    public string OnClearFunctionName           { get; init; }
}



public class SwordSwingDisable : Effect, IDurationFrames, IDisableAttack, IDisableRotate, IDisableMove, IActionLock
{
    public int DurationFrames                   { get; init; }

    public bool DisableAttack                   { get; init; }
    public bool DisableRotate                   { get; init; }
    public bool DisableMove                     { get; init; }
    
    public bool RequestActionLock               { get; init; }
    public List<Capability> ActionLocks         { get; init; }
}



public class ShieldParryActivation : Effect, IDurationFrames
{
    public int DurationFrames                   { get; init; }
}


public class ShieldBlockActivation : Effect, IDuration
{
    public float Duration                       { get; init; }
}


public class ShieldBraceDisable : Effect, ITrigger, IDuration, IDisableAttack, IDisableRotate
{
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.Idle;
    public float Duration                       { get; init; }
    
    public bool DisableAttack                   { get; init; }
    public bool DisableRotate                   { get; init; }
}


public class ShieldBraceAim : Effect, ITrigger, IDurationFrames
{
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.Idle;
    public int DurationFrames                   { get; init; }
}

public class SwordMobility : Effect, IType, ITrigger, IDurationFrames, IModifiable
{
    public EffectType Type                      { get; init; }
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.Idle;
    public int DurationFrames                   { get; init; }

    public float Modifier                       { get; init; }
    public float ModifierTarget                 { get; init; } = 0;
    public float ModifierSpeed                  { get; init; } = 0;
}

public class ShieldMobility : Effect, IType, ITrigger, IDurationFrames, IModifiable
{
    public EffectType Type                      { get; init; }
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.Idle;
    public int DurationFrames                   { get; init; }

    public float Modifier                       { get; init; }
    public float ModifierTarget                 { get; init; } = 0;
    public float ModifierSpeed                  { get; init; } = 0;
}


public class DashDisable : Effect, IDurationFrames, IDisableRotate, IDisableMove, IActionLock
{
    public int DurationFrames                   { get; init; }

    public bool DisableRotate                   { get; init; }
    public bool DisableMove                     { get; init; }
    
    public bool RequestActionLock               { get; init; }
    public List<Capability> ActionLocks         { get; init; }

}

public class DashMobility : Effect, IType, ITrigger, IDurationFrames, IModifiable
{
    public EffectType Type                      { get; init; }
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.Idle;
    public int DurationFrames                   { get; init; }

    public float Modifier                       { get; init; }
    public float ModifierTarget                 { get; init; } = 0;
    public float ModifierSpeed                  { get; init; } = 0;
}
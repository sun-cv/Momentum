using System.Collections.Generic;







public class Effect     : Instance
{
    public string Name                          { get; init; }
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
public interface IInterval                  { public float Interval                 { get; init; }}
public interface IDisableMove               { public bool DisableMove               { get; init; }}
public interface IDisableAttack             { public bool DisableAttack             { get; init; }}
public interface IDisableRotate             { public bool DisableRotate             { get; init; }}
public interface ICancelable                { public bool Cancelable                { get; init; }}
public interface ICancelableOnRelease       { public bool CancelOnRelease           { get; init; }}

public interface IActionLock                { public bool RequestActionLock         { get; init; } 
                                              public List<Capability> ActionLocks   { get; init; }}


public interface IModifyFloat               { public float Modifier                 { get; init; }}
public interface IModifyTarget              { public float ModifyTarget             { get; init; }}
public interface IModifySpeed               { public float ModifySpeed              { get; init; }}
public interface IModifyDuration            { public float ModifyDuration           { get; init; }}
public interface IModifyDurationFrames      { public float ModifyDurationFrames     { get; init; }}
public interface IModifyTimespan            { public float ModifyTimespan           { get; init; }}

public interface IModifiable : IModifyFloat, IModifyTarget, IModifyTimespan, IModifyDurationFrames {}


public interface INoUnitCollision           { public bool NoUnitCollision           { get; init; }}
public interface IImmuneToForce             { public bool ImmuneToForce             { get; init; }}
public interface ICanAffectInvulnerable     { public bool CanAffectInvulnerable     { get; init; }}

public interface IImmuneToDamage            { public bool ImmuneToDamage            { get; init; }}
public interface IInvulnerable              { public bool Invulnerable              { get; init; }}
public interface IStunned                   { public bool Stunned                   { get; init; }}



public interface IDamage                    { public float Damage                    { get; init; }}
public interface ISlow : IDuration, IModifySpeed {}                     

public interface IDot : IDamage, IDuration, IInterval {}




public interface IDisableRules : IDisableAttack, IDisableMove, IDisableRotate {}
public interface ICollisionRules : INoUnitCollision, IImmuneToForce {}



public interface IEffectCallback
{
    public string OnApplyFunctionName           { get; init; }
    public string OnClearFunctionName           { get; init; }
}



// Example status effect 
public class BurningEffect : Effect, IDot
{
    public float Duration                   { get; init; }
    public float Interval                   { get; init; }
    public float Damage                     { get; init; }
}




public class SwordSwingDisable : Effect, ICancelable, IDurationFrames, IDisableAttack, IDisableRotate, IDisableMove, IActionLock
{
    public bool Cancelable                      { get; init; }

    public int DurationFrames                   { get; init; }

    public bool DisableAttack                   { get; init; }
    public bool DisableRotate                   { get; init; }
    public bool DisableMove                     { get; init; }
    
    public bool RequestActionLock               { get; init; }
    public List<Capability> ActionLocks         { get; init; }
}



public class ShieldParryWindow : Effect, IDurationFrames
{
    public int DurationFrames                   { get; init; }
}


public class ShieldBlockWindow : Effect, ICancelable, IDuration, ICancelableOnRelease
{
    public bool Cancelable                      { get; init; }
    public float Duration                       { get; init; }
    public bool CancelOnRelease                 { get; init; }
}


public class ShieldBraceDisable : Effect, ICancelable, ITrigger, IDuration, IDurationFrames, IDisableAttack, ICancelableOnRelease, IDisableRotate
{
    public bool Cancelable                      { get; init; }

    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.None;
    public float Duration                       { get; init; }
    public int DurationFrames                   { get; init; }

    public bool CancelOnRelease                 { get; init; }
    public bool DisableAttack                   { get; init; }
    public bool DisableRotate                   { get; init; }
}


public class ShieldBraceAim : Effect, ICancelable, ITrigger, IDurationFrames
{
    public bool Cancelable                      { get; init; }

    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.None;
    public int DurationFrames                   { get; init; }
}

public class SwordMobility : Effect, IType, ITrigger, IDurationFrames, IModifiable
{
    public EffectType Type                      { get; init; }
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.None;
    public int DurationFrames                   { get; init; }

    public float Modifier                       { get; init; }
    public float ModifyTarget                   { get; init; } = 0;
    public float ModifyTimespan                 { get; init; } = -1;
    public float ModifyDurationFrames           { get; init; } = -1;
}

public class ShieldMobility : Effect, ICancelable, IType, ITrigger, IDurationFrames, ICancelableOnRelease, IModifiable
{
    public bool Cancelable                      { get; init; }
    public bool CancelOnRelease                 { get; init; }

    public EffectType Type                      { get; init; }
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.None;
    public int DurationFrames                   { get; init; }

    public float Modifier                       { get; init; }
    public float ModifyTarget                   { get; init; } = 0;
    public float ModifyTimespan                 { get; init; } = -1;
    public float ModifyDurationFrames           { get; init; } = -1;
}


public class DashDisable : Effect, ICancelable, IDurationFrames, IDisableRules, IActionLock
{
    public bool Cancelable                      { get; init; }

    public int DurationFrames                   { get; init; }

    public bool DisableAttack                   { get; init; }
    public bool DisableRotate                   { get; init; }
    public bool DisableMove                     { get; init; }
    
    public bool RequestActionLock               { get; init; }
    public List<Capability> ActionLocks         { get; init; }

}

public class WeaponMobility : Effect, ICancelable, IType, ITrigger, IDurationFrames, IModifiable
{
    public bool Cancelable                      { get; init; }
    
    public EffectType Type                      { get; init; }
    public WeaponPhase Trigger                  { get; init; } = WeaponPhase.None;
    public int DurationFrames                   { get; init; }

    public float Modifier                       { get; init; }
    public float ModifyTarget                   { get; init; } = 0;
    public float ModifyTimespan                 { get; init; } = -1;
    public float ModifyDurationFrames           { get; init; } = -1;
}
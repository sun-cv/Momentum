using System;
using System.Collections.Generic;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class DamageRule
{
    public float    Multiplier      { get; init; } = 1f;
    public bool     Unblockable     { get; init; }
    public bool     Piercing        { get; init; }
    public bool     AbsorbOnBreak   { get; init; }

    public static readonly DamageRule Default = new();
}

public struct RuleApplicationEntry
{
    public Func<DamageCalculationContext, bool> Condition;
    public Action<DamageCalculationContext>     OnTrue;
    public Action<DamageCalculationContext>     OnFalse;
}

public class ShieldDamageRules
{
    public readonly Dictionary<(DamageMode, DamageElement), DamageRule> Rules   = new()
    {
        [(DamageMode.DoT,    DamageElement.Fire)]  = new() { Unblockable = true },
        [(DamageMode.DoT,    DamageElement.Shock)] = new() { Multiplier  = 1.5f },
        [(DamageMode.Laser,  DamageElement.Shock)] = new() { Multiplier  = 1.5f },
        [(DamageMode.Direct, DamageElement.Shock)] = new() { Multiplier  = 1.5f },
    };
 
    public readonly List<RuleApplicationEntry> Gates                            = new()
    {
        new() { Condition   = (context) => { return context.Target is IMortal actor && actor.Invulnerable; }},
        new() { Condition   = (context) => { return context.Rule.Unblockable; }},
    };

    public readonly List<RuleApplicationEntry> PreProcess                       = new()
    {
    };

    public readonly List<RuleApplicationEntry> PostProcess                      = new()
    {
        new()
        {
            Condition   = (context) => { return context.Rule.Piercing; },
            OnTrue      = (context) => { },
            OnFalse     = (context) => { context.Result.Damage -= context.Result.Shield / context.Rule.Multiplier; },
        },
        new()
        {
            Condition   = (context) => { return context.Rule.AbsorbOnBreak; },
            OnTrue      = (context) => { context.Result.Shield = context.Result.Shield + context.Result.Damage; context.Result.Damage = 0; },
            OnFalse     = (context) => { },
        }
    };

    public DamageRule Get(DamageMode mode, DamageElement element) { DamageRule rule = null; Rules?.TryGetValue((mode, element), out rule); return rule ?? DamageRule.Default;}
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ArmorDamageRules
{

    readonly Dictionary<(DamageMode, DamageElement), DamageRule> Rules          = new()
    {
        [(DamageMode.DoT, DamageElement.Fire)] = new() { Piercing = true },
    };
 
    public readonly List<RuleApplicationEntry> Gates            = new()
    {
        new() { Condition   = (context) => { return context.Target is IMortal actor && actor.Invulnerable; }},
        new() { Condition   = (context) => { return context.Rule.Unblockable; }},
    };

    public readonly List<RuleApplicationEntry> PreProcess                       = new()
    {
    };

    public readonly List<RuleApplicationEntry> PostProcess                      = new()
    {
        new()
        {
            Condition   = (context) => { return context.Rule.Piercing; },
            OnTrue      = (context) => { },
            OnFalse     = (context) => { context.Result.Damage -= context.Result.Shield / context.Rule.Multiplier; },
        },
        new()
        {
            Condition   = (context) => { return context.Rule.AbsorbOnBreak; },
            OnTrue      = (context) => { context.Result.Shield = context.Result.Shield + context.Result.Damage; context.Result.Damage = 0; },
            OnFalse     = (context) => { },
        }
    };

    public DamageRule Get(DamageMode mode, DamageElement element) { DamageRule rule = null; Rules?.TryGetValue((mode, element), out rule); return rule ?? DamageRule.Default;}
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class HealthDamageRules
{
    readonly Dictionary<(DamageMode, DamageElement), DamageRule> Rules          = new()
    {
        [(DamageMode.DoT, DamageElement.Fire)] = new() { Piercing = true },
    };

    public readonly List<RuleApplicationEntry> Gates            = new()
    {
        new() { Condition   = (context) => { return context.Target is IMortal actor && actor.Invulnerable; }},
        new() { Condition   = (context) => { return context.Rule.Unblockable; }},
    };

    public readonly List<RuleApplicationEntry> PreProcess                       = new()
    {
    };

    public readonly List<RuleApplicationEntry> PostProcess                      = new()
    {
        new()
        {
            Condition   = (context) => { return context.Rule.Piercing; },
            OnTrue      = (context) => { },
            OnFalse     = (context) => { context.Result.Damage -= context.Result.Shield / context.Rule.Multiplier; },
        },
        new()
        {
            Condition   = (context) => { return context.Rule.AbsorbOnBreak; },
            OnTrue      = (context) => { context.Result.Shield = context.Result.Shield + context.Result.Damage; context.Result.Damage = 0; },
            OnFalse     = (context) => { },
        }
    };

    public DamageRule Get(DamageMode mode, DamageElement element) { DamageRule rule = null; Rules?.TryGetValue((mode, element), out rule); return rule ?? DamageRule.Default;}
}



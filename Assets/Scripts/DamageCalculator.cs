using System.Collections.Generic;
using UnityEngine;



public class DamageCalculator : RegisteredService, IServiceLoop
{    
    readonly List<DamageContext> queue = new();

    // -----------------------------------


    // ===============================================================================

    public DamageCalculator()
    {
        Services.Lane.Register(this);
    }

    // ===============================================================================


    public void Loop()
    {
        ProcessQueue();
    }

    void ProcessQueue()
    {
        foreach (var context in queue)
        {
            ProcessContext(context);
        }
    }

    void ProcessContext(DamageContext context)
    {

    }


    // need to handle all 3 resources 
    // if shield, if armor, health


    
    void CalculateDamage(DamageContext context)
    {
        
    }




    // ===============================================================================
    //  Events
    // ===============================================================================


    // ===============================================================================

    readonly Logger Log = new(LogSystem.Combat, LogLevel.Debug);

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.DamageCalculator
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct Damage : IResourceAction
{
    public float Amount                     { get; init; }
    public DamageType Type                  { get; init; }

    public Damage(float amount, DamageType type)
    {
        Amount  = amount;
        Type    = type;
    }
}

public readonly struct DamageConfig
{
    public bool BypassArmor                 { get; init; } // Skips calc
    public bool BypassShield                { get; init; } // Skips calc
    public bool Unblockable                 { get; init; } // By equipped shield 
}

public readonly struct DamagePackageConfig
{
    public ParryConfig Parry                { get; init; }
}

public readonly struct ParryConfig
{
    public bool Enabled                     { get; init; }
    public float ParryWindow                { get; init; }
    public float PerfectParryWindow         { get; init; }

    public float ParryReward                { get; init; }
    public float PerfectParryReward         { get; init; }

    public float StaggerDuration            { get; init; }
}

public readonly struct BlockConfig
{
    public bool Enabled                     { get; init; }
}

public readonly struct DamageComponent
{
    public Damage Damage                    { get; init; }
    public DamageConfig Config              { get; init; }
    public List<Effect> Effects             { get; init; }

    public DamageComponent(Damage damage, DamageConfig config = default)
    {
        Damage      = damage;
        Config      = config;
        Effects     = new();
    }
}

public readonly struct DamagePackage
{
    public List<DamageComponent> Components { get; init; }
    public DamagePackageConfig Config       { get; init; }

    public DamagePackage(List<DamageComponent> components, DamagePackageConfig config = default)
    {
        Components = components;
        Config     = config;
    }
}

public class DamageContext
{
    public Actor Target                     { get; init; }
    public Actor Source                     { get; init; }
    public DamagePackage Package            { get; init; }
    public DamageResult Result              { get; init; }

    public DamageContext(Actor target, Actor source, DamagePackage package)
    {
        Target      = target;
        Source      = source;
        Package     = package;
        Result      = new();
    }
 }

public class DamageResult
{
    public float TotalDamage                { get; set; }
    public float RemainingDamage            { get; set; }
    public float ShieldDamage               { get; set; }
    public float ArmorDamage                { get; set; }
    public float HealthDamage               { get; set; }
    public bool ShieldBroken                { get; set; }
    public bool Parried                     { get; set; }
    public bool Blocked                     { get; set; }
    public bool Killed                      { get; set; }
    public List<Effect> AppliedEffects      { get; set; } = new();
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum DamageType
{
    Fire,       // Burns armor and health on dot, break effect DOT
    Frost,      // slows, break effect: longer recharge on shield.
    Shock,      // Increased damage to shield, break effect stun
    Physical,   // regular damage
    Explosion,  // break effect: knocback
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct DamageEvent : IMessage
{
    public DamageContext Context            { get; init; }

    public DamageEvent(DamageContext context)
    {
        Context = context; 
    }
}

public readonly struct CalculateDamage : IMessage
{
    public DamageContext Context            { get; init; }

    public CalculateDamage(DamageContext context)
    {
        Context = context; 
    }
    
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Processor
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public interface IDamageCalculator
{
    void Calculate(DamageContext context);
}


public class ShieldCalculator : IDamageCalculator
{

    public void Calculate(DamageContext context)
    {

        switch(HasEquippedShield(context.Target))
        {
            case true:  HandleActiveShield(context);  break;
            case false: HandlePassiveShield(context); break;
        }
    } 
    
    void HandleActiveShield(DamageContext context)
    {
        if (!EquippedShieldBlockedDamage(context))
            return;

        if (!HasShieldRemaining(context.Target))
            return;

        CalculateShieldDamage(context);
    }

    void HandlePassiveShield(DamageContext context)
    {
        if (!HasShieldRemaining(context.Target))
            return;

        CalculateShieldDamage(context);
    }

    void CalculateShieldDamage(DamageContext context)
    {
        foreach (var component in context.Package.Components)
        {
            CalculateDamage(context, component);
        }
    }


    void CalculateDamage(DamageContext context, DamageComponent component)
    {
        var target                  = context.Target as IShield;
        var result                  = context.Result;
        var rules                   = DamageRules.Get(component.Damage.Type).Shield;

        var shieldHealth            = target.Shield;
        var incomingDamage          = context.Result.RemainingDamage;

        var damageMultiplier        = rules.Multiplier;
        var totalDamage             = incomingDamage * damageMultiplier;

        float absorbed              = Mathf.Min(totalDamage, shieldHealth);

        result.ShieldDamage        += absorbed;

        switch(rules.CannotAbsorb)
        {
            case true:  result.RemainingDamage = totalDamage;               break;
            case false: result.RemainingDamage = totalDamage - absorbed;    break;
        }

        result.ShieldBroken         = shieldHealth <= absorbed;
    }


   // ===============================================================================
   //   Predicates
   // ===============================================================================

    bool HasShieldRemaining(Actor target)
    {
        return target is IShield actor && actor.Shield > 0;
    }

    bool EquippedShieldBlockedDamage(DamageContext context)
    {
        return context.Result.Blocked;
    }

    bool HasEquippedShield(Actor target)
    {
        return target is IShielded actor && actor.ShieldEquipped;
    }
}


public class Armorcalculator : IDamageCalculator
{

    public void Calculate(DamageContext context)
    {

        switch(HasEquippedShield(context.Target))
        {
            case true:  HandleActiveShield(context);  break;
            case false: HandlePassiveShield(context); break;
        }
    } 
    
    void HandleActiveShield(DamageContext context)
    {
        if (!EquippedShieldBlockedDamage(context))
            return;

        if (!HasShieldRemaining(context.Target))
            return;

        CalculateShieldDamage(context);
    }

    void HandlePassiveShield(DamageContext context)
    {
        if (!HasShieldRemaining(context.Target))
            return;

        CalculateShieldDamage(context);
    }

    void CalculateShieldDamage(DamageContext context)
    {
        foreach (var component in context.Package.Components)
        {
            CalculateDamage(context, component);
        }
    }


    void CalculateDamage(DamageContext context, DamageComponent component)
    {
        var target                  = context.Target as IShield;
        var result                  = context.Result;
        var rules                   = DamageRules.Get(component.Damage.Type).Shield;

        var shieldHealth            = target.Shield;
        var incomingDamage          = context.Result.RemainingDamage;

        var damageMultiplier        = rules.Multiplier;
        var totalDamage             = incomingDamage * damageMultiplier;

        float absorbed              = Mathf.Min(totalDamage, shieldHealth);

        result.ShieldDamage        += absorbed;

        switch(rules.CannotAbsorb)
        {
            case true:  result.RemainingDamage = totalDamage;               break;
            case false: result.RemainingDamage = totalDamage - absorbed;    break;
        }

        result.ShieldBroken         = shieldHealth <= absorbed;
    }


   // ===============================================================================
   //   Predicates
   // ===============================================================================

    bool HasShieldRemaining(Actor target)
    {
        return target is IShield actor && actor.Shield > 0;
    }

    bool EquippedShieldBlockedDamage(DamageContext context)
    {
        return context.Result.Blocked;
    }

    bool HasEquippedShield(Actor target)
    {
        return target is IShielded actor && actor.ShieldEquipped;
    }
}

public class HealthCalculator : IDamageCalculator
{

    public void Calculate(DamageContext context)
    {
        
    } 
    
}

public interface IMitigationProcessor
{
    float Process(DamageContext context);
}


public class ArmorMitigation : IMitigationProcessor
{
    public float Process(DamageContext context)
    {
        return 0f;
    }
}

public class ResistanceMitigation : IMitigationProcessor
{
    public float Process(DamageContext context)
    {
        return 0f;
    }
}

public class KillingBlow {}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                          Maps
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class DamageRule
{
    public ShieldRule Shield                { get; init; }
    public ArmorRule  Armor                 { get; init; }
    public HealthRule Health                { get; init; }

    public static readonly DamageRule Default = new();
}

public readonly struct ShieldRule
{
    public bool Bypass                      { get; init; }
    public float Multiplier                 { get; init; }
    public bool CannotAbsorb                { get; init; }
}

public readonly struct ArmorRule
{
    public bool Bypass                      { get; init; }
    public float Multiplier                 { get; init; }
    public bool CannotAbsorb                { get; init; }
}

public readonly struct HealthRule
{
    public float Multiplier                 { get; init; }
    public bool CannotAbsorb                { get; init; }
}

public static class DamageRules
{
    static readonly Dictionary<DamageType, DamageRule> rules = new()
    {
        [DamageType.Fire]       = new()
        {
            Shield  = new(){ Multiplier                  = 2f    },
            Armor   = new(){ CannotAbsorb                = true  },
            Health  = new(){ CannotAbsorb                = true  }
        },
        [DamageType.Shock]      = new()
        {
            Shield  = new(){ Multiplier                  = 1.5f  },
            Armor   = new(){  },
            Health  = new(){  }
        },
        [DamageType.Frost]      = new()
        {
            Shield  = new(){ Multiplier                  = .75f  },
            Armor   = new(){ Multiplier                  = .75f  },
            Health  = new(){ Multiplier                  = 1f    }
        },
        [DamageType.Physical]   = new()
        {
            Shield  = new(){ Multiplier                  = 1f    },
            Armor   = new(){ Multiplier                  = 1f    },
            Health  = new(){ Multiplier                  = 1f    },
        },
        [DamageType.Explosion]  = new()
        {
            Shield  = new(){ Multiplier                  = 1f    },
            Armor   = new(){ Multiplier                  = 1f    },
            Health  = new(){ Multiplier                  = 1f    },
        }
    };

    public static DamageRule Get(DamageType type) => rules.TryGetValue(type, out var rule) ? rule : new DamageRule();
}
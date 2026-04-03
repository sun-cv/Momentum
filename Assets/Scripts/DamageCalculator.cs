    
using System;
using System.Collections.Generic;
using UnityEngine;



public class DamageCalculator : RegisteredService, IServiceLoop
{    
    readonly List<DamageContext> queue                                  = new();
    readonly List<(Func<Actor, bool>, IDamageCalculator)> calculators   = new();

    // ===============================================================================

    public DamageCalculator()
    {
        RegisterDamageCalculators();

        Link.Global<CalculateDamage>(HandleDamageCalculatorEvent);
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessQueue();
    }

    // ===============================================================================

    void ProcessQueue()
    {
        foreach (var context in queue)
        {
            ProcessContext    (context);
            SendResolvedDamage(context);
        }

        queue.Clear();
    }

    void ProcessContext(DamageContext context)
    {
        foreach(var (predicate, calculator) in calculators)
        {
            if (!predicate(context.Target))
                continue;

            calculator.Calculate(context);
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleDamageCalculatorEvent(CalculateDamage message)
    {
        queue.Add(message.Context);
    }

    void SendResolvedDamage(DamageContext context)
    {
        Emit.Global(new ResolveDamage(context));
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    void RegisterDamageCalculators()
    {
        Register((actor) => actor is IShield, new ShieldCalculator());
        Register((actor) => actor is IArmor,  new ArmorCalculator());
        Register((actor) => actor is IHealth, new HealthCalculator());
    }

    void Register(Func<Actor, bool> func, IDamageCalculator calculator)
    {
        calculators.Add((func, calculator)); 
    }

    // ===============================================================================

    // readonly Logger Log = Logging.For(LogSystem.Combat);

    public UpdatePriority Priority => ServiceUpdatePriority.DamageCalculator;
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct CalculateDamage : IMessage
{
    public DamageContext Context            { get; init; }

    public CalculateDamage(DamageContext context)
    {
        Context = context; 
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IDamageCalculator
{
    void Calculate(DamageContext context);
}
 

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Structs                                                   
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public struct DamageCalculationContext
{
    public Actor Target                 { get; set; }
    public DamageRule Rule              { get; set; }
    public DamageComponent Component    { get; set; }
    public ComponentResult Result       { get; set; }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Processors
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Shield
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class ShieldCalculator : IDamageCalculator
{

    // ===============================================================================

    public void Calculate(DamageContext context)
    {
        ProcessDamageComponents(context);
    }

    // ===============================================================================

    void ProcessDamageComponents(DamageContext context)
    {
        foreach(var component in context.Package.Components)
        {
            ProcessComponent(context, component);
        }
    }

    void ProcessComponent(DamageContext context, DamageComponent component)
    {
        var calculationContext = CalculationContext(context, component);
        
        if (DamageGateProcessor(calculationContext))
            return;

        DamageRuleProcessor(calculationContext, Rules.Damage.Shield.PreProcess );
        DamageCalculation  (calculationContext);
        DamageRuleProcessor(calculationContext, Rules.Damage.Shield.PostProcess);
    }

    void DamageCalculation(DamageCalculationContext context)
    {
        var result                      = context.Result;
        var rule                        = context.Rule;

        var shield                      = Target(context).Shield;
        var damage                      = result.Damage;   

        var multiplier                  = rule.Multiplier;
        var totalDamage                 = MathF.Round(damage * multiplier);

        float absorbed                  = Mathf.Min(totalDamage, shield);

        result.Shield                  += absorbed;
        result.BrokeShield              = shield <= absorbed;  
    }

    bool DamageGateProcessor(DamageCalculationContext context)
    {
        foreach (var gate in Rules.Damage.Shield.Gates)
        {
            if (gate.Condition(context))
                return true;
        }
        return false;
    } 

    void DamageRuleProcessor(DamageCalculationContext context, List<DamageRule.Entry> rules)
    {
        foreach (var rule in rules)
        {
            switch(rule.Condition(context))
            {
                case true:  rule.OnTrue (context);  break;
                case false: rule.OnFalse(context);  break;
            }
        }
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    DamageCalculationContext CalculationContext(DamageContext context, DamageComponent component)
    {
        return new()
        {
            Rule        = Rules.Damage.Shield.Get(component.Mode, component.Damage.Element),
            Target      = context.Target,
            Component   = component,
            Result      = context.Package.Result.Components[component],
        };
    }

    IShield Target(DamageCalculationContext context)
    {
        return context.Target as IShield;
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Armor
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        
public class ArmorCalculator : IDamageCalculator
{

    // ===============================================================================

    public void Calculate(DamageContext context)
    {
        ProcessDamageComponents(context);
    }

    // ===============================================================================

    void ProcessDamageComponents(DamageContext context)
    {
        foreach(var component in context.Package.Components)
        {
            ProcessComponent(context, component);
        }
    }

    void ProcessComponent(DamageContext context, DamageComponent component)
    {
        var calculationContext = CalculationContext(context, component);
        
        if (DamageGateProcessor(calculationContext))
            return;

        DamageRuleProcessor(calculationContext, Rules.Damage.Armor.PreProcess );
        DamageCalculation  (calculationContext);
        DamageRuleProcessor(calculationContext, Rules.Damage.Armor.PostProcess);
    }

    void DamageCalculation(DamageCalculationContext context)
    {
        var result                      = context.Result;
        var rule                        = context.Rule;

        var armor                       = Target(context).Armor;
        var damage                      = result.Damage;   

        var multiplier                  = rule.Multiplier;
        var totalDamage                 = MathF.Round(damage * multiplier);

        float absorbed                  = Mathf.Min(totalDamage, armor);

        result.Armor                   += absorbed;
        result.BrokeArmor               = armor <= absorbed;
    }

    bool DamageGateProcessor(DamageCalculationContext context)
    {
        foreach (var gate in Rules.Damage.Armor.Gates)
        {
            if (gate.Condition(context))
                return true;
        }
        return false;
    } 

    void DamageRuleProcessor(DamageCalculationContext context, List<DamageRule.Entry> rules)
    {
        foreach (var rule in rules)
        {
            switch(rule.Condition(context))
            {
                case true:  rule.OnTrue (context);  break;
                case false: rule.OnFalse(context);  break;
            }
        }
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    DamageCalculationContext CalculationContext(DamageContext context, DamageComponent component)
    {
        return new()
        {
            Rule        = Rules.Damage.Armor.Get(component.Mode, component.Damage.Element),
            Target      = context.Target,
            Component   = component,
            Result      = context.Package.Result.Components[component],
        };
    }

    IArmor Target(DamageCalculationContext context)
    {
        return context.Target as IArmor;
    }
}
    

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Health
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    
public class HealthCalculator : IDamageCalculator
{

    // ===============================================================================

    public void Calculate(DamageContext context)
    {
        ProcessDamageComponents(context);
    }

    // ===============================================================================

    void ProcessDamageComponents(DamageContext context)
    {
        foreach(var component in context.Package.Components)
        {
            ProcessComponent(context, component);
        }
    }

    void ProcessComponent(DamageContext context, DamageComponent component)
    {
        var calculationContext = CalculationContext(context, component);
        
        if (DamageGateProcessor(calculationContext))
            return;

        DamageRuleProcessor(calculationContext, Rules.Damage.Health.PreProcess );
        DamageCalculation  (calculationContext);
        DamageRuleProcessor(calculationContext, Rules.Damage.Health.PostProcess);
    }

    void DamageCalculation(DamageCalculationContext context)
    {
        var result                      = context.Result;
        var rule                        = context.Rule;

        var health                      = Target(context).Health;
        var damage                      = result.Damage;   

        var multiplier                  = rule.Multiplier;
        var totalDamage                 = MathF.Round(damage * multiplier);

        float absorbed                  = Mathf.Min(totalDamage, health);

        result.Health                  += absorbed;
        result.BrokeHealth              = health <= absorbed;
    }

    bool DamageGateProcessor(DamageCalculationContext context)
    {
        foreach (var gate in Rules.Damage.Health.Gates)
        {
            if (gate.Condition(context))
                return true;
        }
        return false;
    } 

    void DamageRuleProcessor(DamageCalculationContext context, List<DamageRule.Entry> rules)
    {
        foreach (var rule in rules)
        {
            switch(rule.Condition(context))
            {
                case true:  rule.OnTrue (context);  break;
                case false: rule.OnFalse(context);  break;
            }
        }
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    DamageCalculationContext CalculationContext(DamageContext context, DamageComponent component)
    {
        return new()
        {
            Rule        = Rules.Damage.Health.Get(component.Mode, component.Damage.Element),
            Target      = context.Target,
            Component   = component,
            Result      = context.Package.Result.Components[component],
        };
    }

    IMortal Target(DamageCalculationContext context)
    {
        return context.Target as IMortal;
    }
}



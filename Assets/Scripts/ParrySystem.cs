using System.Collections.Generic;



public class ParrySystem : RegisteredService, IServiceLoop
{
    
    readonly List<DamageContext> queue = new();

    // ===============================================================================

    public ParrySystem()
    {
        Link.Global<DamageEvent>(HandleParryEvent);
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessQueue();
    }

    // ===============================================================================

    void ProcessQueue()
    {
        foreach (var instance in queue)
        {
            ProcessParry(instance);
        }
    
        queue.Clear();
    }

    void ProcessParry(DamageContext context)
    {
        switch(IsParrying(context.Target))
        {
            case true:  ResolveSuccessfulParry(context);    break;
            case false: ResolveFailedParry(context);        break;
        }
    }

    void ResolveSuccessfulParry(DamageContext context)
    {
        switch(IsPerfectParry(context))
        {
            case false: Parry(context);         break;
            case true:  PerfectParry(context);  break;
        }

        SetDamageToZero(context);
        SetParriedContext(context);
    }

        // REWORK REQUIRED once status effect handler is built out.

    void PerfectParry(DamageContext context)
    {
        // ApplyStagger(context.Target, context.Source, context.Package.Config.Parry.StaggerDuration);
        RechargeEnergy(context.Target, context.Package.Config.Parry.PerfectParryReward);
    }

    void Parry(DamageContext context)
    {
        // Noop currently - optional to add effect on parry. 
    }

    void ResolveFailedParry(DamageContext context)
    {
        SendProcessDamage(context);
    }

    void SetParriedContext(DamageContext context)
    {
        context.Package.Result.Parried = true;
    }

    void SetDamageToZero(DamageContext context)
    {
        foreach ( var (_, result) in context.Package.Result.Components)
        {
            result.Damage = 0;
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleParryEvent(DamageEvent message)
    {
        queue.Add(message.Context);
    }

    void SendProcessDamage(DamageContext context)
    {
        Emit.Global(new CalculateDamage(context));
    }

    void RechargeEnergy(Actor target, float amount)
    {
        target.Bus.Emit.Local(new Recharge(amount));
    }


    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool IsPerfectParry(DamageContext context)
    {
        return ((IParryable)context.Target).Parrying.Frame <= context.Package.Config.Parry.PerfectParryWindow;
    }

    bool IsParrying(Actor target)
    {
        return target is IParryable instance && instance.Parrying;
    }


    // ===============================================================================

    public UpdatePriority Priority => ServiceUpdatePriority.ParrySystem;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct ParryEvent : IMessage
{
    public DamageContext Context            { get; init; }

    public ParryEvent(DamageContext context)
    {
        Context = context;
    }
}

// Data declared Parry presentation for animation? REWORK REQUIRED

// public class AttackPresentation
// {
//     public bool     ShowParryWindow     { get; init; }
//     public float    ParryWindowStart    { get; init; }
//     public float    ParryWindowEnd      { get; init; }
// }

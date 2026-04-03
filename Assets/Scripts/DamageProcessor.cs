using System.Collections.Generic;



public class DamageProcessor : RegisteredService, IServiceLoop
{
    
    readonly List<DamageContext> queue = new();

    // ===============================================================================

    public DamageProcessor()
    {
        Link.Global<DamageEvent>(HandleDamageEvent);
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
            ProcessContext(context);
        }
    
        queue.Clear();
    }

    void ProcessContext(DamageContext context)
    {
        if (IsParryable(context))   { SendParryRequest   (context);  return; }
                                      SendCalculateDamage(context);
    }


    // ===============================================================================
    //  Events
    // ===============================================================================

    void SendParryRequest(DamageContext context)
    {
        Emit.Global(new ParryEvent(context));
    }

    void SendCalculateDamage(DamageContext context)
    {
        Emit.Global(new CalculateDamage(context));
    }

    void HandleDamageEvent(DamageEvent message)
    {
        queue.Add(message.Context);
    }


    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool IsParryable(DamageContext context)
    {
        return context.Package.Config.Parry.Enabled && context.Target is IParryable actor && actor.Parrying;
    }

    // ===============================================================================


    public UpdatePriority Priority => ServiceUpdatePriority.DamageProcessor;
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

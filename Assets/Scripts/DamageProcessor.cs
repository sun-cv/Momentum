using System.Collections.Generic;



public class DamageProcessor : RegisteredService, IServiceLoop
{
    
    readonly List<DamageContext> queue = new();

        // -----------------------------------


    // ===============================================================================

    public DamageProcessor()
    {
        Link.Global<DamageEvent>(HandleDamageEvent);
        Services.Lane.Register(this);
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
            ProcessDamageContext(context);
        }
    
        queue.Clear();
    }

    void ProcessDamageContext(DamageContext context)
    {
        if (IsParryable(context))   { SendParryRequest(context);  return; }
                                      SendProcessDamage(context);
    }

    void SendParryRequest(DamageContext context)
    {
        Emit.Global(new ParryEvent(context));
    }

    void SendProcessDamage(DamageContext context)
    {
        Emit.Global(new CalculateDamage(context));
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

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

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.DamageProcessor;

}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
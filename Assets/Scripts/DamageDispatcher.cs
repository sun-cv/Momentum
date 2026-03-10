using System.Collections.Generic;



public class DamageDispatcher : Service, IServiceLoop
{
    
    readonly List<DamageContext> queue = new();

        // -----------------------------------


    // ===============================================================================

    public DamageDispatcher()
    {
        Link.Global<DispatchDamage>(HandleDamageDispatch);
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
        if (IsBlockable(context))   { SendBlockRequest(context);  return; }
                                      SendProcessDamage(context);
    }

    void SendParryRequest(DamageContext context)
    {
        Emit.Global(new ParryEvent(context));
    }

    void SendBlockRequest(DamageContext context)
    {
        Emit.Global(new BlockEvent(context));
    }

    void SendProcessDamage(DamageContext context)
    {
        Emit.Global(Request.Queue, new ProcessDamage(context));
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleDamageDispatch(DispatchDamage message)
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

    bool IsBlockable(DamageContext context)
    {
        return context.Package.Config.Block.Enabled && context.Target is IBlockable actor && actor.Blocking;
    }

    // ===============================================================================

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.CombatInterpreter;

}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct DispatchDamage : IMessage
{
    public DamageContext Context            { get; init; }

    public DispatchDamage(DamageContext context)
    {
        Context = context;
    }
}
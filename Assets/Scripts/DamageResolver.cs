using System.Collections.Generic;
using UnityEngine;



public class DamageResolver : RegisteredService, IServiceLoop
{
    
    readonly List<DamageContext> queue = new(); 

    // ===============================================================================

    public DamageResolver()
    {

        Link.Global<ResolveDamage>(HandleResolveDamageEvent);

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
            ProcessContext(context);
        }
    
        queue.Clear();
    }

    void ProcessContext(DamageContext context)
    {
        foreach (var component in context.Package.Components)
        {
            ProcessComponent(context, component);
        }
    }

    void ProcessComponent(DamageContext context, DamageComponent component)
    {

        // if parried - block at parry system don't pass

        // Send damage for each component to each resource.

        // Send status effects to dot 


    }



    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleResolveDamageEvent(ResolveDamage message)
    {
        queue.Add(message.Context);
    }

    // ===============================================================================
    //  Predicates
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

public readonly struct ResolveDamage    : IMessage
{
    public DamageContext Context            { get; init; }

    public ResolveDamage(DamageContext context)
    {
        Context = context; 
    }
}






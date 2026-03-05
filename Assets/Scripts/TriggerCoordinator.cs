using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;



public class TriggerCoordinator : RegisteredService, IServiceTick, IInitialize
{
    readonly Queue<TriggerEvent> pendingTriggers = new();

    // ===============================================================================

    public void Initialize()
    {
        Link.Global<Message<Request, TriggerEvent>>(HandleTriggerEvent);
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessPendingTriggers();
    }

    // ===============================================================================

    void ProcessPendingTriggers()
    {
        while (pendingTriggers.TryDequeue(out var trigger))
        {
            RouteTrigger(trigger);
        }
    }

    void RouteTrigger(TriggerEvent trigger)
    {
        switch (trigger.Package)
        {
            case DamagePackage package:
                Emit.Global(Request.Create, new CombatEvent(
                    new DamageContext(
                        trigger.Source, 
                        trigger.Target,
                        package
                    )));
                break;
            case ForcePackage package:
                Emit.Global(Request.Create, new ForceEvent(
                    new ForceContext(
                        trigger.Source,
                        trigger.Target,
                        package
                    )));
                break;
        }
    }

    // ===============================================================================
    // Helpers
    // ===============================================================================

    void HandleTriggerEvent(Message<Request, TriggerEvent> message)
    {
        pendingTriggers.Enqueue(message.Payload);
    }

    // ===============================================================================

    public override void Dispose()
    {
        // NO OP;
    }

    public UpdatePriority Priority => ServiceUpdatePriority.TriggerCoordinator;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct TriggerEvent      
{       
    public Actor Source                         { get; init; }
    public Actor Target                         { get; init; }
    public object Package                       { get; init; }
}
using System.Collections.Generic;
using UnityEngine;





public class TriggerCoordinator : RegisteredService, IServiceTick, IInitialize
{
    Queue<TriggerEvent> pendingTriggers = new();

    public void Initialize()
    {
        Link.Global<Message<Request, TriggerEvent>>(HandleTriggerEvent);
    }

    public void Tick()
    {
        ProcessPendingTriggers();
    }

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
                Emit.Global(Request.Create, new CombatEvent
                {
                    Source = trigger.Source,
                    Target = trigger.Target,
                    Package = package
                });
                break;
        }
    }

    void HandleTriggerEvent(Message<Request, TriggerEvent> message)
    {
        pendingTriggers.Enqueue(message.Payload);
    }

    public override void Dispose()
    {
        // NO OP;
    }

    public UpdatePriority Priority => ServiceUpdatePriority.TriggerCoordinator;
}

public struct TriggerEvent      
{       
    public Actor Source                         { get; init; }
    public Actor Target                         { get; init; }
    public object Package                       { get; init; }
}
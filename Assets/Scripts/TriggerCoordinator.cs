using System.Collections.Generic;



public class TriggerCoordinator : RegisteredService, IServiceTick, IInitialize
{
    readonly Queue<TriggerEvent> pendingTriggers = new();

    // ===============================================================================

    public void Initialize()
    {
        Link.Global<TriggerEvent>(HandleTriggerEvent);
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
            case ForcePackage package:
                Emit.Global(new ForceEvent(
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

    void HandleTriggerEvent(TriggerEvent message)
    {
        pendingTriggers.Enqueue(message);
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

public readonly struct TriggerEvent : IMessage
{       
    public Actor Source                         { get; init; }
    public Actor Target                         { get; init; }
    public object Package                       { get; init; }
}
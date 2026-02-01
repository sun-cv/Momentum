







using System;
using System.Collections.Generic;

public class Lifecycle : IServiceStep
{
    readonly Logger Log = Logging.For(LogSystem.Lifecycle);

    public enum State{ Alive, Dying, Dead }

    Actor       owner;
    IDamageable actor;

    State state = State.Alive;

    Dictionary<State, ILifecycleHandler> lifecycleHandlers;

    public Lifecycle(Actor actor)
    {
        if (actor is not IDamageable instance)
            return;

        owner = actor;
    }

    public void Step()
    {
        TickHandler();
    }


    void TickHandler()
    {
        if (lifecycleHandlers.TryGetValue(state, out var handler))
            handler.Tick(this);        
    }


    public void TransitionTo(State state)
    {
        ExitHandler();
        TransitionState(state);
        EnterHandler();
    }

    void EnterHandler()
    {
        if (lifecycleHandlers.TryGetValue(state, out var handler))
            handler.Enter(this);
    }

    void TransitionState(State state)
    {
        this.state = state;
        PublishState();
    }

    void ExitHandler()
    {
        if (lifecycleHandlers.TryGetValue(state, out var handler))
            handler.Exit(this);
    }

    
    void PublishState()
    {
        EmitLocal<LifecyclePublish>(new(Guid.NewGuid(), Publish.StateChange, new() { Owner = owner, State = state }));
    }

    public UpdatePriority Priority => ServiceUpdatePriority.Lifecycle;

    public bool IsAlive => state == State.Alive;
    public bool IsDying => state == State.Dying;
    public bool IsDead  => state == State.Dead;

    public IDamageable Actor => actor;

    void LinkLocal <T>(Action<T> handler) where T : IEvent  => owner.Bus.Subscribe(handler);
    void EmitLocal <T>(T evt) where T : IEvent              => owner.Bus.Raise(evt);
    void LinkGlobal<T>(Action<T> handler) where T : IEvent  => EventBus<T>.Subscribe(handler);
    void EmitGlobal<T>(T evt) where T : IEvent              => EventBus<T>.Raise(evt);
}


public readonly struct LifecyclePayload
{
    public readonly Actor Owner             { get; init; }
    public readonly Lifecycle.State State   { get; init; }
}

public readonly struct LifecyclePublish : ISystemEvent
{
    public Guid Id                          { get; }
    public Publish Action                   { get; }
    public LifecyclePayload Payload         { get; }

    public LifecyclePublish(Guid id, Publish action, LifecyclePayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}



public interface ILifecycleHandler
{
    Lifecycle.State State { get; }

    void Enter(Lifecycle controller);
    void Tick (Lifecycle controller);
    void Exit (Lifecycle controller);
}


public class AliveStateHandler : ILifecycleHandler
{
    public Lifecycle.State State => Lifecycle.State.Alive;

    public void Enter(Lifecycle controller) {}
    public void Tick (Lifecycle controller)
    {
        if (controller.Actor.Health == 0)
            controller.TransitionTo(Lifecycle.State.Dying);
    }
    public void Exit (Lifecycle controller) {}
}

public class DyingStateHandler : ILifecycleHandler
{
    public Lifecycle.State State => Lifecycle.State.Dying;

    public void Enter(Lifecycle controller) {}
    public void Tick (Lifecycle controller) {}
    public void Exit (Lifecycle controller) {} 
}

public class DeadStateHandler  : ILifecycleHandler
{
    public Lifecycle.State State => Lifecycle.State.Dead;

    public void Enter(Lifecycle controller)
    {
        // if (controller.Actor is not IDefined mortal)
        // {
        //     Destroy(controller.Actor);
        //     return;
        // }

        // var lifecycle = mortal.Definition.Lifecycle;

        // if (lifecycle.Animation.Count > 0)

    }
    public void Tick (Lifecycle controller) {}
    public void Exit (Lifecycle controller) {}
}


// public static class DeathAnimationSelector
// {
//     public static string Select(DeathAnimationSet set, KillingBlowEvent blow)
//     {
//         if (set.ByDamageType?.TryGetValue(blow.damageType, out var byType) == true)
//             return byType;
        
//         if (set.Random?.Length > 0)
//             return set.Random[UnityEngine.Random.Range(0, set.Random.Length)];
        
//         return set.Default ?? "death";
//     }
// }

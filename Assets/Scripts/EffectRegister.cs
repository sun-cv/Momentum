using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;





public class EffectInstance : Instance
{
    public Effect   Effect;
    public Instance Owner;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public DualCountdown timer;

    public EffectInstance(Instance instance, Effect effect)
    {
        Owner  = instance;
        Effect = effect;

        CreateTimer();
    }

    public void Initialize()
    {
        timer.OnTimerStart  += OnApply;
        timer.OnTimerStop   += OnClear;

        timer.Start();
    }

    public void Cancel()
    {
        timer.Cancel();
        OnCancel?.Invoke();
    }

    void CreateTimer()
    {
        if (Effect is IDuration time)
            timer = new(time.Duration);

        else

        if (Effect is IDurationFrames frame)
            timer = new(frame.DurationFrames);
    }
}


public class EffectRegister
{
    readonly Logger Log = Logging.For(LogSystem.Effects);

    readonly Actor owner;
    readonly List<EffectInstance> effects = new();

    public EffectRegister(Actor actor)
    {
        owner = actor;
        LinkLocal<EffectRequest>(HandleEffectRequest);
    } 

    void HandleEffectRequest(EffectRequest evt)
    {
        switch(evt.Action)
        {
            case Request.Create:
                    RegisterEffect(evt.Payload.Instance, evt.Payload.Effect);
                break;
            case Request.Cancel:
                    CancelEffect(evt.Payload.Effect);
                break;
        }
    }

    public void RegisterEffect(Instance actor, Effect effect)
    {
        var instance = new EffectInstance(actor, effect);

        instance.OnApply   += () => EmitLocal<EffectPublish>(new(Guid.NewGuid(), Publish.Activated,   new() { Owner = owner, Instance = instance}));
        instance.OnClear   += () => EmitLocal<EffectPublish>(new(Guid.NewGuid(), Publish.Deactivated, new() { Owner = owner, Instance = instance}));
        instance.OnCancel  += () => EmitLocal<EffectPublish>(new(Guid.NewGuid(), Publish.Canceled,    new() { Owner = owner, Instance = instance}));
        
        RegisterTriggerLock(instance);
        RegisterDebugLog(instance);
        
        instance.OnClear   += () => effects.Remove(instance);
        instance.OnCancel  += () => effects.Remove(instance);

        effects.Add(instance);
        instance.Initialize();
    }


    void RegisterTriggerLock(EffectInstance instance)
    {
        if (instance.Effect is not IActionLock effect)
            return;

        if (effect.ActionLocks == null)
            return;

        if (!effect.RequestActionLock)
            return; 
            
        foreach (var action in effect.ActionLocks)
        {
            instance.OnApply   += () => EmitLocal<LockRequest>(new(Guid.NewGuid(), LockTrigger.Lock,   new() { Action = action, Origin = instance.Effect.Name }));
            instance.OnClear   += () => EmitLocal<LockRequest>(new(Guid.NewGuid(), LockTrigger.Unlock, new() { Action = action, Origin = instance.Effect.Name }));
            instance.OnCancel  += () => EmitLocal<LockRequest>(new(Guid.NewGuid(), LockTrigger.Unlock, new() { Action = action, Origin = instance.Effect.Name }));
        }
    }

    void RegisterDebugLog(EffectInstance instance)
    {
        instance.OnApply   += () => Log.Trace("Effects", () => $"Activating Effect {instance.Effect.Name}");
        instance.OnClear   += () => Log.Trace("Effects", () => $"Clearing Effect {instance.Effect.Name}");
        instance.OnCancel  += () => Log.Trace("Effects", () => $"Canceling Effect {instance.Effect.Name}");
    }

    public void CancelEffect(Effect effect) => effects.FirstOrDefault(instance => instance.Effect.RuntimeID == effect.RuntimeID)?.Cancel();

    public bool Is<T>() where T : class
    {
        foreach (var instance in effects)
        {
            if (instance.Effect is T effect)
                return true; 
        }
        return false;
    }


    public bool Can<T>(Func<T, bool> isBlocked, bool defaultValue = true) where T : class
    {
        foreach (var instance in effects)
        {
            if (instance.Effect is T effect && isBlocked(effect))
                return false; 
        }
        return defaultValue;
    }

    public bool Has<T>(Func<T, bool> hasCondition, bool defaultValue = false) where T : class
    {
        foreach (var instance in effects)
        {
            if (instance.Effect is T effect && hasCondition(effect))
                return true; 
        }
        return defaultValue;
    }

    void LinkLocal <T>(Action<T> handler) where T : IEvent  => owner.Bus.Subscribe(handler);
    void EmitLocal <T>(T evt) where T : IEvent              => owner.Bus.Raise(evt);


    public List<EffectInstance> Effects => effects;
}

public enum EffectAction
{
    Create,
    Cancel,
    Get,
}

public readonly struct EffectPayload
{
    public Actor Owner                      { get; init; }
    public Effect Effect                    { get; init; }
    public Instance Instance                { get; init; }
}

public readonly struct EffectStatePayload
{
    public Actor Owner                      { get; init; }
    public EffectInstance Instance          { get; init; }
}

public readonly struct EffectRequest : ISystemEvent
{
    public Guid Id                          { get; }
    public Request Action                   { get; }
    public EffectPayload Payload            { get; }

    public EffectRequest(Guid id, Request action, EffectPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}


public readonly struct EffectPublish : ISystemEvent
{
    public Guid Id                          { get; }
    public Publish Action                   { get; }
    public EffectStatePayload Payload       { get; }

    public EffectPublish(Guid id, Publish action, EffectStatePayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}



public class EffectCache : IDisposable
{
    readonly LocalEventbus bus;
    readonly EventBinding<EffectPublish> binding;
    readonly List<EffectInstance> activeEffects = new();

    readonly Func<EffectInstance, bool> filter;

    public Action<EffectInstance> OnApply;
    public Action<EffectInstance> OnClear;
    public Action<EffectInstance> OnCancel;

    public EffectCache(LocalEventbus bus, Func<EffectInstance, bool> filter = null)
    {
        this.bus    = bus;
        this.filter = filter;

        this.bus.Subscribe<EffectPublish>(HandleEffectPublish);
    }

    public void HandleEffectPublish(EffectPublish evt)
    {
        var instance = evt.Payload.Instance;

        if (filter != null && !filter(instance))
            return;

        switch(evt.Action)
        {
            case Publish.Activated:
                activeEffects.Add(instance);
                OnApply?.Invoke(instance);
                break;
            case Publish.Canceled:
                if (activeEffects.Remove(instance))
                    OnCancel?.Invoke(instance);
                break;
            case Publish.Deactivated:               
                if (activeEffects.Remove(instance))
                    OnClear?.Invoke(instance);
                break;
        }
    }

    public void Bind(LocalEventbus eventbus)
    {
        eventbus.Subscribe<EffectPublish>(HandleEffectPublish);
    }

    public IReadOnlyList<EffectInstance> Effects => activeEffects;
    public void Dispose() => bus.Unsubscribe<EffectPublish>(binding);
}


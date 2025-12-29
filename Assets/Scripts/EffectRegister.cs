using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;





public class EffectInstance
{
    public Entity Owner;
    public Effect Effect;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public DualCountdown timer;

    public EffectInstance(Entity entity, Effect effect)
    {
        Owner  = entity;
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
            
        if (Effect is IDurationFrames frame)
            timer = new(frame.DurationFrames);
    }
}


public class EffectRegister : RegisteredService
{
    List<EffectInstance> effects = new();

    public override void Initialize()
    {
        EventBus<EffectRequest>.Subscribe(HandleEffectRequest);
    } 

    void HandleEffectRequest(EffectRequest evt)
    {
        switch(evt.Action)
        {
            case EffectAction.Create:
                    RegisterEffect(evt.Payload.Entity, evt.Payload.Effect);
                break;
            case EffectAction.Cancel:
                    CancelEffect(evt.Payload.Effect);
                break;
            case EffectAction.Get:
                    GetEffects(evt);
                break;

        }
    }

    public void RegisterEffect(Entity entity, Effect effect)
    {
        var instance = new EffectInstance(entity, effect);

        instance.OnApply   += () => EventBus<EffectPublish>.Raise(new(Guid.NewGuid(), Publish.Activated,   new() { Instance = instance}));
        instance.OnClear   += () => EventBus<EffectPublish>.Raise(new(Guid.NewGuid(), Publish.Deactivated, new() { Instance = instance}));
        instance.OnCancel  += () => EventBus<EffectPublish>.Raise(new(Guid.NewGuid(), Publish.Canceled,    new() { Instance = instance}));
        
        RegisterTriggerLock(instance);
        
        instance.OnClear   += () => effects.Remove(instance);
        instance.OnCancel  += () => effects.Remove(instance);

        effects.Add(instance);
        instance.Initialize();

        Debug.Log(effect.Name);
    }


    void RegisterTriggerLock(EffectInstance instance)
    {
        if (instance.Effect is not IActionLock effect)
            return;

        if (effect.ActionLocks == null)
            return;

        foreach (var action in effect.ActionLocks)
        {
            instance.OnApply   += () => EventBus<LockRequest>.Raise(new(Guid.NewGuid(), LockAction.Lock,   new() { Action = action, Origin = instance.Effect.Name }));
            instance.OnClear   += () => EventBus<LockRequest>.Raise(new(Guid.NewGuid(), LockAction.Unlock, new() { Action = action, Origin = instance.Effect.Name }));
            instance.OnCancel  += () => EventBus<LockRequest>.Raise(new(Guid.NewGuid(), LockAction.Unlock, new() { Action = action, Origin = instance.Effect.Name }));
        }
    }

    public void CancelEffect(Effect effect)         => effects.FirstOrDefault(instance => instance.Effect.RuntimeID == effect.RuntimeID)?.Cancel();
    public void GetEffects(EffectRequest request)   => EventBus<EffectResponse>.Raise(new(request.Id, Response.Success, new() { Effects = GetEntityEffects(request.Payload.Entity) }));

    private static T FirstEffectOrDefault<T>(IEnumerable<EffectInstance> source, Func<EffectInstance, bool> predicate = null) where T : class
    {
        var query = source.Select(e => e.Effect);
        if (predicate != null)
            query = source.Where(predicate).Select(e => e.Effect);

        return query.OfType<T>().FirstOrDefault();
    }

    public T GetPredicate<T>(Func<EffectInstance,bool> predicate = null) where T : class
    {
        return FirstEffectOrDefault<T>(effects, predicate);
    }

    public List<Effect> GetEntityEffects(Entity entity)
    {
        return effects.Where(instance => instance.Owner == entity).Select(instance => instance.Effect).ToList();
    }

    public bool Get<T>(Func<T, bool> selector, bool defaultValue = true) where T : class => GetPredicate<T>() != null ? selector(GetPredicate<T>()) : defaultValue;


    public List<EffectInstance> Effects => effects;
}


public enum EffectAction
{
    Create,
    Cancel,
    Get,
}

public readonly struct EffectRequestPayload
{
    public Entity Entity { get; init; }
    public Effect Effect { get; init; }
}

public readonly struct EffectResponsePayload
{
    public Entity Entity { get; init; }
    public readonly List<Effect> Effects { get; init; }
}

public readonly struct EffectStatePayload
{
    public EffectInstance Instance { get; init; }
}

public readonly struct EffectRequest : IEventRequest
{
    public Guid Id                      { get; }
    public EffectAction Action          { get; }
    public EffectRequestPayload Payload { get; }

    public EffectRequest(Guid id, EffectAction action, EffectRequestPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}

public readonly struct EffectResponse : IEventResponse
{
    public Guid Id                      { get; }
    public Response Action              { get; }
    public EffectResponsePayload Payload   { get; }

    public EffectResponse(Guid id, Response response, EffectResponsePayload payload)
    {
        Id       = id;
        Action   = response;
        Payload  = payload;
    }
}

public readonly struct EffectPublish : IEventPublish
{
    public Guid Id                      { get; }
    public Publish Action               { get; }
    public EffectStatePayload Payload   { get; }

    public EffectPublish(Guid id, Publish action, EffectStatePayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}



public class EffectCache : IDisposable
{
    readonly EventBinding<EffectPublish> binding;
    readonly List<EffectInstance> activeEffects = new();

    readonly Func<EffectInstance, bool> filter;

    public Action<EffectInstance> OnApply;
    public Action<EffectInstance> OnClear;
    public Action<EffectInstance> OnCancel;

    public EffectCache(Func<EffectInstance, bool> filter = null)
    {
        this.filter = filter;
        binding     = EventBus<EffectPublish>.Subscribe(HandleEffectPublish);
    }

    void HandleEffectPublish(EffectPublish evt)
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

    public IReadOnlyList<EffectInstance> Effects => activeEffects;
    public void Dispose() => EventBus<EffectPublish>.Unsubscribe(binding);
}


    // public bool TryGet<T>(out T effect, Func<EffectInstance,bool> predicate = null) where T : class
    // {
    //     effect = FirstEffectOrDefault<T>(effects, predicate);
    //     return effect != null;
    // }


    // public T GetEffectType<T>() where T : class => FirstEffectOrDefault<T>(effects);
    // public bool TryGetEffectType<T>(out T effect) where T : class
    // {
    //     effect = GetEffectType<T>();
    //     return effect != null;
    // }

    // public T GetEntityEffectType<T>(Entity entity) where T : class => FirstEffectOrDefault<T>(effects, e => e.Owner == entity);
    // public bool TryGetEntityEffectType<T>(Entity entity, out T effect) where T : class
    // {
    //     effect = GetEntityEffectType<T>(entity);
    //     return effect != null;
    // }

    // public T GetEffectClass<T>(string className) where T : class => FirstEffectOrDefault<T>(effects, e => e.Effect.Class == className);
    // public bool TryGetEffectClass<T>(string className, out T effect) where T : class
    // {
    //     effect = GetEffectClass<T>(className);
    //     return effect != null;
    // }

    // public T GetEntityEffectClass<T>(Entity entity, string className) where T : class => FirstEffectOrDefault<T>(effects, e => e.Owner == entity && e.Effect.Class == className);
    // public bool TryGetEntityEffectClass<T>(Entity entity, string className, out T effect) where T : class
    // {
    //     effect = GetEntityEffectClass<T>(entity, className);
    //     return effect != null;
    // }

    // public EffectInstance GetEffect(Effect effect)  => effects.Where(instance => instance.Effect == effect).FirstOrDefault();
    // public List<EffectInstance> GetEntityEffects(Entity entity) => effects.Where(instance => instance.Owner == entity).ToList();

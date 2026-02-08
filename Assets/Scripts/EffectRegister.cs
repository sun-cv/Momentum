using System;
using System.Collections.Generic;
using System.Linq;





public class EffectInstance : Instance
{
    public Runtime  Owner;
    public Effect   Effect;

    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

    public DualCountdown timer;

    public EffectInstance(Runtime runtime, Effect effect)
    {
        Owner  = runtime;
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
        owner.Emit.Link.Local<Request, EffectDeclarationEvent>(HandleEffectRequest);
        owner.Emit.Link.Local<Request, EffectInstanceEvent>   (HandleEffectCancellation);
    } 

    void HandleEffectRequest(Message<Request, EffectDeclarationEvent> message)
    {                    
        RegisterEffect(message.Payload.Runtime, message.Payload.Effect);
    }

    void HandleEffectCancellation(Message<Request, EffectInstanceEvent> message)
    {
        CancelEffect(message.Payload.Instance.Effect);
    }

    public void RegisterEffect(Runtime runtime, Effect effect)
    {
        var instance = new EffectInstance(runtime, effect);

        instance.OnApply   += () => owner.Emit.Local(Guid.NewGuid(), Publish.Activated,   new EffectInstanceEvent(instance));
        instance.OnClear   += () => owner.Emit.Local(Guid.NewGuid(), Publish.Deactivated, new EffectInstanceEvent(instance));
        instance.OnCancel  += () => owner.Emit.Local(Guid.NewGuid(), Publish.Canceled,    new EffectInstanceEvent(instance));
        
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
            instance.OnApply   += () => owner.Emit.Local(Request.Lock,   new LockEvent(action, instance.Effect.Name));
            instance.OnClear   += () => owner.Emit.Local(Request.Unlock, new LockEvent(action, instance.Effect.Name));
            instance.OnCancel  += () => owner.Emit.Local(Request.Unlock, new LockEvent(action, instance.Effect.Name));
        }
    }

    void RegisterDebugLog(EffectInstance instance)
    {
        instance.OnApply   += () => Log.Trace($"Activating Effect {instance.Effect.Name}");
        instance.OnClear   += () => Log.Trace($"Clearing Effect {instance.Effect.Name}");
        instance.OnCancel  += () => Log.Trace($"Canceling Effect {instance.Effect.Name}");
    }

    public void CancelEffect(Effect effect)
    {
        if (effect is ICancelable instance && instance.Cancelable)
            effects.FirstOrDefault(instance => instance.Effect.RuntimeID == effect.RuntimeID)?.Cancel();
    }

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

    public List<EffectInstance> Effects => effects;
}




public readonly struct EffectDeclarationEvent
{
    public Effect Effect                    { get; init; }
    public Runtime Runtime                  { get; init; }

    public EffectDeclarationEvent(Runtime runtime, Effect effect)
    {
        Effect  = effect;
        Runtime = runtime;
    }
}

public readonly struct EffectInstanceEvent
{
    public EffectInstance Instance          { get; init; }

    public EffectInstanceEvent(EffectInstance instance)
    {
        Instance    = instance;
    }
}


public class EffectCache : IDisposable
{
    readonly Emit emit;
    readonly EventBinding<Message<Publish, EffectInstanceEvent>> binding;
    readonly List<EffectInstance> activeEffects = new();

    readonly Func<EffectInstance, bool> filter;

    public Action<EffectInstance> OnApply;
    public Action<EffectInstance> OnClear;
    public Action<EffectInstance> OnCancel;

    public EffectCache(Emit emit, Func<EffectInstance, bool> filter = null)
    {
        this.emit   = emit;
        this.filter = filter;

        this.emit.Link.Local<Message<Publish, EffectInstanceEvent>>(HandleEffectPublish);
    }

    public void HandleEffectPublish(Message<Publish, EffectInstanceEvent> message)
    {
        var instance = message.Payload.Instance;

        if (filter != null && !filter(instance))
            return;

        switch(message.Action)
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
        eventbus.Subscribe<Message<Publish, EffectInstanceEvent> >(HandleEffectPublish);
    }

    public IReadOnlyList<EffectInstance> Instances => activeEffects.ToList();
    public void Dispose() => emit.Link.UnsubscribeLocal(binding);
}


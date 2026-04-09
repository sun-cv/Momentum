using System;
using System.Collections.Generic;
using System.Linq;



public class EffectRegister : ActorService, IServiceTick
{

    readonly List<EffectAPI> queue   = new();
    readonly List<EffectInstance> effects       = new();

    // ===============================================================================

    public EffectRegister(Actor actor) : base(actor)
    {
        owner.Bus.Link.Local<Message<Request, EffectAPI>>(HandleEffectAPI);
    } 

    // ===============================================================================
    //  Public API
    // ===============================================================================

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

    public bool Is<T>() where T : class
    {
        foreach (var instance in effects)
        {
            if (instance.Effect is T)
                return true; 
        }
        return false;
    }
    
    // ===============================================================================

    public void Tick()
    {
        ProcessQueues();
    }

    void ProcessQueues()
    {
        foreach(var request in queue)
        {
            ProcessRequest(request);
        }
        
        queue.Clear();
    }

    void ProcessRequest(EffectAPI request)
    {
        switch(request.Request)
        {
            case Request.Create: RegisterEffect(request); break;
            case Request.Cancel: CancelEffect(request);   break;
        }
    }



    // ===============================================================================

    public void RegisterEffect(EffectAPI request)
    {
        var instance = new EffectInstance(request.Runtime, request.Effect);

        instance.OnApply   += () => owner.Bus.Emit.Local(new EffectEvent(instance));
        instance.OnClear   += () => owner.Bus.Emit.Local(new EffectEvent(instance));
        instance.OnCancel  += () => owner.Bus.Emit.Local(new EffectEvent(instance));
        
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
            instance.OnApply   += () => owner.Bus.Emit.Local(new LockEvent(action, instance.Effect.Name, Request.Lock));
            instance.OnClear   += () => owner.Bus.Emit.Local(new LockEvent(action, instance.Effect.Name, Request.Unlock));
            instance.OnCancel  += () => owner.Bus.Emit.Local(new LockEvent(action, instance.Effect.Name, Request.Unlock));
        }
    }


    void RegisterDebugLog(EffectInstance instance)
    {
        instance.OnApply   += () => Log.Trace($"Activating Effect {instance.Effect.Name}");
        instance.OnClear   += () => Log.Trace($"Clearing Effect {instance.Effect.Name}");
        instance.OnCancel  += () => Log.Trace($"Canceling Effect {instance.Effect.Name}");
    }

    public void CancelEffect(EffectAPI request)
    {
        if (request.Instance.Effect is ICancelable effect && effect.Cancelable)
        {
            effects.FirstOrDefault(instance => instance.RuntimeId == request.Instance.RuntimeId)?.Cancel();
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleEffectAPI(Message<Request, EffectAPI> message)
    {                    
        queue.Add(message.Payload);
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Effects);

    public List<EffectInstance> Effects => effects;
    public UpdatePriority Priority      => ServiceUpdatePriority.EffectRegister;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class EffectInstance : Instance
{
    public enum EffectState { Active, Completed, Canceled }

        // -----------------------------------

    public Runtime  Owner;
    public Effect   Effect;

        // -----------------------------------

    public EffectState State { get; private set; }


    public Action OnApply;
    public Action OnClear;
    public Action OnCancel;

        // -----------------------------------

    public DualCountdown timer;

    // ===============================================================================

    public EffectInstance(Runtime runtime, Effect effect)
    {
        Owner  = runtime;
        Effect = effect;

        CreateTimer();
    }

    public void Initialize()
    {
        timer.OnTimerStart  += () => { State = EffectState.Active;      OnApply?.Invoke(); };
        timer.OnTimerStop   += () => { State = EffectState.Completed;   OnClear?.Invoke(); };

        timer.Start();
    }

    // ===============================================================================
    
    void CreateTimer()
    {
        if (Effect is IDuration time)
            timer = new(time.Duration);

        else

        if (Effect is IDurationFrames frame)
            timer = new(frame.DurationFrames);
    }

    public void Cancel()
    {
        State = EffectState.Canceled;
        timer.Cancel();
        OnCancel?.Invoke();
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Utilities
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class EffectCache : IDisposable
{
    readonly Bus bus;

        // -----------------------------------

    readonly Func<EffectInstance, bool> filter;

    public Action<EffectInstance> OnApply;
    public Action<EffectInstance> OnClear;
    public Action<EffectInstance> OnCancel;

        // -----------------------------------

    readonly List<EffectInstance> activeEffects = new();
    readonly EventBinding<EffectEvent> binding;

    // ===============================================================================

    public EffectCache(Bus bus, Func<EffectInstance, bool> filter = null)
    {
        this.bus    = bus;
        this.filter = filter;

        binding = this.bus.Link.Local<EffectEvent>(HandleEffectPublish);
    }

    // ===============================================================================

    public void HandleEffectPublish(EffectEvent message)
    {
        var instance = message.Instance;

        if (filter != null && !filter(instance))
            return;

        switch(instance.State)
        {
            case EffectInstance.EffectState.Active:
                activeEffects.Add(instance);
                OnApply?.Invoke(instance);
                break;
            case EffectInstance.EffectState.Canceled:
                if (activeEffects.Remove(instance))
                    OnCancel?.Invoke(instance);
                break;
            case EffectInstance.EffectState.Completed:               
                if (activeEffects.Remove(instance))
                    OnClear?.Invoke(instance);
                break;
        }
    }

    // ===============================================================================

    public void Bind(LocalEventBus eventbus)
    {
        eventbus.Subscribe<EffectEvent>(HandleEffectPublish);
    }

    public void Dispose()
    {
        bus.Link.UnsubscribeLocal(binding);
    }

    public IReadOnlyList<EffectInstance> Instances => activeEffects.ToList();
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class EffectEvent : IMessage
{
    public EffectInstance Instance  { get; set; }

    public EffectEvent(EffectInstance instance)
    {
        Instance = instance;
    }
}

public class EffectAPI : API
{
    public Effect    Effect         { get; init; }
    public Runtime   Runtime        { get; init; }

    public EffectInstance Instance  { get; set; }

    public EffectAPI(Runtime id, Effect effect)
    {
        Runtime = id;
        Effect  = effect;   
    }
    
    public EffectAPI(EffectInstance instance)
    {
        Instance = instance;
    }

}

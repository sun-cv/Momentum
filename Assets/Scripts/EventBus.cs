using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public static class EventBus<T> where T : IEvent 
{
    static readonly HashSet<IEventBinding<T>> bindings = new();

    // ===============================================================================

    public static EventBinding<T> Subscribe(Action<T> handler)
    {
        var binding = new EventBinding<T>(handler);
        Register(binding);
        return binding;
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public static void Register(EventBinding<T>   binding)
    {
        bindings.Add(binding);
    }

    public static void Deregister(EventBinding<T> binding)
    {
        bindings.Remove(binding);
    }

    public static void Unsubscribe(EventBinding<T> binding)
    {
        Deregister(binding);
    }

    public static T SubscribeOnce(Action triggerEvent)
    {
        T result = default;
        EventBinding<T> binding = null;

        binding = Subscribe(evt =>
        {
            result = evt;
            Unsubscribe(binding);
        });

        triggerEvent();

        return result;
    }

    public static void Raise(T @event) 
    {
        var snapshot = new HashSet<IEventBinding<T>>(bindings);
        foreach (var binding in snapshot) 
        {
            if (bindings.Contains(binding)) 
            {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }
    }

    public static void Clear()
    {
        bindings.Clear();
    }
}


public class LocalEventbus : IDisposable
{
    private readonly Dictionary<Type, HashSet<object>> bindings = new();

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public EventBinding<T> Subscribe<T>(Action<T> handler) where T : IEvent
    {
        if (!bindings.TryGetValue(typeof(T), out var set))
        {
            set = new HashSet<object>();
            bindings[typeof(T)] = set;
        }

        var binding = new EventBinding<T>(handler);
        set.Add(binding);
        return binding;
    }
    
    public void Unsubscribe<T>(EventBinding<T> binding) where T : IEvent
    {
        if (bindings.TryGetValue(typeof(T), out var set))
        {
            set.Remove(binding);
            if (set.Count == 0)
                bindings.Remove(typeof(T));
        }
    }

    public void Raise<T>(T @event) where T : IEvent
    {
        if (!bindings.TryGetValue(typeof(T), out var set)) return;
        foreach (var obj in set.Cast<EventBinding<T>>().ToArray())
            ((IEventBinding<T>)obj).OnEvent.Invoke(@event);
    }

    // ===============================================================================


    public void Dispose()
    {
        bindings.Clear();
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IEvent {};
public interface ISystemEvent : IEvent { public Guid Id { get; } }

public interface IEventBinding<T> 
{
    public Action<T> OnEvent    { get; set; }
    public Action OnEventNoArgs { get; set; }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class EventBinding<T> : IEventBinding<T> where T : IEvent 
{
    Action<T> onEvent    = _  => { };
    Action onEventNoArgs = () => { };

    Action<T> IEventBinding<T>.OnEvent 
    {
        get => onEvent;
        set => onEvent = value;
    }

    Action IEventBinding<T>.OnEventNoArgs 
    {
        get => onEventNoArgs;
        set => onEventNoArgs = value;
    }

    // ===============================================================================

    public EventBinding(Action<T> onEvent)      => this.onEvent         = onEvent;
    public EventBinding(Action onEventNoArgs)   => this.onEventNoArgs   = onEventNoArgs;
    public EventBinding()                       => this.onEventNoArgs   = (() => {});

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void Add(Action onEvent)             => onEventNoArgs += onEvent;
    public void Remove(Action onEvent)          => onEventNoArgs -= onEvent;
    
    public void Add(Action<T> onEvent)          => this.onEvent  += onEvent;
    public void Remove(Action<T> onEvent)       => this.onEvent  -= onEvent;
}

public class Emit : IDisposable
{
    public LocalEventbus Bus    { get; }
    public Link Link            { get; }

    // ===============================================================================

    public Emit()
    {
        Bus     = new ();
        Link    = new (Bus);
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void Local<TAction, TPayload>(TAction action, TPayload payload) 
    {
        Bus.Raise(new Message<TAction, TPayload>(action, payload));
    }

    public void Local<TAction, TPayload>(Guid id, TAction action, TPayload payload) 
    {
        Bus.Raise(new Message<TAction, TPayload>(id, action, payload));
    }

    public void Local<TMessage>(TMessage message) where TMessage : IEvent
    {
        Bus.Raise(message);
    }

    public void Local<TAction>(Guid id, TAction action) 
    {
        Bus.Raise(new EmptyMessage<TAction>(id, action));
    }    

    public static void Global<TAction, TPayload>(TAction action, TPayload payload) 
    {
        EventBus<Message<TAction, TPayload>>.Raise(new Message<TAction, TPayload>(action, payload));
    }

    public static void Global<TAction, TPayload>(Guid id, TAction action, TPayload payload) 
    {
        EventBus<Message<TAction, TPayload>>.Raise(new Message<TAction, TPayload>(id, action, payload));
    }

    public static void Global<TMessage>(TMessage message) where TMessage : IEvent
    {
        EventBus<TMessage>.Raise(message);
    }

    // ===============================================================================


    public void Dispose()
    {
        Bus.Dispose();
    }

}

public class Link
{
    LocalEventbus Bus { get; }

    // ===============================================================================

    public Link(LocalEventbus bus)
    {
        Bus = bus;
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public EventBinding<T> Local<T>(Action<T> handler) where T : IEvent
    {
        return Bus.Subscribe(handler);
    }

    public EventBinding<T> LocalBinding<T>(Action<T> handler) where T : IEvent
    {
        return Bus.Subscribe(handler);
    }

    public EventBinding<Message<TAction, TPayload>> Local<TAction, TPayload>(Action<Message<TAction, TPayload>> handler)
    {
        return Bus.Subscribe(handler);
    }

    public void UnsubscribeLocal<T>(EventBinding<T> binding) where T : IEvent
    {
        Bus.Unsubscribe(binding);
    }

    public static EventBinding<T> Global<T>(Action<T> handler) where T : IEvent
    {
        return EventBus<T>.Subscribe(handler);
    }

    public static EventBinding<Message<TAction, TPayload>> Global<TAction, TPayload>(Action<Message<TAction, TPayload>> handler)
    {
        return EventBus<Message<TAction, TPayload>>.Subscribe(handler);
    }

    public static void UnsubscribeGlobal<T>(EventBinding<T> binding) where T : IEvent
    {
        EventBus<T>.Unsubscribe(binding);
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Utilities
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class GlobalEventHandler<TResponse> : IDisposable where TResponse : ISystemEvent
{
    
    readonly Action<TResponse> onResponse;

        // -----------------------------------

    readonly HashSet<Guid>  pendingIds = new();
    readonly EventBinding<TResponse> binding;

    // ===============================================================================

    public GlobalEventHandler(Action<TResponse> onResponse)
    {
        this.onResponse = onResponse;

        binding = Link.Global<TResponse>(Receive);
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void Send<TAction, TPayload>(TAction action, TPayload payload)
    {
        var message = new Message<TAction, TPayload>(action, payload);
        pendingIds.Add(message.Id);
        EventBus<Message<TAction, TPayload>>.Raise(message);
    }

    void Receive(TResponse response)
    {
        if (!pendingIds.Remove(response.Id))
            return;

        onResponse(response);
    }

    public void Clear()
    {
        pendingIds.Clear();
    }

    public void Dispose()
    {
        Link.UnsubscribeGlobal(binding);
    }

    // ===============================================================================


    public int PendingCount => pendingIds.Count;
}

public class LocalEventHandler<TResponse> : IDisposable  where TResponse : ISystemEvent
{
    readonly Emit emit;

        // -----------------------------------

    readonly Action<TResponse> onResponse;

        // -----------------------------------

    readonly HashSet<Guid> pendingIds = new();
    readonly EventBinding<TResponse> binding;

    // ===============================================================================

    public LocalEventHandler(Emit emit, Action<TResponse> onResponse)
    {
        this.emit = emit;
        this.onResponse = onResponse;
        
        binding = emit.Link.Local<TResponse>(Receive);
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void Send<TAction, TPayload>(TAction action, TPayload payload)
    {
        Forward(Guid.NewGuid(), action, payload);
    }

    public void Forward<TAction, TPayload>(Guid id, TAction action, TPayload payload)
    {
        var message = new Message<TAction, TPayload>(id, action, payload);
        pendingIds.Add(message.Id);
        emit.Local(message);
    }

    void Receive(TResponse response)
    {
        if (!pendingIds.Remove(response.Id))
            return;

        onResponse(response);
    }

    public void Clear() => pendingIds.Clear();

    public void Dispose()
    {
        emit.Link.UnsubscribeLocal(binding);
    }
    
    // ===============================================================================


    public int PendingCount => pendingIds.Count;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct Message<TAction, TPayload> : ISystemEvent
{
    public Guid Id              { get; }
    public TPayload Payload     { get; }
    public TAction Action       { get; }

    public Message(TAction action, TPayload payload)
    {
        Id          = Guid.NewGuid();
        Action      = action;
        Payload     = payload;
    }

    public Message(Guid id, TAction action, TPayload payload)
    {
        Id          = id;
        Action      = action;
        Payload     = payload;
    }
}

public readonly struct EmptyMessage<TAction> : ISystemEvent
{
    public Guid Id              { get; }
    public TAction Action       { get; }

    public EmptyMessage(Guid id, TAction action)
    {
        Id          = id;
        Action      = action;
    }
}

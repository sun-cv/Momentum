using System;
using System.Collections.Generic;
using System.Linq;




public interface IEvent {};
public interface ISystemEvent : IEvent { public Guid Id { get; } }

public interface IEventBinding<T> 
{
    public Action<T> OnEvent    { get; set; }
    public Action OnEventNoArgs { get; set; }
}

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

    public EventBinding(Action<T> onEvent)      => this.onEvent = onEvent;
    public EventBinding(Action onEventNoArgs)   => this.onEventNoArgs = onEventNoArgs;
    public EventBinding()                       => this.onEventNoArgs = (() => {});

    
    public void Add(Action onEvent)             => onEventNoArgs += onEvent;
    public void Remove(Action onEvent)          => onEventNoArgs -= onEvent;
    
    public void Add(Action<T> onEvent)          => this.onEvent += onEvent;
    public void Remove(Action<T> onEvent)       => this.onEvent -= onEvent;
}





public static class EventBus<T> where T : IEvent 
{
    static readonly HashSet<IEventBinding<T>> bindings = new();

    public static EventBinding<T> Subscribe(Action<T> handler)
    {
        var binding = new EventBinding<T>(handler);
        Register(binding);
        return binding;
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

    public static void Register(EventBinding<T>   binding)  => bindings.Add(binding);
    public static void Deregister(EventBinding<T> binding)  => bindings.Remove(binding);

    static void Clear()                                     => bindings.Clear();
    static bool DelegateEquals(Action<T> a, Action<T> b)    => a?.GetInvocationList().SequenceEqual(b?.GetInvocationList()) ?? false;
}





public class LocalEventbus
{
    private readonly Dictionary<Type, HashSet<object>> bindings = new();

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
}




public class EventBus
{
    public sealed class Signal<T> 
    {
        private readonly Dictionary<string, List<Action<T>>> payloadHandlers = new();
        private readonly Dictionary<string, List<Action>>    noArgHandlers   = new();

        public void Subscribe(string signal, Action<T> callback)
        {
            if (!payloadHandlers.TryGetValue(signal, out var list))
            {
                list = new List<Action<T>>();
                payloadHandlers[signal] = list;
            }
            list.Add(callback);
        }

        public void Subscribe(string signal, Action callback)
        {
            if (!noArgHandlers.TryGetValue(signal, out var list))
            {
                list = new List<Action>();
                noArgHandlers[signal] = list;
            }
            list.Add(callback);
        }

        public void Unsubscribe(string signal, Action<T> callback)
        {
            if (payloadHandlers.TryGetValue(signal, out var list))
            {
                list.Remove(callback);
                if (list.Count == 0) payloadHandlers.Remove(signal);
            }
        }

        public void Unsubscribe(string signal, Action callback)
        {
            if (noArgHandlers.TryGetValue(signal, out var list))
            {
                list.Remove(callback);
                if (list.Count == 0) noArgHandlers.Remove(signal);
            }
        }

        public void Publish(string signal, object payload = null) 
        {
            if (payload != null)
            {
                if (payloadHandlers.TryGetValue(signal, out var payloadList))
                    foreach (var callback in payloadList.ToArray())
                        callback((T)payload);
            }
                if (noArgHandlers.TryGetValue(signal, out var noArgList))
                    foreach (var callback in noArgList.ToArray())
                        callback();
        }
    }


    public class Local<T> where T : IEvent 
    {
        static readonly HashSet<IEventBinding<T>> bindings = new();

        public static EventBinding<T> Subscribe(Action<T> handler)
        {
            var binding = new EventBinding<T>(handler);
            Register(binding);
            return binding;
        }

        public static void Unsubscribe(EventBinding<T> binding)
        {
            Deregister(binding);
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

        public static void Register(EventBinding<T>   binding)  => bindings.Add(binding);
        public static void Deregister(EventBinding<T> binding)  => bindings.Remove(binding);

        static void Clear()                                     => bindings.Clear();
        static bool DelegateEquals(Action<T> a, Action<T> b)    => a?.GetInvocationList().SequenceEqual(b?.GetInvocationList()) ?? false;
    }
}




public class EventCache<TRequest, TResponse> where TRequest : ISystemEvent where TResponse : ISystemEvent
{
    private readonly Dictionary<Guid, TRequest> pending = new();
    private readonly Action<TRequest, TResponse> onResponse;

    public EventCache(Action<TRequest, TResponse> onResponse)
    {
        this.onResponse = onResponse;
        EventBus<TResponse>.Subscribe(Receive);
    }

    public Guid Send(TRequest request)
    {
        var id = Guid.NewGuid();
        pending[id] = request;

        OnEvent(request);

        return id;
    }

    void Receive(TResponse response)
    {
        if (!pending.TryGetValue(response.Id, out var request))
            return;

        onResponse(request, response);
        pending.Remove(response.Id);
    }


    public void Clear()
    {
        pending.Clear();
    }

    void OnEvent<T>(T evt) where T : IEvent => EventBus<T>.Raise(evt);

    public int PendingCount => pending.Count;
}
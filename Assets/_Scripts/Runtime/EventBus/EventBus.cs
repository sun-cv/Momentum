using System;
using System.Collections.Generic;
using System.Linq;

namespace Momentum

{

public static class EventBus<T> where T : IEvent 
{
    static readonly HashSet<IEventBinding<T>> bindings = new();
    
    public static void Register(EventBinding<T>   binding) => bindings.Add(binding);
    public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

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
    
    static bool DelegateEquals(Action<T> a, Action<T> b)
    {
        return a?.GetInvocationList().SequenceEqual(b?.GetInvocationList()) ?? false;
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


    static void Clear() 
    {
        // Debug.Log($"Clearing {typeof(T).Name} bindings");
        bindings.Clear();
    }
}
}
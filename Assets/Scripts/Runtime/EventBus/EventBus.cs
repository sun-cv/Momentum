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

    }
}
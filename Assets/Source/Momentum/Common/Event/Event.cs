using System;
using System.Collections.Generic;



namespace Game.Common
{

    public interface IEvent {};

    public static class Event 
    {
        private static readonly Dictionary<Type, Dictionary<Type, object>> mailboxes    = new();
        private static readonly Dictionary<Type, Delegate> handlers                     = new();
        private static readonly Dictionary<Type, Delegate> handlersNoArgs               = new();

        public static void Register<TOwner, TEvent>() where TEvent : IEvent
        {
            if (!mailboxes.TryGetValue(typeof(TEvent), out var owners))
            {
                owners = new();
                mailboxes[typeof(TEvent)] = owners;
            }

            if (!owners.ContainsKey(typeof(TOwner)))
                owners[typeof(TOwner)] = new List<TEvent>();
        }

        public static void Send<TEvent>(TEvent message) where TEvent : IEvent
        {
            if (!mailboxes.TryGetValue(typeof(TEvent), out var owners))
                return;

            foreach (var mailbox in owners.Values)
                ((List<TEvent>)mailbox).Add(message);
        }

        public static List<TEvent> Read<TOwner, TEvent>() where TEvent : IEvent
        {
            if (!mailboxes.TryGetValue(typeof(TEvent), out var owners))
            {
                owners = new();
                mailboxes[typeof(TEvent)] = owners;
            }

            if (!owners.TryGetValue(typeof(TOwner), out var mailbox))
                mailbox = new List<TEvent>();

            owners[typeof(TOwner)] = new List<TEvent>();

            return (List<TEvent>)mailbox;
        }


        public static void Register<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            handlers[typeof(TEvent)] = handlers.TryGetValue(typeof(TEvent), out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public static void Register<TEvent>(Action handler) where TEvent : IEvent
        {
            handlersNoArgs[typeof(TEvent)] = handlers.TryGetValue(typeof(TEvent), out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public static void Deregister<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            if (!handlers.TryGetValue(typeof(TEvent), out var existing))
                return;

            var result = Delegate.Remove(existing, handler);

            if (result == null)
                handlers.Remove(typeof(TEvent));

            else handlers[typeof(TEvent)] = result;
        }

        public static void Deregister<TEvent>(Action handler) where TEvent : IEvent
        {
            if (!handlersNoArgs.TryGetValue(typeof(TEvent), out var existing)) 
                return;
            var result = Delegate.Remove(existing, handler);

            if (result == null)
                handlers.Remove(typeof(TEvent));

            else handlersNoArgs[typeof(TEvent)] = result;
        }

        public static void Push<TEvent>(TEvent message) where TEvent : IEvent
        {
            if (handlers.TryGetValue(typeof(TEvent), out var existing))
                ((Action<TEvent>)existing)?.Invoke(message);
        }        

        public static void Push<TEvent>() where TEvent : IEvent
        {
            if (handlersNoArgs.TryGetValue(typeof(TEvent), out var existing))
                ((Action)existing)?.Invoke();
        }
    }
}















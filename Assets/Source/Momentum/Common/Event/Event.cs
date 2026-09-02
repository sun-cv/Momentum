using System;
using System.Collections.Generic;



namespace Game.Common
{

    public interface IEvent {};

    public static class Event 
    {
        private static readonly Dictionary<Type, Dictionary<Type, object>> mailboxes = new();

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
    }
}















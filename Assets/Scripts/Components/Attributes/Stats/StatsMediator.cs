

using System;
using System.Collections.Generic;

namespace Momentum
{


    public class StatsMediator
    {
        readonly LinkedList<StatModifier> modifiers = new();
        public event EventHandler<Query> Queries;
        
        private WeakSubscriber<Tick> subscription;

        public StatsMediator() 
        {
            subscription = new WeakSubscriber<Tick>(GameTickBinding.Tick, Tick, this);
        }
        public void PerformQuery(object sender, Query query) => Queries?.Invoke(sender, query);

        public void AddModifier(StatModifier modifier) 
        {
            modifiers.AddLast(modifier);
            Queries += modifier.Handle;

            modifier.OnDispose += _ => 
            {
                RemoveModifier(modifier);
            };
        }

        public void RemoveModifier(StatModifier modifier)
        {
            modifiers.Remove(modifier);
            Queries -= modifier.Handle;
        }

        public void Tick()
        {
            var node = modifiers.First;
            while (node != null)
            {
                var nextNode = node.Next;

                if (node.Value.MarkedForRemoval)
                {
                    node.Value.Dispose();
                }

                node = nextNode;
            }
        }
    }

    public class Query
    {
        public readonly string stat;
        public float value;

        public Query(string stat, float value)
        {
            this.stat  = stat;
            this.value = value;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace Momentum
{
    public class GenericQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> queue = new();
        private readonly Dictionary<Type, int> queueLimit = new();
        private readonly int maxPerType;

        public GenericQueue(int maxPerType = 3)
        {
            this.maxPerType = maxPerType;
        }

        public void Enqueue(T command)
        {
            queueLimit.TryGetValue(command.GetType(), out var count);

            if (count >= maxPerType)
                return;

            queue.Enqueue(command);
            queueLimit[command.GetType()] = count + 1;
        }

        public T Peek() => queue.Count > 0 ? queue.Peek() : default;

        public T Dequeue()
        {
            if (queue.Count == 0)
                return default;

            var instance = queue.Dequeue();
            var type = instance.GetType();

            if (queueLimit.TryGetValue(type, out var count) && count > 0)
            {
                count--;
                if (count == 0) queueLimit.Remove(type);
                else queueLimit[type] = count;
            }

            return instance;
        }

        public void Clear()
        {
            queue.Clear();
            queueLimit.Clear();
        }

        public bool IsEmpty => queue.Count == 0;
        public int Count => queue.Count;


        public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => queue.GetEnumerator();
    }
}

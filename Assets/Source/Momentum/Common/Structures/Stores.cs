using System;
using System.Collections.Generic;
using System.Linq;



namespace Game.Common
{

    public class Store<TValue>
    {
        TValue[] values;

        public Store(int capacity = 10)
        {
            values = new TValue[capacity];
        }

        public void Add(int index, TValue component)
        {
            EnsureCapacity(index);

            this.values[index] = component;
        }

        public TValue View(int index)
        {
            return index < values.Length ? values[index] : default;
        }
        
        public ref TValue Reference(int index)
        {
            return ref values[index];
        }

        public void Remove(int index)
        {
            values[index] = default;
        }

        void EnsureCapacity(int index)
        {
            if (index < values.Length) 
                return;

            int newSize = Math.Max(values.Length * 2, index + 1);

            Array.Resize(ref values, newSize);
        }

        public void CopyFrom(Store<TValue> other)
        {
            if (values.Length < other.values.Length)
                Array.Resize(ref values, other.values.Length);

            Array.Copy(other.values, values, other.values.Length);
        }

        public int Count => values.Count();
    }    

   public class Store<TValue, TConstraint> where TValue : TConstraint
    {
        TValue[] values;

        public Store(int capacity = 10)
        {
            values = new TValue[capacity];
        }

        public void Add(int index, TValue value)
        {
            EnsureCapacity(index);

            this.values[index] = value;
        }

        public TValue View(int index)
        {
            return values[index];
        }
        
        public ref TValue Reference(int index)
        {
            return ref values[index];
        }

        public void Remove(int index)
        {
            values[index] = default;
        }

        void EnsureCapacity(int index)
        {
            if (index < values.Length) 
                return;

            int newSize = Math.Max(values.Length * 2, index + 1);

            Array.Resize(ref values, newSize);
        }

        public void CopyFrom(Store<TValue, TConstraint> other)
        {
            if (values.Length < other.values.Length)
                Array.Resize(ref values, other.values.Length);

            Array.Copy(other.values, values, other.values.Length);
        }
    }    

    public class TypedStore<TBase>
    {   
        private readonly Dictionary<Type, Dictionary<string, TBase>> stores = new();

        public void Register<TValue>(string id, TValue value) where TValue : TBase
        {
            if (!stores.TryGetValue(typeof(TValue), out var dictionary))
            {
                dictionary = new();
                stores[typeof(TValue)] = dictionary;
            }

            dictionary[id] = value;
        }

        public void Deregister<TValue>(string id) where TValue : TBase
        {
            if (!stores.TryGetValue(typeof(TValue), out var _))
                return;

            stores[typeof(TValue)].Remove(id);
        }

        public TValue Get<TValue>(string id) where TValue : TBase
        {
            if (!stores.TryGetValue(typeof(TValue), out var dictionary))
                throw new Exception($"[TypedStore] Registry<{typeof(TBase)}> missing <{typeof(TValue)}>key: {id}");

            return (TValue)dictionary[id];
        }
    }
}

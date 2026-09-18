using System;
using System.Collections.Generic;



namespace Game.Common
{

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
    }    

    public class TypedStore<TValue>
    {   
        private readonly Dictionary<Type, Dictionary<string, TValue>> stores = new();

        public void Register<TConstraint>(string id, TConstraint value) where TConstraint : TValue
        {
            if (!stores.TryGetValue(typeof(TConstraint), out var dictionary))
            {
                dictionary = new();
                stores[typeof(TConstraint)] = dictionary;
            }

            dictionary[id] = value;
        }

        public void Deregister<TType>(string id) where TType : TValue
        {
            if (!stores.TryGetValue(typeof(TType), out var _))
                return;

            stores[typeof(TType)].Remove(id);
        }

        public TType Get<TType>(string id) where TType : TValue
        {
            if (!stores.TryGetValue(typeof(TValue), out var dictionary))
                throw new Exception($"[TypedStore] Registry<{typeof(TValue)}> missing <{typeof(TValue)}>key: {id}");

            return (TType)dictionary[id];
        }
    }
}

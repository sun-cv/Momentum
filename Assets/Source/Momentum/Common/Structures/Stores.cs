using System;
using System.Collections.Generic;
using Game.Common;



namespace Game.Realm
{

    public class Store<TValue, TBase> where TValue : TBase
    {
        TValue[]    component;

        public Store(int capacity = 10)
        {
            component   = new TValue[capacity];
        }

        public void Add(Entity entity, TValue component)
        {
            EnsureCapacity(entity.Index);

            this.component[entity.Index] = component;
        }

        public TValue View(Entity entity)
        {
            return component[entity.Index];
        }
        
        public ref TValue Reference(Entity entity)
        {
            return ref component[entity.Index];
        }

        public void Remove(Entity entity)
        {
            component[entity.Index] = default;
        }

        void EnsureCapacity(int index)
        {
            if (index < component.Length) 
                return;

            int newSize = Math.Max(component.Length * 2, index + 1);

            Array.Resize(ref component, newSize);
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
                throw new KeyNotFoundException($"Registry<{typeof(TBase)}> missing <{typeof(TValue)}>key: {id}");

            return (TValue)dictionary[id];
        }
    }
}

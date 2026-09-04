using System;
using System.Collections.Generic;
using Game.Common;



namespace Game.Realm
{

    public class Components 
    {
        private readonly Dictionary<Type, object> stores;

        public ComponentStore<TComponent> Store<TComponent>() where TComponent : struct
        {
            return (ComponentStore<TComponent>)stores[typeof(TComponent)];
        }

        internal void Register<T>(ComponentStore<T> store) where T : struct
        {
            stores[typeof(T)] = store;
        }
    }

    public readonly struct ComponentModifier
    {
        readonly Components components;

        internal ComponentModifier(Components components)
        {
            this.components = components;
        }

        public ref Health Health(Entity entity) => ref this.components.Store<Health>().Modify(entity);
    }

    public class ComponentStore<TComponent> where TComponent : struct
    {
        TComponent[] data;
        bool[] has;

        public ComponentStore(int capacity = 10)
        {
            data = new TComponent[capacity];
            has  = new bool[capacity];
        }

        public void Add(Entity entity, TComponent component)
        {
            EnsureCapacity(entity.Index);
            data[entity.Index] = component;
            has[entity.Index]  = true;
        }

        public TComponent View(Entity entity)
        {
            return data[entity.Index];
        }
        
        public ref TComponent Modify(Entity entity)
        {
            return ref data[entity.Index];
        }

        public bool Has(Entity entity)
        {
            return entity.Index < has.Length && has[entity.Index];
        }

        public void Remove(Entity entity)
        {
            if (entity.Index < has.Length) has[entity.Index] = false;
        }

        void EnsureCapacity(int index)
        {
            if (index < data.Length) return;
            int newSize = Math.Max(data.Length * 2, index + 1);
            Array.Resize(ref data, newSize);
            Array.Resize(ref has, newSize);
        }
    }
}




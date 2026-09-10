using System;
using System.Collections.Generic;
using Game.Common;
using Game.Content;



namespace Game.Realm
{

    public class Components 
    { 
        private int capacity = Config.World.Component.Capacity;

        private Modifier modify;

        private readonly Dictionary<Type, object> stores;
        private readonly Dictionary<Mask, HashSet<Entity>> cache;

        private Mask[] masks;
        private Entity[] identity;

        public Components()
        {
            stores      = new();
            cache       = new();
            masks       = new Mask[capacity];
            identity    = new Entity[capacity];
            modify      = new(this);
        }

        internal void Register<T>(Store<T> store) where T : IComponent
        {
            stores[typeof(T)] = store;
        }
        
        internal Store<TComponent> Access<TComponent>() where TComponent : IComponent
        {
            if (!stores.ContainsKey(typeof(TComponent)))
                Register<TComponent>(new());

            return (Store<TComponent>)stores[typeof(TComponent)];
        }

        internal TComponent View<TComponent>(Entity entity) where TComponent : IComponent
        {
            return Access<TComponent>().View(entity);
        }

        public void Add<TComponent>(Entity entity, TComponent component) where TComponent : IComponent
        {
            EnsureCapacity(entity.Index);
            Access<TComponent>().Add(entity, component);
            OnChanged(entity);

            masks   [entity.Index] = masks   [entity.Index].With<TComponent>();
            identity[entity.Index] = entity;
        }

        public void Remove<TComponent>(Entity entity) where TComponent : IComponent
        {
            EnsureCapacity(entity.Index);
            Access<TComponent>().Remove(entity);
            OnChanged(entity);

            masks[entity.Index] = masks[entity.Index].Without<TComponent>();
        }

        internal void Clear(Entity entity)
        {
            if (entity.Index < masks.Length)
                masks[entity.Index] = default;

            OnChanged(entity);
        }

        internal IReadOnlyCollection<Entity> Query(Mask mask)
        {
            if (cache.TryGetValue(mask, out var set))
                return set;

            set = new HashSet<Entity>(new EntityIdentityComparer());

            foreach (var entity in identity)
            {
                var current = entity.Index < masks.Length ? masks[entity.Index] : default;

                if (current.Contains(mask))
                    set.Add(entity);
            }
                    
            cache[mask] = set;
            return set;
        }

        private void OnChanged(Entity entity)
        {
            var current = entity.Index < masks.Length ? masks[entity.Index] : default;

            foreach (var (mask, set) in cache)
            {
                if (current.Contains(mask)) set.Add(entity);
                else                        set.Remove(entity);
            }
        }

        private void EnsureCapacity(int index)
        {
            if (index < capacity)
                return; 

            capacity = Math.Max(capacity * 2, index + 1);

            Array.Resize(ref masks,    capacity);
            Array.Resize(ref identity, capacity);
        }

        public Modifier Modify => modify;
         
        public sealed class Modifier
        {
            readonly Components components;

            internal Modifier(Components components)
            {
                this.components = components;
            }
            
            public ref Health Health(Entity entity) => ref components.Access<Health>().Modify(entity);
            public ref Energy Energy(Entity entity) => ref components.Access<Energy>().Modify(entity);
        }
    }
}


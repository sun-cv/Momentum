using System;
using System.Collections.Generic;
using Game.Common;
using Game.Data;



namespace Game.Realm
{

    public class Components 
    { 
        private int capacity = Config.World.Component.Capacity;

        private readonly Dictionary<Type, object> stores;
        private readonly Dictionary<ComponentMask, HashSet<Entity>> cache;

        private ComponentMask[] masks;
        private Entity[] identity;

        public Components()
        {
            stores      = new();
            cache       = new();
            masks       = new ComponentMask[capacity];
            identity    = new Entity[capacity];

        }

        internal void Register<T>(ComponentStore<T> store) where T : IComponent
        {
            stores[typeof(T)] = store;
        }
        
        internal ComponentStore<TComponent> Access<TComponent>() where TComponent : IComponent
        {
            if (!stores.ContainsKey(typeof(TComponent)))
                Register<TComponent>(new());

            return (ComponentStore<TComponent>)stores[typeof(TComponent)];
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

        internal IReadOnlyCollection<Entity> Query(ComponentMask mask)
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

    }
}


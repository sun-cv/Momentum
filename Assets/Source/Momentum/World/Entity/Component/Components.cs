using System;
using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;



namespace Game.Realm
{

    public partial class Components 
    { 
        private int capacity = Config.World.Component.Capacity;

        private readonly Modifier modify;

        private readonly Dictionary<Type, object> stores;
        private readonly Dictionary<Mask, HashSet<Entity>> cache;

        private Mask[]      masks;
        private Entity[]    identity;

        public Components()
        {
            stores      = new();
            cache       = new();
            masks       = new Mask  [capacity];
            identity    = new Entity[capacity];
            modify      = new(this);
        }

        public void Add<TComponent>(Entity entity, TComponent component) where TComponent : IComponent
        {
            EnsureCapacity(entity.Index);
            Access<TComponent>().Add(entity, component);

            masks   [entity.Index] = masks[entity.Index].With<TComponent>();
            identity[entity.Index] = entity;

            OnComponentChange(entity);
        }

        public void Remove<TComponent>(Entity entity) where TComponent : IComponent
        {
            EnsureCapacity(entity.Index);
            Access<TComponent>().Remove(entity);

            masks[entity.Index] = masks[entity.Index].Without<TComponent>();

            OnComponentChange(entity);
        }

        internal TComponent View<TComponent>(Entity entity) where TComponent : IComponent
        {
            return Access<TComponent>().View(entity);
        }

        internal Store<TComponent> Access<TComponent>() where TComponent : IComponent
        {
            if (!stores.ContainsKey(typeof(TComponent)))
                stores[typeof(TComponent)] = new Store<TComponent>();

            return (Store<TComponent>)stores[typeof(TComponent)];
        }

        internal void Clear(Entity entity)
        {
            if (entity.Index < masks.Length)
                masks[entity.Index] = default;

            OnComponentChange(entity);
        }

        internal IReadOnlyCollection<Entity> Query(Mask mask)
        {
            if (cache.TryGetValue(mask, out var set))
                return set;

            set = new HashSet<Entity>(new EntityIdentityComparer());

            foreach (var entity in identity)
            {
                if (CurrentMask(entity).Contains(mask))
                    set.Add(entity);
            }

            cache[mask] = set;
            return set;
        }

        private void OnComponentChange(Entity entity)
        {
            var current = CurrentMask(entity);

            foreach (var (mask, set) in cache)
            {
                if (current.Contains(mask)) set.Add(entity);
                else                        set.Remove(entity);
            }
        }

        private Mask CurrentMask(Entity entity)
        {
            return entity.Index < masks.Length ? masks[entity.Index] : default;
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
         
        static Components() => Log<Components>.Level(Diagnostic.Log.Level.Debug);
    }


}


using System;
using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;



namespace Game.Realm
{

    public partial class Components
    {
        private readonly Pool pool;
        private readonly Modifier modify;
        private readonly MaskSet<Components> masks;
        private readonly Dictionary<Type, object> stores;

        public Components(Pool pool, MaskSet<Components> masks)
        {
            this.pool   = pool;
            this.masks  = masks;
            stores      = new();
            modify      = new(this);
        }

        public void Add<TComponent>(Entity entity, TComponent component) where TComponent : IComponent
        {
            Guard(entity);

            Access<TComponent>().Add(entity.Index, component);
            masks.Set(entity, masks.View(entity).With<TComponent>());
        }

        public bool Has<TComponent>(Entity entity) where TComponent : IComponent
        {
            return pool.Alive(entity) && masks.View(entity).Contains(Mask<Components, TComponent>.Key);
        }

        public void Remove<TComponent>(Entity entity) where TComponent : IComponent
        {
            Guard(entity);

            Access<TComponent>().Remove(entity.Index);
            masks.Set(entity, masks.View(entity).Without<TComponent>());
        }

        internal TComponent View<TComponent>(Entity entity) where TComponent : IComponent
        {
            Guard(entity);

            return Access<TComponent>().View(entity.Index);
        }

        internal ref TComponent Reference<TComponent>(Entity entity) where TComponent : IComponent
        {
            Guard(entity);

            return ref Access<TComponent>().Reference(entity.Index);
        }

        internal void Clear(Entity entity)
        {
            masks.Clear(entity);
        }

        private Store<TComponent> Access<TComponent>() where TComponent : IComponent
        {
            if (!stores.TryGetValue(typeof(TComponent), out var store))
            {
                store = new Store<TComponent>();
                stores[typeof(TComponent)] = store;
            }

            return (Store<TComponent>)store;
        }

        private void Guard(Entity entity)
        {
            if (!pool.Alive(entity))
                throw new Exception($"[Components] Stale entity {entity.Index}:{entity.Generation}");
        }

        public Modifier Modify => modify;

        static Components() => Log<Components>.Level(Diagnostic.Log.Level.Debug);
    }
}


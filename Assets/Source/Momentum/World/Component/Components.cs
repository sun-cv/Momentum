using System;
using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;



namespace Game.Realm
{

    public partial class Components
    {
        private readonly Pool Pool;
        private readonly Masks Mask;
        private readonly Modifier modify;
        private readonly Dictionary<Type, object> stores;

        internal Components(Masks masks, Pool pool)
        {
            Pool        = pool;
            Mask        = masks;
            stores      = new();
            modify      = new(this);
        }

        public void Add<TComponent>(Entity entity, TComponent component) where TComponent : IComponent
        {
            Guard(entity);

            Access<TComponent>().Add(entity.Index, component);
            Mask.Get<Components>().Set(entity, Mask.Get<Components>().View(entity).With<TComponent>());
        }

        public void Remove<TComponent>(Entity entity) where TComponent : IComponent
        {
            Guard(entity);

            Access<TComponent>().Remove(entity.Index);
            Mask.Get<Components>().Set(entity, Mask.Get<Components>().View(entity).Without<TComponent>());
        }

        internal bool Has<TComponent>(Entity entity) where TComponent : IComponent
        {
            return Pool.Alive(entity) && Mask.Get<Components>().View(entity).Contains(Mask<Components, TComponent>.Key);
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
            if (!Pool.Alive(entity))
                throw new Exception($"[Components] Stale entity {entity.Index}:{entity.Generation}");
        }

        internal Modifier Modify => modify;

        static Components() => Log<Components>.Level(Diagnostic.Log.Level.Debug);
    }
}


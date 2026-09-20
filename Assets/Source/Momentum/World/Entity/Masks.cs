using System;
using System.Collections.Generic;
using Game.Common;



namespace Game.Realm
{
    public class Masks
    {
        private readonly Pool pool;
        private readonly Dictionary<Type, object> sets = new();

        public Masks(Pool pool)
        {
            this.pool = pool;
        }

        public void Register<TDomain>()
        {
            if (sets.ContainsKey(typeof(TDomain)))
                return;

            sets[typeof(TDomain)] = new MaskSet<TDomain>(pool);
        }

        public MaskSet<TDomain> Get<TDomain>()
        {
            return (MaskSet<TDomain>)sets[typeof(TDomain)];
        }

        public IReadOnlyCollection<Entity> Query<TDomain>(Mask<TDomain> mask)
        {
            return Get<TDomain>().Query(mask);
        }
    }

    public class MaskSet<TDomain>
    {
        private readonly Pool pool;
        private readonly Store<Mask<TDomain>> store = new();
        private readonly Dictionary<Mask<TDomain>, HashSet<Entity>> cache = new();

        public MaskSet(Pool pool)
        {
            this.pool = pool;
        }

        public Mask<TDomain> View(Entity entity)
        {
            return store.View(entity.Index);
        }

        public void Set(Entity entity, Mask<TDomain> mask)
        {
            store.Add(entity.Index, mask);
            Patch(entity, mask);
        }

        public void Clear(Entity entity)
        {
            Set(entity, default);
        }

        public IReadOnlyCollection<Entity> Query(Mask<TDomain> mask)
        {
            if (cache.TryGetValue(mask, out var set))
                return set;

            set = new HashSet<Entity>(new EntityIdentityComparer());

            foreach (var entity in pool.Enumerate())
            {
                if (View(entity).Contains(mask))
                    set.Add(entity);
            }

            cache[mask] = set;
            return set;
        }

        private void Patch(Entity entity, Mask<TDomain> current)
        {
            foreach (var (mask, set) in cache)
            {
                if (current.Contains(mask)) set.Add(entity);
                else                        set.Remove(entity);
            }
        }
    }
}

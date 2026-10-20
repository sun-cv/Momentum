using System;

using Game.Common;
using Game.Realm;



namespace Game.Service
{

    public class StatSystem : RegisteredService, IWorld, IGameBase
    {


        private readonly World World;

        public StatSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            Reset();
            Collect();
            Resolve();
        }

        private void Reset()
        {
            foreach (var owner in World.Query(Mask<Components, Stats, Modifiers>.Key))
            {
                World.Entity.Modify.Modifiers(owner) = default;
            }
        }

        private void Collect()
        {
            foreach (var source in World.Query(Mask<Components, Modifiers, Parent>.Key))
            {
                if (World.Entity.Has<Stats>(source))
                    continue;

                var parent = World.Entity.Parent(source).Entity;

                if (!World.Entity.Has<Modifiers>(parent))
                    continue;

                var from    = World.Entity.Modifiers(source);
                ref var to  = ref World.Entity.Modify.Modifiers(parent);

                Combine(ref to.Health,     from.Health);
                Combine(ref to.Armor,      from.Armor);
                Combine(ref to.Energy,     from.Energy);
                Combine(ref to.Damage,     from.Damage);
                Combine(ref to.CritChance, from.CritChance);
                Combine(ref to.CritDamage, from.CritDamage);
                Combine(ref to.Speed,      from.Speed);
            }
        }

        private void Resolve()
        {
            foreach (var owner in World.Query(Mask<Components, Stats, Modifiers>.Key))
            {
                var total       = World.Entity.Modifiers(owner);
                ref var stats   = ref World.Entity.Modify.Stats(owner);

                ApplyWhole  (ref stats.Health,     total.Health);
                ApplyWhole  (ref stats.Armor,      total.Armor);
                ApplyWhole  (ref stats.Energy,     total.Energy);
                ApplyWhole  (ref stats.Damage,     total.Damage);
                Apply       (ref stats.CritChance, total.CritChance);
                Apply       (ref stats.CritDamage, total.CritDamage);
                Apply       (ref stats.Speed,      total.Speed);
            }
        }

        private static void Combine(ref Modifier target, in Modifier source)
        {
            target.Flat    += source.Flat;
            target.Positive = Math.Max(target.Positive, source.Positive);
            target.Negative = Math.Min(target.Negative, source.Negative);
        }

        private static void Apply(ref Stat stat, in Modifier total)
        {
            stat.Final = (stat.Base + total.Flat) * (1f + total.Negative) * (1f + total.Positive);
        }

        private static void ApplyWhole(ref Stat stat, in Modifier total)
        {
            stat.Final = MathF.Ceiling((stat.Base + total.Flat) * (1f + total.Negative) * (1f + total.Positive));
        }
    }
}

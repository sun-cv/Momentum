


using Game.Common;
using Game.Diagnostic;
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
            Process();
        }

        private void Process()
        {
            Reset();

            Collect(Mask<Components, Item, Equipped, Stats, Parent>.Key);
            Collect(Mask<Components, Talent, Stats, Parent>.Key);
            Collect(Mask<Components, Buff, Stats, Parent>.Key);
            Collect(Mask<Components, Debuff, Stats, Parent>.Key);

            Apply();
        }

        private void Reset()
        {
            foreach (var actor in World.Query(Mask<Components, Actor, Stats>.Key))
            {
                StatResolver.Each(ref World.Entity.Modify.Stats(actor), StatResolver.Reset);
            }
        }

        private void Collect(Mask<Components> key)
        {
            foreach (var source in World.Query(key))
            {
                var sourceStats     = World.Entity.Stats(source);
                ref var ownerStats  = ref World.Entity.Modify.Stats(World.Entity.Parent(source).Entity);

                StatResolver.Each(ref ownerStats, in sourceStats, StatResolver.Combine);
            }
        }

        private void Apply()
        {
            foreach (var actor in World.Query(Mask<Components, Actor, Stats>.Key))
            {
                ref var stats = ref World.Entity.Modify.Stats(actor);
                StatResolver.Each(ref stats, StatResolver.Apply);
            }
        }

        static StatSystem() => Log<StatSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}

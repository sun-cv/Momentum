using Game.Realm;
using Game.Common;
using System.Collections.Generic;



namespace Game.Service
{

    public class BoundSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        public BoundSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            Process();
        }

        private void Process()
        {
            List<Entity> release = new();

            foreach ( var entity in World.Query(Mask<Components, Bound>.Key))
            {
                if (World.Entity.Alive(World.Entity.Bound(entity).Entity))
                    continue;

                release.Add(entity);
            }
            foreach (var entity in release)
            {
                World.Entity.Release(entity);
            }
        }
    }

}

using System;
using System.Collections.Generic;

using Game.Realm;
using Game.Common;



namespace Game.Service
{

    public class DurationSystem : RegisteredService, IWorld, IGameBase
    {

        private readonly World World;
        private readonly List<Entity> entities = new();


        public DurationSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            Process();
        }

        private void Process()
        {
            foreach (var entity in World.Query(Mask<Components, Duration>.Key))
            {
                TickDuration(entity);
                CheckExpiration(entity);
            }

            ReleaseExpired();
        }

        private void TickDuration(Entity entity)
        {
            World.Entity.Modify.Duration(entity).Elapsed++;
        }

        private void CheckExpiration(Entity entity)
        {
            var duration = World.Entity.Duration(entity);

            if (duration.Elapsed >= duration.Length)
            {
                entities.Add(entity);
            }
        }
        
        private void ReleaseExpired()
        {
            foreach(var entity in entities)
            {
                if (!World.Entity.Alive(entity))
                    continue;
                
                World.Entity.Release(entity);
            }

            entities.Clear();
        }

    }
}

using UnityEngine;

using Game.Realm;
using Game.Common;
using Game.Diagnostic;



namespace Game.Service
{
    public class HeadingSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        public HeadingSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            ProcessHeadings();
        }

        private void ProcessHeadings()
        {
            foreach (var entity in World.Query(Mask<Components, Heading, Displacement, Parent>.Key))
            {
                var parent = World.Entity.Parent(entity).Entity;

                if (World.Entity.Displacement(entity).Direction == Vector2.zero)
                {
                    Start(entity, parent);
                    continue;
                }

                Steer(entity, parent);
            }
        }

        private void Start(Entity entity, Entity parent)
        {
            var direction = World.Entity.Heading(entity).Toward == Heading.Source.Aim
                ? World.Entity.Aim(entity).Direction
                : World.Entity.Intent(parent).Direction;

            if (direction == Vector2.zero)
            {
                direction = World.Entity.Facing(parent).Direction;
            }

            World.Entity.Modify.Displacement(entity).Direction = direction.normalized;
        }

        private void Steer(Entity entity, Entity parent)
        {
            var heading = World.Entity.Heading(entity);

            if (heading.SteerRate <= 0f)
                return;

            var target = heading.Toward == Heading.Source.Aim
                ? World.Entity.Aim(parent).Direction
                : World.Entity.Intent(parent).Direction;

            if (target == Vector2.zero)
                return;

            var scale   = World.Entity.TimeScale(parent).Scale;
            var radians = heading.SteerRate * Mathf.Deg2Rad * scale;

            ref var displacement   = ref World.Entity.Modify.Displacement(entity);
            displacement.Direction = Vector3.RotateTowards(displacement.Direction, target, radians, 0f);
        }

        static HeadingSystem() => Log<HeadingSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}

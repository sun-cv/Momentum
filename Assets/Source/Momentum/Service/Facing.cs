using UnityEngine;

using Game.Common;
using Game.Realm;



namespace Game.Service
{

    public class FacingSystem : RegisteredService, IWorld, IGameBase
    {

        private readonly World World;

        public FacingSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            foreach (var entity in World.Query(Mask<Components, Facing, Control>.Key))
                FaceVelocity(entity);

            foreach (var ability in World.Query(Mask<Components, Ability, Aim, Parent>.Key))
                FaceAim(ability);
        }

        private void FaceVelocity(Entity entity)
        {
            var velocity    = World.Entity.Control(entity).Velocity;
            ref var facing  = ref World.Entity.Modify.Facing(entity);

            if (velocity.sqrMagnitude < Config.Graphics.Facing.MinimumSpeed)
                return;

            var target = Orientation.Cardinal(velocity);

            if (target == facing.Direction)
            {
                facing.Ticks = 0;
                return;
            }

            if (++facing.Ticks < Config.Graphics.Facing.TurnDelay)
                return;

            facing.Direction = target;
            facing.Ticks     = 0;
        }

        private void FaceAim(Entity ability)
        {
            var aim = World.Entity.Aim(ability).Direction;

            if (aim == Vector2.zero)
                return;

            ref var facing   = ref World.Entity.Modify.Facing(World.Entity.Parent(ability).Entity);
            facing.Direction = Orientation.Cardinal(aim);
            facing.Ticks     = 0;
        }
    }
}


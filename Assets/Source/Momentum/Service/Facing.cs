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
            var velocity    = World.Entity.Intent(entity).Direction;
            ref var facing  = ref World.Entity.Modify.Facing(entity);

            if (velocity.sqrMagnitude < Config.Facing.MinimumSpeed * Config.Facing.MinimumSpeed)
                return;

            var direction = velocity.normalized;

            if (Mathf.Abs(direction.y) < Config.Facing.Axis)
            {
                facing.Direction = new Vector2(Mathf.Sign(direction.x), 0f);
                facing.Ticks     = 0;
                return;
            }

            if (Mathf.Abs(direction.x) < Config.Facing.Axis)
            {
                facing.Direction = new Vector2(0f, Mathf.Sign(direction.y));
                facing.Ticks     = 0;
                return;
            }

            if (++facing.Ticks < TurnDelay(facing.Direction, direction))
                return;

            facing.Direction = new Vector2(Mathf.Sign(direction.x), 0f);
        }

        private int TurnDelay(Vector2 current, Vector2 target)
        {
            var clockwise = current.y > 0f ? target.x > 0f
                : current.y < 0f ? target.x < 0f
                : current.x > 0f ? target.y < 0f
                :                  target.y > 0f;

            return clockwise ? Config.Facing.Clockwise : Config.Facing.Counterclockwise;
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


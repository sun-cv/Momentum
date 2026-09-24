using UnityEngine;

using Game.Realm;
using Game.Common;



namespace Game.Service
{
    public class ImpulseSystem : RegisteredService, IWorld, IGameBase
    {
        private const float Rest = Config.World.Physics.Rest;

        private readonly World World;

        public ImpulseSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            Decay();
        }

        private void Decay()
        {
            foreach (var entity in World.Query(Mask<Components, Impulse, Mass>.Key))
            {
                ref var impulse = ref World.Entity.Modify.Impulse(entity);
                var friction    = World.Entity.Mass(entity).Friction;
                var scale       = World.Entity.TimeScale(entity).Scale;

                impulse.Velocity *= Mathf.Exp(-friction * Watch.Tick.Delta * scale);

                if (impulse.Velocity.sqrMagnitude < Rest * Rest)
                {
                    impulse.Velocity = Vector2.zero;
                }
            }
        }
    }
}


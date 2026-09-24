using Game.Realm;
using Game.Common;



namespace Game.Service
{
    public class ResolveSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        public ResolveSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            Resolve();
        }

        private void Resolve()
        {
            foreach (var entity in World.Query(Mask<Components, Velocity, Control, Kinematic, Impulse>.Key))
            {
                var scale       = World.Entity.TimeScale(entity).Scale;
                var control     = World.Entity.Control(entity).Velocity;
                var impulse     = World.Entity.Impulse(entity).Velocity;
                var kinematic   = World.Entity.Kinematic(entity).Velocity;

                World.Entity.Modify.Velocity(entity).Value = (control + kinematic + impulse) * scale;
            }
        }
    }
}


using UnityEngine;

using Game.Realm;
using Game.Common;
using Game.Diagnostic;



namespace Game.Service
{
    public class ControlSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        public ControlSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            CalculateVelocity();
        }

        private void CalculateVelocity()
        {
            var movers = World.Query(Mask<Components, Control, Mass, Intent, Speed, Acceleration>.Key);

            foreach (var entity in movers)
            {
                CalculateControlVelocity(entity);
            }
        }

        private void CalculateControlVelocity(Entity entity)
        {
            var intent          = World.Entity.Intent(entity);
            var speed           = World.Entity.Speed(entity);
            var acceleration    = World.Entity.Acceleration(entity);
            var control         = World.Entity.Control(entity);
            var scale           = World.Entity.TimeScale(entity).Scale;

            var direction       = Vector2.ClampMagnitude(intent.Direction, 1f);
            var step            = acceleration.Current * Watch.Tick.Delta * scale;
            var target          = World.Entity.CanMove(entity) ? direction * speed.Current : Vector2.zero;

            World.Entity.Modify.Control(entity).Velocity = Vector2.MoveTowards(control.Velocity, target, step);
        }

        static ControlSystem() => Log<ControlSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}


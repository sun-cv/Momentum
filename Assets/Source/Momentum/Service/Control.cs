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
            var movers = World.Query(Mask<Components, Control, Mass, Intent, Movement>.Key);

            foreach (var entity in movers)
            {
                ClearControlModifier(entity);
            }

            CalculateControlModifiers();

            foreach (var entity in movers)
            {
                CalculateControlVelocity(entity);
            }
        }

        private void ClearControlModifier(Entity entity)
        {
            World.Entity.Modify.Control(entity).Modifier.Reset();
        }

        private void CalculateControlModifiers()
        {
            foreach (var modifier in World.Query(Mask<Components, SpeedModifier, Parent>.Key))
            {
                World.Entity.Modify.Control(World.Entity.Parent(modifier).Entity).Modifier.Fold(World.Entity.SpeedModifier(modifier).Value);
            }
        }

        private void CalculateControlVelocity(Entity entity)
        {
            var intent      = World.Entity.Intent(entity);
            var movement    = World.Entity.Movement(entity);
            var control     = World.Entity.Control(entity);
            var scale       = World.Entity.TimeScale(entity).Scale;

            var direction   = Vector2.ClampMagnitude(intent.Direction, 1f);
            var target      = World.Entity.CanMove(entity)
                ? direction * movement.Speed * control.Modifier.Value
                : Vector2.zero;
            var step        = movement.Acceleration * Watch.Tick.Delta * scale;


            World.Entity.Modify.Control(entity).Velocity = Vector2.MoveTowards(control.Velocity, target, step);
        }

        static ControlSystem() => Log<ControlSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}


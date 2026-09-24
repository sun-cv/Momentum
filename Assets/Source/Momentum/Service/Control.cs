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
            var control = World.Query<Components>(Mask<Components, Control, Mass, Intent, Movement>.Key);

            foreach(var entity in control)
            {
                CalculateControlModifier(entity);
                CalculateControlVelocity(entity);
            }
        }

        private void CalculateControlModifier(Entity entity)
        {
            float slowest = 0f;
            float fastest = 0f;

            if (World.Entity.Has<Child>(entity))
            {
                foreach (var child in World.Entity.Child(entity).Entities)
                {
                    if (World.Entity.Has<SpeedModifier>(child))
                    {
                        var value = World.Entity.SpeedModifier(child).Value;

                        slowest = Mathf.Min(slowest, value);
                        fastest = Mathf.Max(fastest, value);
                    }
                }
            }

            World.Entity.Modify.Control(entity).Modifier = (1f + slowest) * (1f + fastest);
        }

        private void CalculateControlVelocity(Entity entity)
        {
            var intent      = World.Entity.Intent(entity);
            var movement    = World.Entity.Movement(entity);
            var control     = World.Entity.Control(entity);
            var scale       = World.Entity.TimeScale(entity).Scale;

            var direction   = Vector2.ClampMagnitude(intent.Direction, 1f);
            var target      = World.Entity.CanMove(entity)
                ? direction * movement.Speed * control.Modifier
                : Vector2.zero;
            var step        = movement.Acceleration * Watch.Tick.Delta * scale;


            World.Entity.Modify.Control(entity).Velocity = Vector2.MoveTowards(control.Velocity, target, step);
        }

        static ControlSystem() => Log<ControlSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}


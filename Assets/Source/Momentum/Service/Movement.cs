using UnityEngine;
using System.Collections.Generic;

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

    public class DirectiveSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        public DirectiveSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            ResetKinematic();
            ProcessDisplacement();
        }

        private void ResetKinematic()
        {
            foreach (var entity in World.Query(Mask<Components, Kinematic>.Key))
            {
                World.Entity.Modify.Kinematic(entity).Velocity = Vector2.zero;
            }
        }

        private void ProcessDisplacement()
        {
            var finished = new List<Entity>();

            foreach (var entity in World.Query(Mask<Components, Displacement, Parent>.Key))
            {
                var parent = World.Entity.Parent(entity).Entity;

                Steer(entity, parent);

                if (Advance(entity, parent))
                {
                    finished.Add(entity);
                }
            }

            foreach (var entity in finished)
            {
                World.Entity.Release(entity);
            }
        }

        private void Steer(Entity entity, Entity parent)
        {
            var displacement = World.Entity.Displacement(entity);

            if (displacement.SteerRate <= 0f)
                return;

            var intent = World.Entity.Intent(parent).Direction;

            if (intent == Vector2.zero)
                return;

            var radians = displacement.SteerRate * Mathf.Deg2Rad;

            World.Entity.Modify.Displacement(entity).Direction = Vector3.RotateTowards(displacement.Direction, intent, radians, 0f);
        }

        private bool Advance(Entity entity, Entity parent)
        {
            ref var displacement    = ref World.Entity.Modify.Displacement(entity);
            var scale               = World.Entity.TimeScale(parent).Scale;

            if (scale <= 0f)
                return false;

            var before = Shape((float)displacement.Progress / displacement.Duration);
            displacement.Progress++;
            var after  = Shape((float)displacement.Progress / displacement.Duration);

            World.Entity.Modify.Kinematic(parent).Velocity = displacement.Direction * (after - before) * displacement.Distance / (Watch.Tick.Delta * scale);

            return displacement.Progress >= displacement.Duration;
        }

        private float Shape(float t) => t;

        static DirectiveSystem() => Log<DirectiveSystem>.Level(Diagnostic.Log.Level.Debug);
    }

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

    public class MovementSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        public MovementSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            ApplyVelocity();
        }

        private void ApplyVelocity()
        {
            foreach (var entity in World.Query(Mask<Components, Velocity, Body>.Key))
            {
                World.Entity.Body(entity).Form.linearVelocity = World.Entity.Velocity(entity).Value;
            }
        }
    }

    public class SimulationSystem : RegisteredService, IGameBase
    {
        public SimulationSystem()
        {
            // Physics2D.simulationMode = SimulationMode2D.Script;
        }

        public void Tick()
        {
            // Physics2D.Simulate(Watch.Tick.Delta);
        }
    }
}

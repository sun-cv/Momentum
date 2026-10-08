using UnityEngine;
using System.Collections.Generic;

using Game.Realm;
using Game.Common;
using Game.Diagnostic;



namespace Game.Service
{
    public class DirectiveSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;

        private readonly List<Entity> finished = new();

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
            foreach (var entity in World.Query(Mask<Components, Displacement, Parent>.Key))
            {
                if (Advance(entity, World.Entity.Parent(entity).Entity))
                {
                    finished.Add(entity);
                }
            }

            foreach (var entity in finished)
            {
                World.Entity.Release(entity);
            }

            finished.Clear();
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

            World.Entity.Modify.Kinematic(parent).Velocity += displacement.Direction * (after - before) * displacement.Distance / (Watch.Tick.Delta * scale);

            return displacement.Progress >= displacement.Duration;
        }

        private float Shape(float t) => t;

        static DirectiveSystem() => Log<DirectiveSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}

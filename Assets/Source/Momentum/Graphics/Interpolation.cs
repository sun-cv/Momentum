using UnityEngine;

using Game.Realm;
using Game.Common;
using Game.Diagnostic;



namespace Game.Graphics
{

    public class InterpolationSystem : RegisteredService, IWorld, IRealLate
    {
        private readonly World World;

        public InterpolationSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            Interpolate();
        }

        private void Interpolate()
        {
            var alpha = Watch.Tick.Alpha;

            foreach (var entity in World.Query(Mask<Components, Visual>.Key))
            {
                var visual      = World.Entity.Visual(entity);
                var position    = Vector2.LerpUnclamped(visual.Previous, visual.Current, alpha);

                visual.Transform.position = new Vector3(position.x, position.y, visual.Transform.position.z);
            }
        }

        static InterpolationSystem() => Log<InterpolationSystem>.Level(Diagnostic.Log.Level.Debug);
    }
}

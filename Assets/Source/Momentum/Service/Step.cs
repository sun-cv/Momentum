using UnityEngine;

using Game.Realm;
using Game.Common;
using Game.Common.Events;

using Event = Game.Common.Event;



namespace Game.Service
{
    public class StepSystem : RegisteredService, IWorld, IGameBase
    {
        private readonly World World;
        private readonly Rigidbody2D.SlideMovement slide;

        public StepSystem(World world)
        {
            World = world;

            slide = new()
            {
                maxIterations       = 3,
                surfaceSlideAngle   = 90f,
                surfaceUp           = Vector2.up,
                surfaceAnchor       = Vector2.zero,
                gravity             = Vector2.zero,
                useSimulationMove   = false,
            };
        }

        public void Tick()
        {
            Step();
        }

        private void Step()
        {
            foreach (var entity in World.Query(Mask<Components, Velocity, Form>.Key))
            {
                var body        = World.Entity.Form(entity).Body;
                var result      = body.Slide(World.Entity.Velocity(entity).Value, Watch.Tick.Delta, slide);

                Report(entity, result.slideHit);
            }
        }

        private void Report(Entity entity, RaycastHit2D hit)
        {
            if (!hit)
                return;

            if (hit.rigidbody != null && World.Entity.Body.Owner(hit.rigidbody, out var target))
            {
                Event.Send(new BodyContacted { Source = entity, Target = target, Normal = hit.normal });
                return;
            }

            Event.Send(new SurfaceContacted { Source = entity, Normal = hit.normal });
        }
    }
}

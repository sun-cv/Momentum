using UnityEngine;

using Game.Realm;
using Game.Common;
using Game.Common.Events;

using Event = Game.Common.Event;



namespace Game.Service
{
    public class ContactSystem : RegisteredService, IWorld, IGameBase
    {
        private const float Knockback = Config.World.Physics.Knockback;

        private readonly World World;

        public ContactSystem(World world)
        {
            World = world;

            Event.Register<ContactSystem, BodyContacted>();
            Event.Register<ContactSystem, SurfaceContacted>();
        }

        public void Tick()
        {
            ResolveSurfaces();
            ResolveBodies();
        }

        private void ResolveSurfaces()
        {
            foreach (var contact in Event.Read<ContactSystem, SurfaceContacted>())
            {
                ref var impulse = ref World.Entity.Modify.Impulse(contact.Source);
                var into        = Vector2.Dot(impulse.Velocity, -contact.Normal);

                if (into > 0f)
                {
                    impulse.Velocity += contact.Normal * into;
                }
            }
        }

        private void ResolveBodies()
        {
            foreach (var contact in Event.Read<ContactSystem, BodyContacted>())
            {
                if (!World.Entity.Has<Mass>(contact.Target) || !World.Entity.CanYield(contact.Target))
                    continue;

                if (World.Entity.TimeScale(contact.Target).Scale <= 0f)
                    continue;

                var into    = -contact.Normal;
                var closing = Vector2.Dot(World.Entity.Velocity(contact.Source).Value, into) - Own(contact.Target, into);

                if (closing <= 0f)
                    continue;

                if (closing > Knockback)
                    Knock(contact.Source, contact.Target, into, closing);

                else Push(contact.Source, contact.Target, into);
            }
        }

        private void Knock(Entity source, Entity target, Vector2 into, float closing)
        {
            var scale       = World.Entity.TimeScale(target).Scale;
            var momentum    = World.Entity.Mass(source).Weight * closing;

            World.Entity.Modify.Impulse(target).Velocity += into * (momentum / World.Entity.Mass(target).Weight / scale);
        }

        private void Push(Entity source, Entity target, Vector2 into)
        {
            var scale       = World.Entity.TimeScale(target).Scale;
            var weight      = World.Entity.Mass(source).Weight;
            var resist      = World.Entity.Mass(target).Weight;
            var own         = Own(target, into);
            var shared      = (weight * Vector2.Dot(World.Entity.Velocity(source).Value, into) + resist * own) / (weight + resist);

            ref var impulse = ref World.Entity.Modify.Impulse(target);
            var current     = Vector2.Dot(impulse.Velocity, into);

            impulse.Velocity += into * ((shared - own) / scale - current);
        }

        private float Own(Entity entity, Vector2 into)
        {
            var own = World.Entity.Control(entity).Velocity + World.Entity.Kinematic(entity).Velocity;

            return Vector2.Dot(own, into) * World.Entity.TimeScale(entity).Scale;
        }
    }
}

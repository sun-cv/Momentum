using System;
using Game.Common;
using Game.Common.Events;
using Game.Realm;

using Event = Game.Common.Event;



namespace Game.Service
{

    public class HealthSystem : RegisteredService, IWorld, IGameBase
    {

        private readonly World World;

        public HealthSystem(World world)
        {
            World = world;

            Event.Register<HealthSystem, Heal>();
            Event.Register<HealthSystem, Wound>();
        }

        public void Tick()
        {
            ProcessHeals();
            ProcessWounds();     
        }

        private void ProcessHeals()
        {
            foreach (var heal in Event.Read<HealthSystem, Heal>())
            {
                Heal(heal.Entity, heal.Amount);
            }
        }

        private void ProcessWounds()
        {
            foreach (var wound in Event.Read<HealthSystem, Wound>())
            {
                Wound(wound.Entity, wound.Amount);
            }
        }

        private void Heal(Entity entity, int amount)
        {
            var before  = World.Entity.Health(entity);
            World.Entity.Modify.Health(entity).Current = Math.Min(before.Current + amount, before.Maximum); 
            var after   = World.Entity.Health(entity);

            Notify(entity, before.Current, after.Current);
        }

        private void Wound(Entity entity, int amount)
        {
            var before  = World.Entity.Health(entity);
            World.Entity.Modify.Health(entity).Current = Math.Max(before.Current - amount, 0); 
            var after   = World.Entity.Health(entity);

            Notify(entity, before.Current, after.Current);
        }

        private void Notify(Entity entity, int before, int after)
        {
            if (before == after)
                return;

            Event.Send<HealthChanged>(new() { Entity = entity, Before = before, After = after });
        }
    }
}

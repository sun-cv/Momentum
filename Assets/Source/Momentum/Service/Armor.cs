using System;
using Game.Common;
using Game.Common.Events;
using Game.Realm;

using Event = Game.Common.Event;



namespace Game.Service
{

    public class ArmorSystem : RegisteredService, IWorld, IGameBase
    {

        private readonly World World;

        public ArmorSystem(World world)
        {
            World = world;

            Event.Register<ArmorSystem, Repair>();
            Event.Register<ArmorSystem, Fracture>();
        }

        public void Tick()
        {
            ProcessRepairs();
            ProcessFractures();     
        }

        private void ProcessRepairs()
        {
            foreach (var repair in Event.Read<ArmorSystem, Repair>())
            {
                Repair(repair.Entity, repair.Amount);
            }
        }

        private void ProcessFractures()
        {
            foreach (var fracture in Event.Read<ArmorSystem, Fracture>())
            {
                Fracture(fracture.Entity, fracture.Amount);
            }
        }

        private void Repair(Entity entity, int amount)
        {
            var before  = World.Entity.Armor(entity);
            World.Entity.Modify.Armor(entity).Current = Math.Min(before.Current + amount, before.Maximum); 
            var after   = World.Entity.Armor(entity);

            Notify(entity, before.Current, after.Current);
        }

        private void Fracture(Entity entity, int amount)
        {
            var before  = World.Entity.Armor(entity);
            World.Entity.Modify.Armor(entity).Current = Math.Max(before.Current - amount, 0); 
            var after   = World.Entity.Armor(entity);

            Notify(entity, before.Current, after.Current);
        }

        private void Notify(Entity entity, int before, int after)
        {
            if (before == after)
                return;

            Event.Send<ArmorChanged>(new() { Entity = entity, Before = before, After = after });
        }
    }
}

using System;
using Game.Common;
using Game.Common.Events;
using Game.Realm;

using Event = Game.Common.Event;



namespace Game.Service
{

    public class EnergySystem : RegisteredService, IWorld, IGameBase
    {

        private readonly World World;

        public EnergySystem(World world)
        {
            World = world;

            Event.Register<EnergySystem, Recharge>();
            Event.Register<EnergySystem, Expend>();
        }

        public void Tick()
        {
            ProcessRecharging();
            ProcessExpenditure();     
        }

        private void ProcessRecharging()
        {
            foreach (var recharge in Event.Read<EnergySystem, Recharge>())
            {
                Recharge(recharge.Entity, recharge.Amount);
            }
        }

        private void ProcessExpenditure()
        {
            foreach (var expend in Event.Read<EnergySystem, Expend>())
            {
                Expend(expend.Entity, expend.Amount);
            }
        }

        private void Recharge(Entity entity, int amount)
        {
            var before  = World.Entity.Energy(entity);
            World.Entity.Modify.Energy(entity).Current = Math.Min(before.Current + amount, before.Maximum); 
            var after   = World.Entity.Energy(entity);

            Notify(entity, before.Current, after.Current);
        }

        private void Expend(Entity entity, int amount)
        {
            var before  = World.Entity.Energy(entity);
            World.Entity.Modify.Energy(entity).Current = Math.Max(before.Current - amount, 0); 
            var after   = World.Entity.Energy(entity);

            Notify(entity, before.Current, after.Current);
        }

        private void Notify(Entity entity, int before, int after)
        {
            if (before == after)
                return;

            Event.Send<EnergyChanged>(new() { Entity = entity, Before = before, After = after });
        }
    }
}

using System;

using Game.Realm;
using Game.Common;
using Game.Content;
using System.Collections.Generic;
using System.Linq;




namespace Game.Service
{

    public class AbilitySystem : RegisteredService, IWorld, IData, IGameBase
    {
        private readonly Data Data;
        private readonly World World;

        public AbilitySystem(World world, Data data)
        {
            Data    = data;
            World   = world;
        }

        public void Tick()
        {
            ProcessCommands();    
        }

        private void ProcessCommands()
        {
            foreach (var entity in World.Query(Mask<Components, Commands>.Key))
            {
                ProcessCommandBuffer(entity); 
            }
        }

        private void ProcessCommandBuffer(Entity entity)
        {
            List<Capability> capabilities = new();

            foreach (var (capability, _) in World.Entity.Command(entity).Buffer)
            {
                if (!World.Entity.Loadout(entity).Abilities.TryGetValue(capability, out var ability))
                    continue;

                if (!Can(entity, ability))
                    continue;

                Create(entity, ability);
                capabilities.Add(capability);
            }

            PromoteCapability(entity, capabilities);
        }

        private bool Can(Entity entity, string ability)
        {
            return true;
        }

        private void Create(Entity parent, string ability)
        {
            World.Entity.Create(Data.Definition(ability), parent);
        }

        private void PromoteCapability(Entity entity, List<Capability> capabilities)
        {
            var active = World.Entity.Command(entity).Active;
            var buffer = World.Entity.Command(entity).Buffer;

            foreach(var capability in capabilities)
            {
                if (active.Keys.Contains(capability))
                    continue;

                active[capability] = buffer[capability];

                buffer.Remove(capability);
            }
        }



    }
}

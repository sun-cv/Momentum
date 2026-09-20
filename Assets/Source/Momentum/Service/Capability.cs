using System;
using Game.Common;
using Game.Realm;



namespace Game.Service
{

    public class CapabilitySystem : RegisteredService, IWorld, IRealBase
    {

        private readonly World World;

        public CapabilitySystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            ProcessCapabilityMasks();
        }

        private void ProcessCapabilityMasks()
        {
            
            var innate  = World.Query(Mask<Components, Innate>.Key);
            var blocks  = World.Query(Mask<Components, Blocks>.Key);

            foreach (var entity in innate)
            {
                World.Entity.Capability.Reset(entity);
            }

            foreach (var source in blocks)
            {
                foreach (var capability in World.Entity.Blocks(source).Capabilities)
                {
                    World.Entity.Capability.Block(World.Entity.Parent(source).Entity, capability);
                }
            }
        }
    }
}

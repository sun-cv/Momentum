using UnityEngine;

using Game.Realm;
using Game.Common;

using Event = Game.Common.Event;



namespace Game.Service 
{

        // Ai read position > target

    public class AimSystem : RegisteredService, IWorld, IRealBase
    {

        private readonly World World;

        public AimSystem(World world)
        {
            World = world;
            Event.Register<AimSystem, MousePosition>(); 
        }
    
        public void Tick()
        {
            ProcessAim();
        }

        private void ProcessAim()
        {
            foreach (var entity in World.Query(Mask<Components, Aim>.Key))
            {
                if (World.Entity.Has<PlayerController>(entity))
                {
                    ProcessPlayerAim(entity);
                }
                if (World.Entity.Has<AiController>(entity))
                {
                    ProcessEntityAim(entity);
                }
            }
        }

        private void ProcessPlayerAim(Entity entity)
        {
            var aim = World.Entity.Aim(entity);

            foreach (var message in Event.Read<AimSystem, MousePosition>())
            {
                aim.World = message.World;
            }
            
            World.Entity.Modify.Aim(entity).World       = aim.World;
            World.Entity.Modify.Aim(entity).Direction   = (aim.World - World.Entity.Form(entity).Body.position).normalized;
        }

        private void ProcessEntityAim(Entity entity)
        {
            var target  = World.Entity.Target(entity).Entity;
           
            if (!World.Entity.Alive(target))
                return;

            var targetLocation  = World.Entity.Form(target).Body.position;
            var entityPosition  = World.Entity.Form(entity).Body.position;

            World.Entity.Modify.Aim(entity).World       = targetLocation;
            World.Entity.Modify.Aim(entity).Direction   = (targetLocation - entityPosition).normalized;
        }



    }
}

using Game.Realm;
using Game.Common;
using System;



namespace Game.Service
{

    public class AnchorSystem : RegisteredService, IWorld, IGameBase
    {

        private readonly World World;

        public AnchorSystem(World world)
        {
            World = world;
        }

        public void Tick()
        {
            ProcessAnchors();
        }

        private void ProcessAnchors()
        {
            foreach (var entity in World.Query(Mask<Components, Anchor>.Key))
            {
                MoveAnchor(entity);
            }
        }

        private void MoveAnchor(Entity entity)
        {
            var parent      = FindAnchorRoot(World.Entity.Parent(entity).Entity);
            var anchor      = World.Entity.Anchor(entity);
            var instance    = World.Entity.Instance(entity);

            instance.Transform.position = World.Entity.Instance(parent).Transform.position + anchor.Offset;
        }

        private Entity FindAnchorRoot(Entity parent)
        {
            if (!World.Entity.Has<Parent>(parent))
            {
                if (!World.Entity.Has<Form>(parent))
                    throw new Exception("[AnchorSystem] Root anchor entity is missing form");

                return parent;
            }
            
            return FindAnchorRoot(World.Entity.Parent(parent).Entity);
        }
    }
}

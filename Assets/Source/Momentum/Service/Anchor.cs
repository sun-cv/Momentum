using UnityEngine;

using Game.Realm;
using Game.Common;



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
            var root    = World.Entity.Root(entity);
            var aim     = World.Entity.Aim(World.Entity.Parent(entity).Entity).Direction;
            var angle   = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            var body    = World.Entity.Form(entity).Body;

            body.position = World.Entity.Form(root).Body.position + (Vector2)(Quaternion.Euler(0f, 0f, angle) * World.Entity.Anchor(entity).Offset);
            body.rotation = angle;
        }
    }
}

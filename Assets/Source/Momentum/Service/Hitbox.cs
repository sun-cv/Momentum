using System.Collections.Generic;
using UnityEngine;

using Game.Realm;
using Game.Content;
using Game.Common;
using Game.Common.Events;

using Event = Game.Common.Event;



namespace Game.Service
{

    public class HitboxSystem : RegisteredService, IWorld, IAsset, IGameBase
    {

        private readonly World World;
        private readonly Assets Asset;

        private readonly List<Collider2D> overlaps  = new();
        private readonly ContactFilter2D filter     = new() { useTriggers = true };

        public HitboxSystem(World world, Assets asset)
        {
            World = world;
            Asset = asset;

            Event.Register<HitboxSystem, CreateHitbox>();
        }

        public void Tick()
        {
            CreateHitboxes();
            ProcessHitboxes();
        }

        private void CreateHitboxes()
        {
            foreach (var request in Event.Read<HitboxSystem, CreateHitbox>())
            {
                if (!World.Entity.Alive(request.Parent))
                    continue;

                CreateHitbox(request);
            }
        }

        private void CreateHitbox(CreateHitbox request)
        {
            var blueprint   = Asset.Get(request.Definition);
            var source      = World.Entity.Root(request.Parent);
            var rotation    = Rotation(source);
            var offset      = blueprint.Definition.Anchor is Anchor anchor ? rotation * anchor.Offset : Vector3.zero;
            var position    = (Vector3)World.Entity.Form(source).Body.position + offset;

            var hitbox      = World.Entity.Create(blueprint, new ConstructionParameter { Parent = request.Parent, Position = position, Rotation = rotation.eulerAngles });

            World.Entity.Component.Add(hitbox, new Source { Entity = source });
            World.Entity.Component.Add(hitbox, new Struck { Entities = new() });

            if (World.Entity.Has<Anchor>(hitbox))
                World.Entity.Modify.Anchor(hitbox).Offset = offset;
        }

        private Quaternion Rotation(Entity source)
        {
            if (!World.Entity.Has<Aim>(source))
                return Quaternion.identity;

            var direction = World.Entity.Aim(source).Direction;

            return Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        private void ProcessHitboxes()
        {
            foreach (var hitbox in World.Query(Mask<Components, Hitbox, Form>.Key))
            {
                Overlap(hitbox);
            }
        }

        private void Overlap(Entity hitbox)
        {
            var source  = World.Entity.Source(hitbox).Entity;
            var struck  = World.Entity.Modify.Struck(hitbox).Entities;

            World.Entity.Form(hitbox).Body.Overlap(filter, overlaps);

            foreach (var collider in overlaps)
            {
                if (collider.attachedRigidbody == null || !World.Entity.Body.Owner(collider.attachedRigidbody, out var target))
                    continue;

                if (target.Equals(source) || !World.Entity.Has<Health>(target) || struck.Contains(target))
                    continue;

                struck.Add(target);

                Event.Send(new HitboxStruck { Hitbox = hitbox, Target = target });
            }
        }
    }
}

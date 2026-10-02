using System.Collections.Generic;
using UnityEngine;

using Game.Common;
using Game.Content;
using Game.Realm;

using Animation = Game.Common.Animation;
using Pose      = Game.Common.Pose;



namespace Game.Graphics
{

    public class AnimationSystem : RegisteredService, IWorld, IAsset, IGameBase
    {
        private readonly World World;
        private readonly Assets Assets;

        private readonly Dictionary<Entity, Entity> Resolved = new();

        public AnimationSystem(World world, Assets assets)
        {
            World   = world;
            Assets  = assets;
        }

        public void Tick()
        {
            ProcessAnimations();
        }

        private void ProcessAnimations()
        {
            CollectDrawers();
            ResolvePoses();
            DrawResolved();
        }

        private void CollectDrawers()
        {
            Resolved.Clear();

            foreach (var entity in World.Query(Mask<Components, Animation, Pose, Facing, Rendering>.Key))
            {
                Resolved[entity] = entity;
            }
        }

        private void ResolvePoses()
        {
            foreach (var child in World.Query(Mask<Components, Pose, Parent>.Key))
            {
                if (Resolved.ContainsKey(child))
                    continue;

                var parent = World.Entity.Parent(child).Entity;

                if (!Resolved.TryGetValue(parent, out var current))
                    continue;

                if (current.Equals(parent) || Newer(child, current))
                {
                    Resolved[parent] = child;
                }
            }
        }

        private void DrawResolved()
        {
            foreach (var (drawer, source) in Resolved)
            {
                Draw(drawer, World.Entity.Pose(source));
            }
        }

        private bool Newer(Entity child, Entity current)
        {
            var created = World.Entity.Meta(child).Created;
            var against = World.Entity.Meta(current).Created;

            return created != against ? created > against : child.Index < current.Index;
        }

        private void Draw(Entity entity, Pose pose)
        {
            var direction     = World.Entity.Facing(entity).Direction;
            var renderer      = World.Entity.Rendering(entity).Renderer;
            var sheet         = Assets.Sheet(pose.Sheet);
            ref var animation = ref World.Entity.Modify.Animation(entity);

            Advance(ref animation, pose.Sheet, pose.State);

            if (sheet.Clips.TryGetValue((animation.State, direction), out var clip))
            {
                animation.Shown = direction;
            }

            else if (!sheet.Clips.TryGetValue((animation.State, animation.Shown), out clip) && !sheet.Clips.TryGetValue((animation.State, Vector2.zero), out clip))
                throw new System.Exception($"[AnimationSystem] Entity {entity.Index} has no clip {animation.State} facing {direction} in sheet {animation.Sheet}");

            renderer.sprite = Frame(clip, animation.Elapsed);
        }

        private static void Advance(ref Animation animation, string sheet, string state)
        {
            if (animation.Sheet == sheet && animation.State == state)
            {
                animation.Elapsed++;
                return;
            }

            animation.Sheet   = sheet;
            animation.State   = state;
            animation.Elapsed = 0;
        }

        private static Sprite Frame(Clip clip, int elapsed)
        {
            var tick = clip.Loop ? elapsed % clip.Length : Mathf.Min(elapsed, clip.Length - 1);

            for (var i = 0; i < clip.Ticks.Count; i++)
            {
                if (tick < clip.Ticks[i])
                    return clip.Frames[i];

                tick -= clip.Ticks[i];
            }

            return clip.Frames[clip.Frames.Count - 1];
        }
    }
}

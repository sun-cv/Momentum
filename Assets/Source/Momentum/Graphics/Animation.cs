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

        public AnimationSystem(World world, Assets assets)
        {
            World   = world;
            Assets  = assets;
        }

        public void Tick()
        {
            foreach (var entity in World.Query(Mask<Components, Animation, Pose, Facing, Rendering>.Key))
                Draw(entity);
        }

        private void Draw(Entity entity)
        {
            var pose          = World.Entity.Pose(entity);
            var direction     = World.Entity.Facing(entity).Direction;
            var renderer      = World.Entity.Rendering(entity).Renderer;
            var sheet         = Assets.Sheet(pose.Sheet);
            ref var animation = ref World.Entity.Modify.Animation(entity);

            Advance(ref animation, pose.Sheet, pose.State);

            if (TryResolve(sheet, animation.State, direction, out var clip, out var flip))
            {
                animation.Shown = direction;
            }
            else if (!TryResolve(sheet, animation.State, animation.Shown, out clip, out flip))
            {
                flip = false;

                if (!sheet.Clips.TryGetValue((animation.State, Vector2.zero), out clip))
                    throw new System.Exception($"[AnimationSystem] Entity {entity.Index} has no clip {animation.State} facing {direction} in sheet {animation.Sheet}");
            }

            renderer.sprite = Frame(clip, animation.Elapsed);
            renderer.flipX  = flip;
        }

        private static bool TryResolve(Sheet sheet, string state, Vector2 direction, out Clip clip, out bool flip)
        {
            flip = false;

            if (sheet.Clips.TryGetValue((state, direction), out clip))
                return true;

            flip = direction == Vector2.left;

            return flip && sheet.Clips.TryGetValue((state, Vector2.right), out clip);
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

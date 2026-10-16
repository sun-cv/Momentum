using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Realm;



namespace Game.Graphics
{

    public class Director
    {
        private readonly Dictionary<Entity, Shot>   shots   = new();
        private readonly List<Entity>               expired = new();

        private Entity?         winner;
        private DirectorResult  from;
        private DirectorResult  current;
        private float           blend = 1f;
        private float           span;

        public void Tick(World world, Entity subject)
        {
            var origin = Position(world, subject);

            Gather(world, subject, origin);
            Advance(world);
            Choose(origin);
        }

        public CameraResult Apply(CameraResult result)
        {
            var takeover = current.Takeover;

            return new CameraResult
            {
                Offset      = Vector3.Lerp(result.Offset, current.Offset, takeover),
                Lean        = result.Lean     * (1f - takeover),
                Deadzone    = result.Deadzone * (1f - current.Level),
                Damping     = result.Damping,
                Size        = result.Size * Mathf.Exp(-current.Zoom * takeover),
            };
        }

        private void Gather(World world, Entity subject, Vector2 origin)
        {
            foreach (var shot in shots.Values)
                shot.Seen = false;

            foreach (var entity in world.Query(Mask<Components, Highlight>.Key))
            {
                if (!Applies(world, entity, subject))
                    continue;

                var target      = world.Entity.Has<CameraFocus>(entity) ? world.Entity.CameraFocus(entity).Entity : entity;
                var highlight   = world.Entity.Highlight(entity);
                var valid       = Valid(world, target);
                var point       = valid ? Position(world, target) : Vector2.zero;
                var wanted      = valid && InRange(highlight, origin, point);

                if (!shots.TryGetValue(entity, out var shot))
                {
                    if (!wanted)
                        continue;

                    shot = new Shot();
                    shots[entity] = shot;
                }

                shot.Seen       = true;
                shot.Wanted     = wanted;
                shot.Target     = target;
                shot.Highlight  = highlight;

                if (valid)
                    shot.Point = point;
            }
        }

        private void Advance(World world)
        {
            foreach (var pair in shots)
            {
                var shot = pair.Value;

                if (!shot.Seen)
                {
                    shot.Wanted = false;

                    if (Valid(world, shot.Target))
                        shot.Point = Position(world, shot.Target);
                }

                Ramp(shot);

                if (shot.Level <= 0f && !shot.Wanted && shot.Hold <= 0f)
                    expired.Add(pair.Key);
            }

            foreach (var entity in expired)
                shots.Remove(entity);

            expired.Clear();
        }

        private void Ramp(Shot shot)
        {
            var delta = Watch.Tick.UnscaledDelta;

            if (shot.Wanted)
            {
                shot.Hold   = shot.Highlight.Stickiness * Watch.Tick.Delta;
                shot.Level  = Toward(shot.Level, 1f, shot.Highlight.In, delta);
                return;
            }

            if (shot.Hold > 0f)
            {
                shot.Hold -= delta;
                return;
            }

            shot.Level = Toward(shot.Level, 0f, shot.Highlight.Out, delta);
        }

        private void Choose(Vector2 origin)
        {
            Entity? best        = null;
            Shot    top         = null;
            var     strength    = 0f;
            var     distance    = 0f;

            foreach (var pair in shots)
            {
                var shot    = pair.Value;
                var power   = shot.Highlight.Takeover * shot.Level;
                var range   = Vector2.Distance(origin, shot.Point);

                if (power <= 0f)
                    continue;

                if (top is not null && !Beats(power, range, strength, distance))
                    continue;

                best        = pair.Key;
                top         = shot;
                strength    = power;
                distance    = range;
            }

            var next = top is null ? new DirectorResult() : Result(origin, top);

            if (best != winner)
            {
                from    = current;
                blend   = 0f;
                span    = top is not null && winner is not null ? top.Highlight.In * Watch.Tick.Delta : 0f;
                winner  = best;
            }

            blend   = span > 0f ? Mathf.MoveTowards(blend, 1f, Watch.Tick.UnscaledDelta / span) : 1f;
            current = blend >= 1f ? next : DirectorResult.Blend(from, next, blend);
        }

        private static DirectorResult Result(Vector2 origin, Shot shot)
        {
            var highlight   = shot.Highlight;
            var reach       = (shot.Point + highlight.Offset - origin) * highlight.Center;

            if (highlight.Limit > 0f)
                reach = Vector2.ClampMagnitude(reach, highlight.Limit);

            return new DirectorResult { Offset = reach, Takeover = highlight.Takeover * shot.Level, Level = shot.Level, Zoom = highlight.Zoom };
        }

        private static bool Applies(World world, Entity entity, Entity subject)
        {
            return !world.Entity.Has<Focus>(entity) || world.Entity.Parent(entity).Entity == subject;
        }

        private static bool Valid(World world, Entity entity)
        {
            return world.Entity.Alive(entity) && (world.Entity.Has<Visual>(entity) || world.Entity.Has<Instance>(entity));
        }

        private static bool InRange(Highlight highlight, Vector2 origin, Vector2 point)
        {
            return highlight.Range <= 0f || Vector2.Distance(origin, point) <= highlight.Range;
        }

        private static bool Beats(float power, float range, float strength, float distance)
        {
            return Mathf.Approximately(power, strength) ? range < distance : power > strength;
        }

        private static float Toward(float level, float goal, int ticks, float delta)
        {
            return ticks > 0 ? Mathf.MoveTowards(level, goal, delta / (ticks * Watch.Tick.Delta)) : goal;
        }

        private static Vector2 Position(World world, Entity entity)
        {
            return world.Entity.Has<Visual>(entity)
                ? world.Entity.Visual(entity).Transform.position
                : world.Entity.Instance(entity).Transform.position;
        }
    }

    public class Shot
    {
        public Entity    Target     { get; set; }
        public Highlight Highlight  { get; set; }
        public Vector2   Point      { get; set; }
        public float     Level      { get; set; }
        public float     Hold       { get; set; }
        public bool      Wanted     { get; set; }
        public bool      Seen       { get; set; }
    }

    public struct DirectorResult
    {
        public Vector2 Offset   { get; set; }
        public float   Takeover { get; set; }
        public float   Level    { get; set; }
        public float   Zoom     { get; set; }

        public static DirectorResult Blend(DirectorResult from, DirectorResult to, float blend)
        {
            return new DirectorResult
            {
                Offset      = Vector2.Lerp(from.Offset,   to.Offset,   blend),
                Takeover    = Mathf.Lerp  (from.Takeover, to.Takeover, blend),
                Level       = Mathf.Lerp  (from.Level,    to.Level,    blend),
                Zoom        = Mathf.Lerp  (from.Zoom,     to.Zoom,     blend),
            };
        }
    }
}

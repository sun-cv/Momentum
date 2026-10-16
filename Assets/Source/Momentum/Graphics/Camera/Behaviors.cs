using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

using Game.Common;
using Game.Realm;



namespace Game.Graphics
{
    public abstract class CameraBehavior
    {
        private const float Fade = 4f;

        public float    Target      { get; set; } = 1f;
        public float    Weight      { get; private set; } = 1f;
        public Vector3  Offset      { get; protected set; }
        public Vector3  Lean        { get; protected set; }
        public Vector2  Deadzone    { get; protected set; }

        public void Advance(World world, Entity subject)
        {
            var valid = Guard(world, subject);

            Weight = Mathf.MoveTowards(Weight, valid ? Target : 0f, Fade * Watch.Tick.UnscaledDelta);

            if (valid)
                Tick(world, subject);
        }

        protected abstract void Tick(World world, Entity subject);
        protected abstract bool Guard(World world, Entity subject);
    }

    public class MouseBehavior : CameraBehavior
    {
        private const float Range       = 7f;
        private const float Horizontal  = 1f;
        private const float Vertical    = 2f;
        private const float SpeedX      = 3f;
        private const float SpeedY      = 3f;

        private readonly Camera                  view;
        private readonly CinemachineCameraOffset lean;

        private Vector2 offset;

        public MouseBehavior(Camera view, CinemachineCameraOffset lean)
        {
            this.view = view;
            this.lean = lean;
        }

        protected override bool Guard(World world, Entity subject) 
        { 
            return true;
        }

        protected override void Tick(World world, Entity subject)
        {
            if (Mouse.current is not Mouse pointer)
                return;

            var screen  = pointer.position.ReadValue();
            var cursor  = (Vector2)view.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f)) - (Vector2)lean.Offset;
            var origin  = (Vector2)view.transform.position - (Vector2)lean.Offset;
            var unit    = Vector2.ClampMagnitude((cursor - origin) / Range, 1f);

            offset = new Vector2(
                    Mathf.Lerp(offset.x, unit.x, 1f - Mathf.Exp(-(SpeedX / Horizontal) * Watch.Tick.UnscaledDelta)),
                    Mathf.Lerp(offset.y, unit.y, 1f - Mathf.Exp(-(SpeedY / Vertical)   * Watch.Tick.UnscaledDelta)));

            Lean = new Vector3(offset.x * Horizontal, offset.y * Vertical, 0f);
        }
    }

    public class LeadBehavior : CameraBehavior
    {
        private const float Horizontal  = 2f;
        private const float Vertical    = 3f;
        private const float SpeedX      = 6f;
        private const float SpeedY      = 4f;
        private const float Moving      = 0.1f;

        private Vector2 direction = Vector2.zero;
        private Vector2 lead;

        protected override void Tick(World world, Entity subject)
        {
            var velocity = world.Entity.Control(subject).Velocity;

            if (velocity.sqrMagnitude > Moving * Moving)
            {
                direction = velocity.normalized;
            }

            lead = new Vector2(
                    Mathf.Lerp(lead.x, direction.x, 1f - Mathf.Exp(-(SpeedX / Horizontal) * Watch.Tick.UnscaledDelta)),
                    Mathf.Lerp(lead.y, direction.y, 1f - Mathf.Exp(-(SpeedY / Vertical)   * Watch.Tick.UnscaledDelta)));

            Offset = new Vector3(lead.x * Horizontal, lead.y * Vertical, 0f);
        }

        protected override bool Guard(World world, Entity subject)
        {
            return world.Entity.Has<Control>(subject);
        }
    }

    public class DeadzoneBehavior : CameraBehavior
    {
        private const float Idle    = 1f;
        private const float Open    = 2f;
        private const float Shrink  = 0.75f;
        private const float Moving  = 0.1f;

        private static readonly Vector2 Size = new(0.7f, 0.7f);

        private readonly Camera                  view;
        private readonly CinemachineCameraOffset lean;

        private float still;
        private bool  open;

        public DeadzoneBehavior(Camera view, CinemachineCameraOffset lean)
        {
            this.view = view;
            this.lean = lean;
        }

        public Vector3 Marker { get; set; }

        protected override void Tick(World world, Entity subject)
        {
            var velocity = world.Entity.Velocity(subject).Value;
            var origin   = (Vector2)world.Entity.Visual(subject).Transform.position;
            var marker   = origin + (Vector2)Marker;

            still = velocity.sqrMagnitude > Moving * Moving ? 0f : still + Watch.Tick.UnscaledDelta;

            if (!open && still >= Idle)
            {
                open = true;
            }

            if (open && (Outside(marker) || Outside(origin)))
            {
                open = false;
            }

            var desired = open ? Size : Vector2.zero;
            var speed   = open ? Open : Shrink;

            Deadzone = Vector2.MoveTowards(Deadzone, desired, speed * Watch.Tick.UnscaledDelta);
        }

        private bool Outside(Vector2 position)
        {
            var height  = view.orthographicSize * 2f;
            var screen  = new Vector2(height * view.aspect, height);
            var half    = Vector2.Scale(Size, screen) * 0.5f;
            var offset  = position - ((Vector2)view.transform.position - (Vector2)lean.Offset);

            return Mathf.Abs(offset.x) > half.x || Mathf.Abs(offset.y) > half.y;
        }

        protected override bool Guard(World world, Entity subject) 
        {
            return world.Entity.Has<Velocity>(subject);
        }
    }
}

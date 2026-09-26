using System.Collections.Generic;

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

using Game.Common;
using Game.Diagnostic;
using Game.Realm;
using Game.Common.Events;

using Event = Game.Common.Event;



namespace Game.Graphics
{

    public class CameraRig : RegisteredService, IWorld, IRealLate, IInitialize
    {
        private const float Transition = 1.5f;

        private readonly World World;

        private GameObject                  rig;
        private Camera                      view;
        private CinemachineBrain            brain;
        private CinemachineCamera           follow;
        private CinemachinePositionComposer composer;
        private CinemachineCameraOffset     lean;
        private Transform                   target;

        private CameraMode  from;
        private CameraMode  to;
        private float       blend;

        public CameraRig(World world)
        {
            World = world;
        }

        public void Initialize()
        {
            rig         = new GameObject("CameraRig");
            var root    = new GameObject("CameraRoot");
            var core    = new GameObject("Camera");
            var focus   = new GameObject("CameraTarget");

            root .transform.SetParent(rig.transform, false);
            core .transform.SetParent(rig.transform, false);
            focus.transform.SetParent(rig.transform, false);

            view        = root.AddComponent<Camera>();
            brain       = root.AddComponent<CinemachineBrain>();
            follow      = core.AddComponent<CinemachineCamera>();
            composer    = core.AddComponent<CinemachinePositionComposer>();
            lean        = core.AddComponent<CinemachineCameraOffset>();
            target      = focus.transform;

            view.orthographic               = true;
            brain.UpdateMethod              = CinemachineBrain.UpdateMethods.ManualUpdate;
            follow.Lens.OrthographicSize    = Config.Graphics.Camera.OrthographicSize;
            follow.Target.TrackingTarget    = target;
            lean.ApplyAfter                 = CinemachineCore.Stage.Body;
            lean.PreserveComposition        = false;

            to          = new ExploreMode(view, composer, lean);
            blend       = 1f;

            Event.Push<CameraCreated>(new() { View = view });
        }

        public void Tick()
        {
            foreach (var entity in World.Query(Mask<Components, Common.CameraTarget, Visual, Velocity>.Key))
            {
                Advance(entity);
                Follow(entity);
                break;
            }

            Apply();

            brain.ManualUpdate();
        }

        public void Switch(CameraMode mode)
        {
            from    = to;
            to      = mode;
            blend   = 0f;
        }

        private void Advance(Entity entity)
        {
            from?.Tick(World, entity);
            to   .Tick(World, entity);

            blend = Mathf.MoveTowards(blend, 1f, Watch.Tick.UnscaledDelta / Transition);

            if (blend >= 1f)
            {
                from = null;
            }
        }

        private void Follow(Entity entity)
        {
            target.position = World.Entity.Visual(entity).Transform.position;
        }

        private void Apply()
        {
            var result = from is null ? to.Result : CameraResult.Blend(from.Result, to.Result, blend);

            composer.TargetOffset                   = result.Offset;
            composer.Damping                        = result.Damping;
            composer.Composition.DeadZone.Size      = result.Deadzone;
            composer.Composition.DeadZone.Enabled   = result.Deadzone.sqrMagnitude > 0f;

            lean.Offset                             = result.Lean;
        }

        public Camera View => view;

        static CameraRig() => Log<CameraRig>.Level(Diagnostic.Log.Level.Debug);
    }

    public struct CameraResult
    {
        public Vector3 Offset   { get; set; }
        public Vector3 Lean     { get; set; }
        public Vector2 Deadzone { get; set; }
        public Vector3 Damping  { get; set; }

        public static CameraResult Blend(CameraResult from, CameraResult to, float blend)
        {
            return new CameraResult
            {
                Offset      = Vector3.Lerp(from.Offset,   to.Offset,   blend),
                Lean        = Vector3.Lerp(from.Lean,     to.Lean,     blend),
                Deadzone    = Vector2.Lerp(from.Deadzone, to.Deadzone, blend),
                Damping     = Vector3.Lerp(from.Damping,  to.Damping,  blend),
            };
        }
    }

    public abstract class CameraMode
    {
        protected readonly List<CameraBehavior> behaviors = new();

        public void Tick(World world, Entity focus)
        {
            Rule();

            foreach (var behavior in behaviors)
            {
                behavior.Advance(world, focus);
            }
        }

        public CameraResult Result
        {
            get
            {
                var offset   = Vector3.zero;
                var lean     = Vector3.zero;
                var deadzone = Vector2.zero;

                foreach (var behavior in behaviors)
                {
                    offset   += behavior.Offset   * behavior.Weight;
                    lean     += behavior.Lean     * behavior.Weight;
                    deadzone += behavior.Deadzone * behavior.Weight;
                }

                return new CameraResult { Offset = offset, Lean = lean, Deadzone = deadzone, Damping = Damping };
            }
        }

        protected T Find<T>() where T : CameraBehavior
        {
            foreach (var behavior in behaviors)
            {
                if (behavior is T match)
                    return match;
            }

            throw new KeyNotFoundException($"[CameraMode] {GetType().Name} has no {typeof(T).Name}");
        }

        protected abstract void Rule();

        protected abstract Vector3 Damping { get; }
    }

    public class ExploreMode : CameraMode
    {
        private const float Idle        = 1f;
        private const float Moved       = 0.5f;
        private const float Handover    = 1f;

        private static readonly Vector3 Mouse = new(0.5f, 0.5f, 0f);
        private static readonly Vector3 Lead  = new(0.3f, 0.3f, 0f);

        private Vector2 cursor;
        private float   still;
        private float   blend;

        public ExploreMode(Camera view, CinemachinePositionComposer composer, CinemachineCameraOffset lean)
        {
            behaviors.Add(new MouseBehavior(view, lean));
            behaviors.Add(new LeadBehavior());
            behaviors.Add(new DeadzoneBehavior(view, composer, lean));
        }

        protected override void Rule()
        {
            if (UnityEngine.InputSystem.Mouse.current is not Mouse pointer)
                return;

            var position = pointer.position.ReadValue();

            still   = (position - cursor).sqrMagnitude > Moved * Moved ? 0f : still + Watch.Tick.UnscaledDelta;
            cursor  = position;

            blend   = Mathf.MoveTowards(blend, still >= Idle ? 1f : 0f, Watch.Tick.UnscaledDelta / Handover);

            Find<LeadBehavior>() .Target = blend;
            Find<MouseBehavior>().Target = 1f - blend;
        }

        protected override Vector3 Damping => Vector3.Lerp(Mouse, Lead, blend);
    }

    public abstract class CameraBehavior
    {
        private const float Fade = 4f;

        public float    Target      { get; set; } = 1f;
        public float    Weight      { get; private set; } = 1f;
        public Vector3  Offset      { get; protected set; }
        public Vector3  Lean        { get; protected set; }
        public Vector2  Deadzone    { get; protected set; }

        public void Advance(World world, Entity focus)
        {
            Weight = Mathf.MoveTowards(Weight, Target, Fade * Watch.Tick.UnscaledDelta);

            Tick(world, focus);
        }

        protected abstract void Tick(World world, Entity focus);
    }

    public class MouseBehavior : CameraBehavior
    {
        private const float Range       = 7f;
        private const float Horizontal  = 1f;
        private const float Vertical    = 2f;
        private const float SpeedX      = 4f;
        private const float SpeedY      = 4f;

        private readonly Camera                  view;
        private readonly CinemachineCameraOffset lean;

        private Vector2 offset;

        public MouseBehavior(Camera view, CinemachineCameraOffset lean)
        {
            this.view = view;
            this.lean = lean;
        }

        protected override void Tick(World world, Entity focus)
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
        private const float Horizontal  = 1f;
        private const float Vertical    = 2f;
        private const float SpeedX      = 8f;
        private const float SpeedY      = 4f;
        private const float Moving      = 0.1f;

        private Vector2 direction = Vector2.zero;
        private Vector2 lead;

        protected override void Tick(World world, Entity focus)
        {
            var velocity = world.Entity.Velocity(focus).Value;

            if (velocity.sqrMagnitude > Moving * Moving)
            {
                direction   = velocity.normalized;
            }
            
            if (velocity.sqrMagnitude < Moving * Moving)
            {
                direction   = Vector2.zero;
            }

            lead = new Vector2(
                    Mathf.Lerp(lead.x, direction.x, 1f - Mathf.Exp(-(SpeedX / Horizontal) * Watch.Tick.UnscaledDelta)),
                    Mathf.Lerp(lead.y, direction.y, 1f - Mathf.Exp(-(SpeedY / Vertical)   * Watch.Tick.UnscaledDelta)));

            Offset = new Vector3(lead.x * Horizontal, lead.y * Vertical, 0f);
        }
    }

    public class DeadzoneBehavior : CameraBehavior
    {
        private const float Idle    = 1f;
        private const float Open    = 2f;
        private const float Shrink  = 0.75f;
        private const float Moving  = 0.1f;

        private static readonly Vector2 Size = new(0.7f, 0.7f);

        private readonly Camera                      view;
        private readonly CinemachinePositionComposer composer;
        private readonly CinemachineCameraOffset     lean;

        private float still;
        private bool  open;

        public DeadzoneBehavior(Camera view, CinemachinePositionComposer composer, CinemachineCameraOffset lean)
        {
            this.view       = view;
            this.composer   = composer;
            this.lean       = lean;
        }

        protected override void Tick(World world, Entity focus)
        {
            var velocity = world.Entity.Velocity(focus).Value;
            var origin   = (Vector2)world.Entity.Visual(focus).Transform.position;
            var marker   = origin + (Vector2)composer.TargetOffset;

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
    }
}

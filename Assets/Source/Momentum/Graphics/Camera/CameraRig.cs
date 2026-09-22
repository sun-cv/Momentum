using System.Linq;
using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;
using Game.Realm;
using Unity.Cinemachine;
using UnityEngine;



namespace Game.Graphics
{

    public class CameraRig : RegisteredService, IWorld, IRealLate, IInitialize
    {
        private readonly World World;

        private readonly List<CameraBehavior> behaviors = new();

        private GameObject                  rig;
        private Camera                      view;
        private CinemachineBrain            brain;
        private CinemachineCamera           follow;
        private CinemachinePositionComposer composer;
        private Transform                   target;

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
            target      = focus.transform;

            view.orthographic                       = true;
            brain.UpdateMethod                      = CinemachineBrain.UpdateMethods.ManualUpdate;
            follow.Lens.OrthographicSize            = Config.Graphics.Camera.OrthographicSize;
            follow.Target.TrackingTarget            = target;
            composer.Damping                        = new Vector3(0.3f, 0.3f, 0f);
            composer.Composition.DeadZone.Enabled   = true;

            behaviors.Add(new LeadBehavior());
            // behaviors.Add(new DeadzoneBehavior(view, composer));
        }

        public void Tick()
        {
            foreach (var entity in World.Query(Mask<Components, Common.CameraTarget, Instance, Velocity>.Key))
            {
                Follow(entity);
                Advance(entity);
                break;
            }

            Apply();

            brain.ManualUpdate();
        }

        public void Enable<T>()  where T : CameraBehavior
        {
            Find<T>().Enabled = true;
        }

        public void Disable<T>() where T : CameraBehavior
        {
            Find<T>().Enabled = false;
        }

        private void Follow(Entity entity)
        {
            target.position = World.Entity.Instance(entity).Transform.position;
        }

        private void Advance(Entity entity)
        {
            foreach (var behavior in behaviors)
            {
                behavior.Advance(World, entity, Time.unscaledDeltaTime);
            }
        }

        private void Apply()
        {
            var offset   = Vector3.zero;
            var deadzone = Vector2.zero;

            foreach (var behavior in behaviors)
            {
                offset   += behavior.Offset   * behavior.Weight;
                deadzone += behavior.Deadzone * behavior.Weight;
            }

            composer.TargetOffset               = offset;
            composer.Composition.DeadZone.Size  = deadzone;
        }

        private T Find<T>() where T : CameraBehavior => behaviors.OfType<T>().First();

        public Camera View => view;

        static CameraRig() => Log<CameraRig>.Level(Diagnostic.Log.Level.Debug);
    }

    public abstract class CameraBehavior
    {
        private const float Fade = 4f;

        public bool     Enabled     { get; set; } = true;
        public float    Weight      { get; private set; } = 1f;
        public Vector3  Offset      { get; protected set; }
        public Vector2  Deadzone    { get; protected set; }

        public void Advance(World world, Entity focus, float delta)
        {
            Weight = Mathf.MoveTowards(Weight, Enabled ? 1f : 0f, Fade * delta);

            if (Weight > 0f)
                Tick(world, focus, delta);
        }

        protected abstract void Tick(World world, Entity focus, float delta);
    }

    public class LeadBehavior : CameraBehavior
    {
        private const float Horizontal  = 2f;
        private const float Vertical    = 4f;
        private const float SpeedX      = 10f;
        private const float SpeedY      = 8f;
        private const float Moving      = 0.1f;

        private Vector2 direction = Vector2.right;

        protected override void Tick(World world, Entity focus, float delta)
        {
            var velocity = world.Entity.Velocity(focus).Value;

            if (velocity.sqrMagnitude > Moving * Moving)
                direction = velocity.normalized;

            var desired = new Vector3(direction.x * Horizontal, direction.y * Vertical, 0f);

            Offset = new Vector3(
                    Mathf.MoveTowards(Offset.x, desired.x, SpeedX * delta),
                    Mathf.MoveTowards(Offset.y, desired.y, SpeedY * delta),
                    0f);
        }
    }

    public class DeadzoneBehavior : CameraBehavior
    {
        private const float Idle    = 0.5f;
        private const float Open    = 2f;
        private const float Shrink  = 0.5f;
        private const float Moving  = 0.1f;

        private static readonly Vector2 Size = new(0.7f, 0.7f);

        private readonly Camera                      view;
        private readonly CinemachinePositionComposer composer;

        private float still;
        private bool  open;

        public DeadzoneBehavior(Camera view, CinemachinePositionComposer composer)
        {
            this.view     = view;
            this.composer = composer;

        }

        protected override void Tick(World world, Entity focus, float delta)
        {
            var velocity = world.Entity.Velocity(focus).Value;
            var position = (Vector2)world.Entity.Instance(focus).Transform.position + (Vector2)composer.TargetOffset;

            still = velocity.sqrMagnitude > Moving * Moving ? 0f : still + delta;

            if (!open && still >= Idle)
                open = true;

            if (open && Outside(position))
                open = false;

            var desired = open ? Size : Vector2.zero;
            var speed   = open ? Open : Shrink;

            Deadzone = Vector2.MoveTowards(Deadzone, desired, speed * delta);
        }

        private bool Outside(Vector2 position)
        {
            var height  = view.orthographicSize * 2f;
            var screen  = new Vector2(height * view.aspect, height);
            var half    = Vector2.Scale(Size, screen) * 0.5f;
            var offset  = position - (Vector2)view.transform.position;

            return Mathf.Abs(offset.x) > half.x || Mathf.Abs(offset.y) > half.y;
        }
    }
}

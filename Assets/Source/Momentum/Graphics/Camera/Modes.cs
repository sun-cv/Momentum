using System.Collections.Generic;

using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

using Game.Common;
using Game.Realm;



namespace Game.Graphics
{

    public abstract class CameraMode
    {
        protected readonly List<CameraBehavior> behaviors = new();

        public void Tick(World world, Entity subject)
        {
            Rule(world, subject);

            foreach (var behavior in behaviors)
            {
                behavior.Advance(world, subject);
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

                return new CameraResult { Offset = offset, Lean = lean, Deadzone = deadzone, Damping = Damping, Size = Size };
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

        protected abstract void Rule(World world, Entity subject);
        protected abstract Vector3 Damping { get; }
        protected virtual float Size => Config.Engine.Camera.OrthographicSize;
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

        public ExploreMode(Camera view, CinemachineCameraOffset lean)
        {
            behaviors.Add(new MouseBehavior(view, lean));
            behaviors.Add(new LeadBehavior());
            behaviors.Add(new DeadzoneBehavior(view, lean));
        }

        protected override void Rule(World world, Entity subject)
        {
            Find<DeadzoneBehavior>().Marker = Result.Offset;

            if (UnityEngine.InputSystem.Mouse.current is not Mouse pointer)
                return;

            var position = pointer.position.ReadValue();

            still   = (position - cursor).sqrMagnitude > Moved * Moved ? 0f : still + Watch.Tick.UnscaledDelta;
            cursor  = position;

            blend   = Mathf.MoveTowards(blend, still >= Idle ? 1f : 0f, Watch.Tick.UnscaledDelta / Handover);
        }

        protected override Vector3 Damping => Vector3.Lerp(Mouse, Lead, blend) * 3;
    }
}

using System.Collections.Generic;

using Unity.Cinemachine;
using UnityEngine;

using Game.Common;
using Game.Diagnostic;
using Game.Realm;
using Game.Common.Events;

using Event = Game.Common.Event;



namespace Game.Graphics
{

    public class CameraRig : RegisteredService, IWorld, IRealLate, IInitialize
    {
        private readonly World World;

        private GameObject                  rig;
        private Camera                      view;
        private CinemachineBrain            brain;
        private CinemachineCamera           follow;
        private CinemachinePositionComposer composer;
        private CinemachineCameraOffset     lean;
        private Transform                   target;

        private readonly Dictionary<Framing, CameraMode>    modes       = new();
        private readonly Framer                             framer      = new();
        private readonly Director                           director    = new();
        private readonly CueLayer                           cues        = new();

        private CameraMode                  mode;
        private CameraResult                from;
        private float                       blend;
        private float                       span;

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
            follow.Lens.OrthographicSize    = Config.Engine.Camera.OrthographicSize;
            follow.Target.TrackingTarget    = target;
            lean.ApplyAfter                 = CinemachineCore.Stage.Body;
            lean.PreserveComposition        = false;

            modes[Framing.Explore] = new ExploreMode(view, lean);

            mode        = modes[Framing.Explore];
            blend       = 1f;

            Event.Push<CameraCreated>(new() { View = view });
        }

        public void Tick()
        {
            Frame();

            foreach (var entity in World.Query(Mask<Components, Common.CameraTarget, Visual>.Key))
            {
                Advance(entity);
                Follow(entity);
                director.Tick(World, entity);
                break;
            }

            cues.Tick(World, target.position);

            Apply();

            brain.ManualUpdate();
        }

        private void Frame()
        {
            if (!framer.Tick(World, out var framing, out var ticks))
                return;

            var next = modes[framing];

            if (next == mode)
                return;

            from    = Compose();
            mode    = next;
            blend   = 0f;
            span    = ticks * Watch.Tick.Delta;
        }

        private void Advance(Entity subject)
        {
            mode.Tick(World, subject);

            blend = span > 0f ? Mathf.MoveTowards(blend, 1f, Watch.Tick.UnscaledDelta / span) : 1f;
        }

        private void Follow(Entity subject)
        {
            target.position = World.Entity.Visual(subject).Transform.position;
        }

        private void Apply()
        {
            var result  = director.Apply(Compose());
            var cue     = cues.Result;

            composer.TargetOffset                   = result.Offset;
            composer.Damping                        = result.Damping;
            composer.Composition.DeadZone.Size      = result.Deadzone;
            composer.Composition.DeadZone.Enabled   = result.Deadzone.sqrMagnitude > 0f;

            lean.Offset                             = result.Lean + cue.Lean;
            follow.Lens.OrthographicSize            = result.Size * cue.Zoom;
        }

        private CameraResult Compose()
        {
            return blend >= 1f ? mode.Result : CameraResult.Blend(from, mode.Result, blend);
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
        public float   Size     { get; set; }

        public static CameraResult Blend(CameraResult from, CameraResult to, float blend)
        {
            return new CameraResult
            {
                Offset      = Vector3.Lerp(from.Offset,   to.Offset,   blend),
                Lean        = Vector3.Lerp(from.Lean,     to.Lean,     blend),
                Deadzone    = Vector2.Lerp(from.Deadzone, to.Deadzone, blend),
                Damping     = Vector3.Lerp(from.Damping,  to.Damping,  blend),
                Size        = Mathf.Lerp  (from.Size,     to.Size,     blend),
            };
        }
    }
}

using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

using DG.Tweening;
using UnityEngine.Rendering.Universal;



public class CameraContext
{
    public CinemachineCamera            camera;
    public CinemachineBrain             brain;
    public CinemachinePositionComposer  composer;
    public Camera                       cameraRoot;
    public Transform                    cameraTarget;
    public PixelPerfectCamera           cameraPixel;
    public Context hero;
}


public interface ICameraTarget
{
    public bool IsValid { get; }
    public Vector3 GetPosition();
}

public struct DefaultTargetProvider : ICameraTarget
{
    public readonly bool IsValid            => true;
    public readonly Vector3 GetPosition()   => new();
}



public class CameraRig : RegisteredService, IServiceTick, IServiceLoop
{
    GameObject cameraRig;
    CameraContext context;

    ICameraTarget targetProvider;
    readonly List<ICameraBehavior> activeBehaviors  = new();

    public override void Initialize()
    {
        context     = new();

        cameraRig   = new GameObject("CameraRig");
        var core    = new GameObject("Camera");
        var root    = new GameObject("CameraRoot");
        var hero    = new GameObject("CameraTarget");

        core.transform.SetParent(cameraRig.transform, false);
        root.transform.SetParent(cameraRig.transform, false);
        hero.transform.SetParent(cameraRig.transform, false);

        context.camera      = core.AddComponent<CinemachineCamera>();
        context.composer    = core.AddComponent<CinemachinePositionComposer>();

        context.cameraRoot  = root.AddComponent<Camera>();
        context.brain       = root.AddComponent<CinemachineBrain>();

        context.cameraRoot.orthographic         = true;
        context.camera.Lens.OrthographicSize    = Config.GRAPHICS_ORTHOGRAPHIC;

        context.cameraTarget                    = hero.transform;
        context.camera.Target.TrackingTarget    = context.cameraTarget;

        foreach(var (type, behavior) in cameraBehaviors) 
             behavior.Initialize(context);

        // ActivateBehavior(CameraBehavior.PlayerOffset);
        ActivateBehavior(CameraBehavior.MouseOffset);
        ActivateBehavior(CameraBehavior.PlayerDeadzone);

    }

    public void Tick()
    {
        UpdateCameraTargetPosition();

        foreach (var behavior in activeBehaviors)
            behavior.Tick();
    }


    public void Loop()
    {
        foreach (var behavior in activeBehaviors)
            behavior.Loop();
    }

    public void ActivateBehavior(CameraBehavior behavior)
    {
        if (!cameraBehaviors.TryGetValue(behavior, out var instance))
            return;

        instance.Enable();
        activeBehaviors.Add(instance);
    }

    public void DeactivateBehavior(CameraBehavior behavior)
    {
        if (!cameraBehaviors.TryGetValue(behavior, out var instance))
            return;

        instance.Disable();
        activeBehaviors.Remove(instance);
    }

    void UpdateCameraTargetPosition()
    {
        if (targetProvider == null || !targetProvider.IsValid)
            targetProvider = new DefaultTargetProvider();

        context.cameraTarget.position = targetProvider.GetPosition();
    }
    public void SetCameraTarget(ICameraTarget target) => targetProvider = target;
    public void AssignHero(Hero hero) => context.hero = hero.Context;

    public UpdatePriority Priority => ServiceUpdatePriority.CameraRig;

    readonly Dictionary<CameraBehavior, ICameraBehavior> cameraBehaviors  = new()
    {
        { CameraBehavior.PlayerOffset,      new CameraOffsetBehavior()      },
        { CameraBehavior.MouseOffset,       new CameraMouseOffsetBehavior() },
        { CameraBehavior.PlayerDeadzone,    new DeadzoneCameraBehavior()    }
    };


}

public enum CameraBehavior
{
    PlayerOffset,
    MouseOffset,
    PlayerDeadzone,
}


public interface ICameraBehavior
{
    public void Initialize(CameraContext context);
    public void Tick();
    public void Loop();
    public void Enable();
    public void Disable();
    public void Cleanup();
}


public class CameraOffsetBehavior : ICameraBehavior
{
    readonly float followSpeedX         = 3f;
    readonly float followSpeedY         = 6f;
    readonly float verticalOffset       = 3f;
    readonly float horizontalOffset     = 2f;
    readonly Ease easing                = Ease.OutSine;

    Vector3 targetOffset                = Vector3.zero;
    Vector3 currentOffset               = Vector3.zero;
    Vector3 lastMoveDirection           = Vector3.right;

    CameraContext context;
    Tween offsetTween;

    public void Initialize(CameraContext context)
    {
        this.context = context;
    }

    public void Enable()
    {
        context.composer.Damping.x = 0.3f;
        context.composer.Damping.y = 0.3f;
        currentOffset = context.composer.TargetOffset;
    }

    public void Disable()
    {
        context.composer.Damping.x = 0;
        context.composer.Damping.y = 0;
    }

    public void Tick() {}

    public void Loop()
    {
        if (context.hero.IsMoving)
        {
            var moveDir = context.hero.Velocity.normalized;
            if (moveDir.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = moveDir;
            }
        }

        var desiredOffset = new Vector3(
            lastMoveDirection.x * horizontalOffset,
            lastMoveDirection.y * verticalOffset,
            0f
        );
        if (desiredOffset != targetOffset)
        {
            targetOffset = desiredOffset;
            ApplyOffset(desiredOffset);
        }
    }


    void ApplyOffset(Vector3 offset)
    {
        offsetTween?.Kill();

        var maxDuration = Mathf.Max(
            Mathf.Abs(offset.x - context.composer.TargetOffset.x) / followSpeedX,
            Mathf.Abs(offset.y - context.composer.TargetOffset.y) / followSpeedY
        );

        offsetTween = DOTween.To(
            () => context.composer.TargetOffset,
            x => context.composer.TargetOffset = x,
            offset,
            maxDuration
        ).SetEase(easing);
    }

    public void Cleanup()
    {
        offsetTween?.Kill();
    }
}

public class CameraMouseOffsetBehavior : ICameraBehavior
{
    readonly float followSpeedX     = 3f;
    readonly float followSpeedY     = 6f;
    readonly float maxOffsetX       = 2f;
    readonly float maxOffsetY       = 3f;
    readonly Ease easing            = Ease.OutSine;

    Vector3 targetOffset            = Vector3.zero;

    CameraContext context;
    Tween offsetTween;

    public void Initialize(CameraContext context)
    {
        this.context = context;
    }

    public void Enable()
    {
        context.composer.Damping.x = 0.3f;
        context.composer.Damping.y = 0.3f;
    }

    public void Disable()
    {
        context.composer.Damping.x = 0;
        context.composer.Damping.y = 0;
    }

    public void Tick() {}

    public void Loop()
    {
        Vector2 mousePos = Services.Get<InputRouter>().MousePosition;

        Vector2 screenCenter = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        Vector2 normalizedDelta = (mousePos - screenCenter);
        normalizedDelta.x /= screenCenter.x;
        normalizedDelta.y /= screenCenter.y;

        normalizedDelta = Vector2.ClampMagnitude(normalizedDelta, 1f);

        var desiredOffset = new Vector3(
            normalizedDelta.x * maxOffsetX,
            normalizedDelta.y * maxOffsetY,
            0f
        );

    if (desiredOffset != targetOffset)
    {
        targetOffset = desiredOffset;
            ApplyOffset(desiredOffset);
    }
    }

    void ApplyOffset(Vector3 offset)
    {
        offsetTween?.Kill();

        float maxDuration = Mathf.Max(
            Mathf.Abs(offset.x - context.composer.TargetOffset.x) / followSpeedX,
            Mathf.Abs(offset.y - context.composer.TargetOffset.y) / followSpeedY
        );

        offsetTween = DOTween.To(
            () => context.composer.TargetOffset,
            x => context.composer.TargetOffset = x,
            offset,
            maxDuration
        ).SetEase(easing);
    }

    public void Cleanup()
    {
        offsetTween?.Kill();
    }
}


public class DeadzoneCameraBehavior : ICameraBehavior
{
    private enum DeadzoneState
    {
        Closed,
        Opening,
        Open,
        Shrinking
    }

    readonly float idleTimeThreshold        = 0.5f;
    readonly float deadzoneTimeThreshold    = 1f;
    readonly float openSpeed                = 2f;
    readonly float shrinkSpeed              = 0.5f;
    readonly float sizeThreshold            = 0.3f;

    readonly Ease openEase                  = Ease.Linear;
    readonly Ease shrinkEase                = Ease.Linear;

    readonly Vector2 idleSize               = new(.5f, .5f);
    readonly Vector2 movingSize             = new(0f, 0f);

    CameraContext context;
    CinemachinePositionComposer composer;

    Tween sizeTween;
    DeadzoneState state = DeadzoneState.Closed;
    
    ClockWatch timer;

    public void Initialize(CameraContext context)
    {
        this.context  = context;
        
        composer = context.composer;
    }

    public void Enable()
    {
        composer.Composition.DeadZone.Enabled = true;
        composer.Composition.DeadZone.Size = movingSize;

        timer = new ClockWatch();
        timer.Start();

        state       = DeadzoneState.Closed;
    }

    public void Disable()
    {
        composer.Composition.DeadZone.Enabled = false;
        timer.Reset();
    }

    public void Tick() {}

    public void Loop()
    {
        Vector2 currentSize = composer.Composition.DeadZone.Size;

        switch (state)
        {
            case DeadzoneState.Closed:
                if (ShouldOpenDeadzone())
                {
                    StartSizeTween(idleSize, openSpeed, openEase);
                    state = DeadzoneState.Opening;
                }
                break;

            case DeadzoneState.Opening:
                if (IsNearTarget(currentSize, idleSize))
                {
                    state = DeadzoneState.Open;
                    ResetTimer();
                }
                break;

            case DeadzoneState.Open:
                if (IsPlayerOutsideDeadzone())
                {
                    StartSizeTween(movingSize, shrinkSpeed, shrinkEase);
                    state = DeadzoneState.Shrinking;
                }
                break;

            case DeadzoneState.Shrinking:
                if (IsNearTarget(currentSize, movingSize))
                {
                    state = DeadzoneState.Closed;
                    ResetTimer();
                }
                break;
        }
    }

    bool ShouldOpenDeadzone()
    {
        return context.hero.IsIdle.Timer.CurrentTime > idleTimeThreshold && timer.CurrentTime > deadzoneTimeThreshold;
    }

    bool IsNearTarget(Vector2 current, Vector2 target)
    {
        return Vector2.Distance(current, target) <= sizeThreshold;
    }

    void ResetTimer()
    {
        timer = new ClockWatch();
        timer.Start();
    }

    bool IsPlayerOutsideDeadzone()
    {
        var rect = GetDeadzoneWorldBounds();
        
        Vector3 offsetTarget = context.cameraTarget.position + composer.TargetOffset;
        Vector2 point = offsetTarget;

        return !rect.Contains(point);
    }

    Rect GetDeadzoneWorldBounds()
    {
        float camHeight = context.camera.Lens.OrthographicSize * 2f;
        float camWidth = camHeight * context.camera.Lens.Aspect;

        Vector2 screenSize = new(camWidth, camHeight);
        Vector2 zoneSize = Vector2.Scale(composer.Composition.DeadZone.Size, screenSize);

        Vector3 center = context.camera.transform.position;
        Vector2 half = zoneSize / 2f;
        Vector3 min = center - new Vector3(half.x, half.y, 0f);
        
        return new Rect(min, zoneSize);
    }

    void StartSizeTween(Vector2 toSize, float speed, Ease ease)
    {
        sizeTween?.Kill();

        Vector2 currentSize = composer.Composition.DeadZone.Size;
        float distance = Vector2.Distance(currentSize, toSize);
        float duration = distance / Mathf.Max(speed, 0.01f);

        sizeTween = DOTween.To(
            () => composer.Composition.DeadZone.Size,
            x => composer.Composition.DeadZone.Size = x,
            toSize,
            duration
        ).SetEase(ease);
    }

    public void Cleanup()
    {
        sizeTween?.Kill();
    }

}
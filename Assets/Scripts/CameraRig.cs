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
    public PixelPerfectCamera           cameraPixel;
    public Transform                    cameraTarget;
}


public interface ICameraTarget
{
    public bool IsValid { get; }
    public Vector3 GetPosition();
}

public interface IAdvancedCameraTarget : ICameraTarget
{
    public bool IsMoving        { get; }
    public Vector2 Velocity     { get; }
    public TimePredicate Idle   { get; }
}

public struct DefaultTargetProvider : ICameraTarget
{
    public readonly bool IsValid            => true;
    public readonly Vector3 GetPosition()   => new();
}



public class CameraRig : RegisteredService, IServiceTick, IBind
{
    GameObject  cameraRig;
    CameraContext context;
    ICameraTarget target;

    readonly HashSet<CameraBehavior> activeBehaviors    = new();

    public override void Initialize()
    {
        context     = new();

        cameraRig   = new GameObject("CameraRig");
        var core    = new GameObject("Camera");
        var root    = new GameObject("CameraRoot");
        var target  = new GameObject("CameraTarget");

        core  .transform.SetParent(cameraRig.transform, false);
        root  .transform.SetParent(cameraRig.transform, false);
        target.transform.SetParent(cameraRig.transform, false);

        context.camera      = core.AddComponent<CinemachineCamera>();
        context.composer    = core.AddComponent<CinemachinePositionComposer>();

        context.cameraRoot  = root.AddComponent<Camera>();
        context.brain       = root.AddComponent<CinemachineBrain>();

        context.cameraTarget= target.transform;

        context.cameraRoot.orthographic         = true;

        context.camera.Lens.OrthographicSize    = Config.Graphics.ORTHOGRAPHIC_SIZE;
        context.camera.Target.TrackingTarget    = target.transform;

        context.brain.UpdateMethod = CinemachineBrain.UpdateMethods.SmartUpdate;

        foreach(var (type, behavior) in cameraBehaviors) 
             behavior.Initialize(context);
    }

    public void Tick()
    {
        UpdateCameraTargetPosition();

        foreach (var behavior in activeBehaviors)
            cameraBehaviors[behavior].Tick();
    }


    void UpdateCameraTargetPosition()
    {
        if (target == null || !target.IsValid)
            target = new DefaultTargetProvider();

        context.cameraTarget.position = target.GetPosition();
    }


    public void ActivateBehavior(CameraBehavior behavior)
    {
        if (!cameraBehaviors.TryGetValue(behavior, out var instance))
            return;

        if (!instance.IsValid(target))
            return;

        instance.Enable(target);
        activeBehaviors.Add(behavior);
    }

    public void DeactivateBehavior(CameraBehavior behavior)
    {
        if (!cameraBehaviors.TryGetValue(behavior, out var instance))
            return;

        instance.Disable();
        activeBehaviors.Remove(behavior);
    }


    public void SetCameraTarget(ICameraTarget target)
    {
        if (target == null || !target.IsValid)
            target = new DefaultTargetProvider();

        this.target = target;

        ReevaluateBehaviors();
    }

    public void ReevaluateBehaviors()
    {
        foreach (var behavior in activeBehaviors)
        {
            if (cameraBehaviors[behavior].IsValid(target))
                return;

            DeactivateBehavior(behavior);
        }
    }

    public Camera Camera => context.cameraRoot;

    public UpdatePriority Priority => ServiceUpdatePriority.CameraRig;

    readonly Dictionary<CameraBehavior, ICameraBehavior> cameraBehaviors  = new()
    {
        { CameraBehavior.PlayerOffset,      new CameraOffsetBehavior()      },
        { CameraBehavior.MouseOffset,       new CameraMouseOffsetBehavior() },
        { CameraBehavior.PlayerDeadzone,    new DeadzoneCameraBehavior()    }
    };


    public void Bind()
    {
        foreach(var (type, behavior) in cameraBehaviors) 
            behavior.Bind();
    }

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
    public bool IsValid(ICameraTarget target);
    public void Enable(ICameraTarget target);
    public void Disable();
    public void Bind();
    public void Cleanup();
}


public class CameraOffsetBehavior : ICameraBehavior
{
    CameraContext                       context;
    IAdvancedCameraTarget               target;

    readonly float followSpeedX         = 3f;
    readonly float followSpeedY         = 6f;
    readonly float verticalOffset       = 3f;
    readonly float horizontalOffset     = 2f;
    readonly Ease easing                = Ease.OutSine;

    Vector3 targetOffset                = Vector3.zero;
    Vector3 currentOffset               = Vector3.zero;
    Vector3 lastMoveDirection           = Vector3.right;

    Tween offsetTween;

    public void Initialize(CameraContext context)
    {
        this.context = context;
    }

    public bool IsValid(ICameraTarget target)
    {
        return target is IAdvancedCameraTarget;
    }

    public void Enable(ICameraTarget target)
    {
        this.target = target as IAdvancedCameraTarget; 

        context.composer.Damping.x = 0.3f;
        context.composer.Damping.y = 0.3f;
        currentOffset = context.composer.TargetOffset;
    }

    public void Disable()
    {
        this.target = null;

        context.composer.Damping.x = 0;
        context.composer.Damping.y = 0;
    }

    public void Tick()
    {
        if (target.IsMoving)
            StoreLastMoveDirection();

        var desiredOffset = CalculateDesiredOffset();
        
        if (desiredOffset != targetOffset)
        {
            targetOffset = desiredOffset;
            ApplyOffset(desiredOffset);
        }
    }


    void StoreLastMoveDirection()
    {
        var moveDir = target.Velocity.normalized;
        if (moveDir.sqrMagnitude > 0.01f)
            lastMoveDirection = moveDir;
    }

    Vector3 CalculateDesiredOffset()
    {
        return new Vector3(lastMoveDirection.x * horizontalOffset, lastMoveDirection.y * verticalOffset, 0f);
    }

    void ApplyOffset(Vector3 offset)
    {
        offsetTween?.Kill();

        var maxDuration = Mathf.Max(
            Mathf.Abs(offset.x - context.composer.TargetOffset.x) / followSpeedX,
            Mathf.Abs(offset.y - context.composer.TargetOffset.y) / followSpeedY
        );

        offsetTween = DOTween.To(() => context.composer.TargetOffset, x => context.composer.TargetOffset = x, offset, maxDuration).SetEase(easing);
    }

    public void Bind() {}

    public void Cleanup()
    {
        offsetTween?.Kill();
    }
}

public class CameraMouseOffsetBehavior : ICameraBehavior
{
    CameraContext                   context;
    IAdvancedCameraTarget           target;

    RemoteVector2                   mousePosition;

    readonly float followSpeedX     = 1f;
    readonly float followSpeedY     = 1f;
    readonly float maxOffsetX       = 2f;
    readonly float maxOffsetY       = 2f;
    readonly float damping          = .5f;
    readonly float maxDistance      = 10f;
    readonly Ease easing            = Ease.OutQuint;

    Vector3 targetOffset            = Vector3.zero;

    Tween offsetTween;

    public void Initialize(CameraContext context)
    {
        this.context = context;
    }

    public bool IsValid(ICameraTarget target)
    {
        return target is IAdvancedCameraTarget;
    }

    public void Enable(ICameraTarget target)
    {
        this.target = target as IAdvancedCameraTarget;

        context.composer.Damping.x = damping;
        context.composer.Damping.y = damping;
    }

    public void Disable()
    {
        this.target = null;

        context.composer.Damping.x = 0;
        context.composer.Damping.y = 0;
    }

    public void Tick()
    {
        Vector2 mouseWorldPos   = context.cameraRoot.ScreenToWorldPoint((Vector2)mousePosition);
        Vector2 characterPos    = target.GetPosition();
        Vector2 worldDelta      = mouseWorldPos - characterPos;
        Vector2 normalizedDelta = worldDelta / maxDistance;

        normalizedDelta = Vector2.ClampMagnitude(normalizedDelta, 1f);

        var desiredOffset = new Vector3(normalizedDelta.x * maxOffsetX,  normalizedDelta.y * maxOffsetY, 0f);

        if (Vector3.Distance(desiredOffset, targetOffset) > 0.01f)
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

        offsetTween = DOTween.To(() => context.composer.TargetOffset, x => context.composer.TargetOffset = x, offset, maxDuration).SetEase(easing);
    }

    public void Bind()
    {
        mousePosition = Services.Get<InputRouter>().RemoteMousePosition;
    }

    public void Cleanup()
    {
        offsetTween?.Kill();
    }
}


public class DeadzoneCameraBehavior : ICameraBehavior
{
    CameraContext                           context;
    IAdvancedCameraTarget                   target;

    readonly float idleTimeThreshold        = 0.5f;
    readonly float deadzoneTimeThreshold    = 1f;
    readonly float openSpeed                = 2f;
    readonly float shrinkSpeed              = 0.5f;
    readonly float sizeThreshold            = 0.3f;

    readonly Ease openEase                  = Ease.Linear;
    readonly Ease shrinkEase                = Ease.Linear;

    readonly Vector2 idleSize               = new(.5f, .5f);
    readonly Vector2 movingSize             = new(0f, 0f);

    Tween sizeTween;
    DeadzoneState state = DeadzoneState.Closed;
    
    ClockWatch timer;

    public void Initialize(CameraContext context)
    {
        this.context  = context;
    }

    public bool IsValid(ICameraTarget target)
    {
        return target is IAdvancedCameraTarget;
    }

    public void Enable(ICameraTarget target)
    {
        this.target = target as IAdvancedCameraTarget;

        context.composer.Composition.DeadZone.Enabled = true;
        context.composer.Composition.DeadZone.Size = movingSize;

        timer = new ClockWatch();
        timer.Start();

        state       = DeadzoneState.Closed;
    }

    public void Disable()
    {
        this.target = null;

        context.composer.Composition.DeadZone.Enabled = false;
        timer.Reset();
    }

    public void Tick()
    {
        Vector2 currentSize = context.composer.Composition.DeadZone.Size;

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
        return target.Idle.Timer.CurrentTime > idleTimeThreshold && timer.CurrentTime > deadzoneTimeThreshold;
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
        
        Vector3 offsetTarget = target.GetPosition() + context.composer.TargetOffset;
        Vector2 point = offsetTarget;

        return !rect.Contains(point);
    }

    Rect GetDeadzoneWorldBounds()
    {
        float camHeight = context.camera.Lens.OrthographicSize * 2f;
        float camWidth = camHeight * context.camera.Lens.Aspect;

        Vector2 screenSize = new(camWidth, camHeight);
        Vector2 zoneSize = Vector2.Scale(context.composer.Composition.DeadZone.Size, screenSize);

        Vector3 center = context.camera.transform.position;
        Vector2 half = zoneSize / 2f;
        Vector3 min = center - new Vector3(half.x, half.y, 0f);
        
        return new Rect(min, zoneSize);
    }

    void StartSizeTween(Vector2 toSize, float speed, Ease ease)
    {
        sizeTween?.Kill();

        Vector2 currentSize = context.composer.Composition.DeadZone.Size;
        float distance = Vector2.Distance(currentSize, toSize);
        float duration = distance / Mathf.Max(speed, 0.01f);

        sizeTween = DOTween.To(() => context.composer.Composition.DeadZone.Size, x => context.composer.Composition.DeadZone.Size = x, toSize, duration).SetEase(ease);
    }

    public void Bind() {}

    public void Cleanup()
    {
        sizeTween?.Kill();
    }

    private enum DeadzoneState
    {
        Closed,
        Opening,
        Open,
        Shrinking
    }

}
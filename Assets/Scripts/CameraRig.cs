using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using System;



public class CameraRig : RegisteredService, IServiceTick, IServiceLate, IInitialize
{
    ICameraTarget   target;
    CameraContext   context;
    GameObject      cameraRig;
    
        // -----------------------------------

    HashSet<ICameraBehavior>                    activeBehaviors;
    Dictionary<CameraBehavior, ICameraBehavior> cameraBehaviors;

    // ===============================================================================

    public void Initialize()
    {
        context     = new();

        cameraRig   = new GameObject("CameraRig");
        var core    = new GameObject("Camera");
        var root    = new GameObject("CameraRoot");
        var target  = new GameObject("CameraTarget");

        core  .transform.SetParent(cameraRig.transform, false);
        root  .transform.SetParent(cameraRig.transform, false);
        target.transform.SetParent(cameraRig.transform, false);

        context.camera          = core.AddComponent<CinemachineCamera>();
        context.composer        = core.AddComponent<CinemachinePositionComposer>();

        context.cameraRoot      = root.AddComponent<Camera>();
        context.brain           = root.AddComponent<CinemachineBrain>();

        context.cameraTarget    = target.transform;

        context.cameraRoot.orthographic = true;

        context.camera.Lens.OrthographicSize = Config.Graphics.ORTHOGRAPHIC_SIZE;
        context.camera.Target.TrackingTarget = target.transform;

        context.brain.UpdateMethod = CinemachineBrain.UpdateMethods.SmartUpdate;

        InitializeCameraBehaviors();
    }

    void InitializeCameraBehaviors()
    {
        activeBehaviors  = new();

        cameraBehaviors  = new()
        {
            { CameraBehavior.PlayerOffset,      new CameraOffsetBehavior()      },
            { CameraBehavior.MouseOffset,       new CameraMouseOffsetBehavior() },
            { CameraBehavior.PlayerDeadzone,    new DeadzoneCameraBehavior()    }
        };

        foreach(var (type, behavior) in cameraBehaviors)
        {
             behavior.Initialize(context);
        }
    }

    // ===============================================================================

    public void Tick()
    {
        TickCameraBehaviors();
    }

    public void Late()
    {
        UpdateCameraTargetPosition();
    }

    // ===============================================================================

    void TickCameraBehaviors()
    {
        foreach (var instance in activeBehaviors)
        {
            if (!instance.IsValidTarget(target))
                return;

            instance.Tick(target);
        }
    }

    void UpdateCameraTargetPosition()
    {
        if (target is null || !target.IsValid)
        {
            target = new DefaultTargetProvider();
        }

        context.cameraTarget.position = target.GetPosition();
    }

    public void ActivateBehavior(CameraBehavior behavior)
    {
        if (!cameraBehaviors.TryGetValue(behavior, out var instance))
            return;

        if (!instance.IsValidTarget(target))
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

    public void SetCameraTarget(ICameraTarget target)
    {
        if (target == null || !target.IsValid)
            target = new DefaultTargetProvider();

        this.target = target;

        ReevaluateBehaviors();
    }

        // ===================================
        //  Validation
        // ===================================

    public void ReevaluateBehaviors()
    {
        var toDeactivate = new List<ICameraBehavior>();

        foreach (var behavior in activeBehaviors)
        {
            if (!behavior.IsValidTarget(target))
            {
                toDeactivate.Add(behavior);
            }
        }

        foreach (var behavior in toDeactivate)
        {
            activeBehaviors.Remove(behavior);
        }
    }

    // ===============================================================================

    
    readonly Logger Log = Logging.For(LogSystem.Camera);

    public override void Dispose()
    {
        foreach (var behavior in cameraBehaviors.Values)
        {
            behavior.Dispose();
        }

        activeBehaviors.Clear();
    }

    public Camera Camera            => context.cameraRoot;
    public UpdatePriority Priority  => ServiceUpdatePriority.CameraRig;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum CameraBehavior
{
    PlayerOffset,
    MouseOffset,
    PlayerDeadzone,
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Behaviors
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface ICameraBehavior : IDisposable
{
    public void Initialize      (CameraContext context);
    public void Tick            (ICameraTarget target);
    public bool IsValidTarget   (ICameraTarget target);
    public void Enable  ();
    public void Disable ();
}


    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                      Camera Offset
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CameraOffsetBehavior : ICameraBehavior
{
    CameraContext                       context;

        // ---------------------------------

    readonly float dampingX             = 0.3f;
    readonly float dampingY             = 0.3f;

    readonly float followSpeedX         = 3f;
    readonly float followSpeedY         = 6f;
    readonly float verticalOffset       = 3f;
    readonly float horizontalOffset     = 2f;
    readonly Ease easing                = Ease.OutSine;

    Vector3 targetOffset                = Vector3.zero;
    Vector3 currentOffset               = Vector3.zero;
    Vector3 lastMoveDirection           = Vector3.right;

    Tween offsetTween;
    IAdvancedCameraTarget cachedTarget;


    // ===============================================================================

    public void Initialize(CameraContext context)
    {
        this.context = context;
    }

    // ===============================================================================

    public void Enable()
    {
        context.composer.Damping.x  = dampingX;
        context.composer.Damping.y  = dampingY;

        currentOffset               = context.composer.TargetOffset;
    }

    public void Tick(ICameraTarget instance)
    {
        AdvanceState(instance);
        TickHandler();
    }

    public void Disable()
    {
        context.composer.Damping.x  = 0;
        context.composer.Damping.y  = 0;
    }

    // ===============================================================================

    void TickHandler()
    {
        var desiredOffset = CalculateDesiredOffset();
        
        if (desiredOffset != targetOffset)
        {
            targetOffset = desiredOffset;
            ApplyOffset(desiredOffset);
        }
    }

        // ===================================
        //  State Management
        // ===================================

    void AdvanceState(ICameraTarget instance)
    {
        cachedTarget = instance as IAdvancedCameraTarget;

        if (cachedTarget.IsMoving)
        {
            StoreLastMoveDirection();
        }
    }

    void StoreLastMoveDirection()
    {
        var moveDir = cachedTarget.Velocity.normalized;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveDir;
        }
    }

        // ===================================
        //  General
        // ===================================

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

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    public bool IsValidTarget(ICameraTarget target)
    {
        return target is IAdvancedCameraTarget;
    }

    // ===============================================================================


    public void Dispose()
    {
        offsetTween?.Kill();
    }
}


    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                       Mouse Offset
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CameraMouseOffsetBehavior : ICameraBehavior
{
    InputRouter                     inputRouter;
    CameraContext                   context;

        // ---------------------------------

    readonly float dampingX         = .5f;
    readonly float dampingY         = .5f;

    readonly float followSpeedX     = 1f;
    readonly float followSpeedY     = 1f;
    readonly float maxOffsetX       = 2f;
    readonly float maxOffsetY       = 2f;
    readonly float maxDistance      = 10f;
    readonly Ease easing            = Ease.OutQuint;

    Vector3 targetOffset            = Vector3.zero;
    Vector3 desiredOffset           = Vector3.zero;

    Tween offsetTween;

    // ===============================================================================

    public void Initialize(CameraContext context)
    {
        this.context        = context;
        this.inputRouter    = Services.Get<InputRouter>();
    }

    // ===============================================================================

    public void Enable()
    {
        context.composer.Damping.x  = dampingX;
        context.composer.Damping.y  = dampingY;
    }

    public void Tick(ICameraTarget instance)
    {
        AdvanceState();
        TickHandler();
    }

    public void Disable()
    {
        context.composer.Damping.x  = 0;
        context.composer.Damping.y  = 0;
    }

    // ===============================================================================

    void AdvanceState()
    {
        Vector2 mouseWorldPos   = context.cameraRoot.ScreenToWorldPoint(inputRouter.MousePosition);
        Vector2 characterPos    = context.cameraTarget.position;
        Vector2 worldDelta      = mouseWorldPos - characterPos;
        Vector2 normalizedDelta = worldDelta / maxDistance;

        normalizedDelta = Vector2.ClampMagnitude(normalizedDelta, 1f);

        desiredOffset   = new Vector3(normalizedDelta.x * maxOffsetX, normalizedDelta.y * maxOffsetY, 0f);
    }

    void TickHandler()
    {
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

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    public bool IsValidTarget(ICameraTarget target)
    {
        return target is IAdvancedCameraTarget;
    }

    // ===============================================================================

    public void Dispose()
    {
        offsetTween?.Kill();
    }
}


    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                        Deadzone 
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class DeadzoneCameraBehavior : ICameraBehavior
{

    private enum DeadzoneState { Closed, Opening, Open, Shrinking }

        // ---------------------------------

    CameraContext                           context;

        // ---------------------------------

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
    IAdvancedCameraTarget cachedTarget;
    Vector2 currentSize;
    
    // ===============================================================================

    public void Initialize(CameraContext context)
    {
        this.context  = context;
    }

    // ===============================================================================

    public void Enable()
    {
        context.composer.Composition.DeadZone.Enabled = true;
        context.composer.Composition.DeadZone.Size    = movingSize;

        timer = new ClockWatch();
        timer.Start();

        state = DeadzoneState.Closed;
    }

    public void Tick(ICameraTarget instance)
    {
        AdvanceState(instance);
        TickHandler();
    }

    public void Disable()
    {
        context.composer.Composition.DeadZone.Enabled = false;
        timer.Reset();
    }

    // ===============================================================================

    void TickHandler()
    {
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

    void AdvanceState(ICameraTarget instance)
    {
        cachedTarget = instance as IAdvancedCameraTarget;
        currentSize  = context.composer.Composition.DeadZone.Size;
    }

    void ResetTimer()
    {
        timer = new ClockWatch();
        timer.Start();
    }

    Rect GetDeadzoneWorldBounds()
    {
        float camHeight     = context.camera.Lens.OrthographicSize * 2f;
        float camWidth      = camHeight * context.camera.Lens.Aspect;

        Vector2 screenSize  = new(camWidth, camHeight);
        Vector2 zoneSize    = Vector2.Scale(context.composer.Composition.DeadZone.Size, screenSize);

        Vector3 center      = context.camera.transform.position;
        Vector2 half        = zoneSize / 2f;
        Vector3 min         = center - (Vector3)half;

        return new Rect(min, zoneSize);
    }

    void StartSizeTween(Vector2 toSize, float speed, Ease ease)
    {
        sizeTween?.Kill();

        Vector2 currentSize = context.composer.Composition.DeadZone.Size;
        
        float distance      = Vector2.Distance(currentSize, toSize);
        float duration      = distance / Mathf.Max(speed, 0.01f);

        sizeTween = DOTween.To(() => context.composer.Composition.DeadZone.Size, x => context.composer.Composition.DeadZone.Size = x, toSize, duration).SetEase(ease);
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    public bool IsValidTarget(ICameraTarget target)
    {
        return target is IAdvancedCameraTarget;
    }

    bool ShouldOpenDeadzone()
    {
        return cachedTarget.Idle.Timer.CurrentTime > idleTimeThreshold && timer.CurrentTime > deadzoneTimeThreshold;
    }

    bool IsNearTarget(Vector2 current, Vector2 target)
    {
        return Vector2.Distance(current, target) <= sizeThreshold;
    }

    bool IsPlayerOutsideDeadzone()
    {
        var rect = GetDeadzoneWorldBounds();
        
        Vector3 offsetTarget = context.cameraTarget.position + context.composer.TargetOffset;
        Vector2 point        = offsetTarget;

        return !rect.Contains(point);
    }

    // ===============================================================================

    public void Dispose()
    {
        sizeTween?.Kill();
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations                                  
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CameraContext
{
    public CinemachineCamera                camera;
    public CinemachineBrain                 brain;
    public CinemachinePositionComposer      composer;
    public Camera                           cameraRoot;
    public PixelPerfectCamera               cameraPixel;
    public Transform                        cameraTarget;
}

public interface ICameraTarget
{
    public bool IsValid                     { get; }
    public Vector3 GetPosition();
}

public interface IAdvancedCameraTarget  : ICameraTarget
{
    public bool IsMoving                    { get; }
    public Vector2 Velocity                 { get; }
    public TimePredicate Idle               { get; }
}

public struct DefaultTargetProvider     : ICameraTarget
{
    public readonly bool IsValid            => true;
    public readonly Vector3 GetPosition()   => new();
}


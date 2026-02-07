using UnityEngine;



public enum MovementForce
{
    Kinematic,
    Dynamic
}

public enum KinematicAction
{
    Lunge,
    Charge,
    Dash,
}

public enum DynamicSource
{
    Collision
}

public enum ControllerMode
{
    Ignore,
    Blend,
    AllowOverride,
    Additive
}

public enum ControllerPriority
{
    Low         = 0,
    Normal      = 25,
    High        = 50,
    Interrupt   = 75,
    Forced      = 100
}

// ============================================================================
// BASE TYPES
// ============================================================================

public class MovementDefinition : Definition
{
    public MovementForce   MovementForce    { get; init; }
    public DynamicSource   DynamicSource    { get; init; }
    public KinematicAction KinematicAction  { get; init; }

    // DYNAMIC
    public Vector2 Force                    { get; init; }
    public float Mass                       { get; init; }
    
    // KINEMATIC
    public float Speed                      { get; init; }
    public AnimationCurve SpeedCurve        { get; init; }
    public int DurationFrames               { get; init; }

    // Source
    public WeaponPhase Phase                { get; init; }

    // Input
    public InputIntentSnapshot InputIntent  { get; set;  }

    // Config
    public int  Scope                       { get; set;  }
    public bool PersistPastScope            { get; init; }
    public bool PersistPastSource           { get; init; }

    public ControllerPriority Priority      { get; init; }
}

// ============================================================================
// DIRECTIVE
// ============================================================================

public class MovementDirective
{
    public object Owner                     { get; init; }
    public MovementDefinition Definition    { get; init; }
    public IMovementController Controller   { get; init; }
}


// ============================================================================
// CONTROLLERS
// ============================================================================


public interface IMovementController
{
    Vector2 CalculateVelocity(Actor actor);

    float Weight                            { get; }
    bool  Active                            { get; }

    ControllerMode Mode                     { get; }
    ControllerPriority Priority             { get; }
}

    // KINEMATIC

public class DashController : IMovementController
{
    readonly float speed;
    readonly Vector2 direction;
    readonly FrameTimer timer;

    public DashController(Vector2 direction, Vector2 lastDirection, float speed, int duration)
    {
        this.direction  = direction != Vector2.zero ? direction.normalized : lastDirection.normalized;
        this.speed      = speed;
        this.timer      = new FrameTimer(duration);
        timer.Start();
    }

    public Vector2 CalculateVelocity(Actor actor) => direction * speed;

    public float Weight                     => 1f;
    public bool  Active                     => !timer.IsFinished;
    public ControllerMode Mode              => ControllerMode.Ignore;
    public ControllerPriority Priority      => ControllerPriority.Interrupt;
}


public class LungeController : IMovementController
{
    readonly Vector2 direction;
    readonly float maxSpeed;
    readonly AnimationCurve speedCurve;
    readonly FrameTimer timer;
    
    public LungeController(Vector2 direction, float maxSpeed, int duration, AnimationCurve curve = null)
    {
        this.direction  = direction.normalized;
        this.maxSpeed   = maxSpeed;
        this.speedCurve = curve ?? AnimationCurve.EaseInOut(0, 1, 1, 0);
        this.timer      = new FrameTimer(duration);
        
        timer.Start();
    }
    
    public Vector2 CalculateVelocity(Actor actor)
    {
        float speedMultiplier = speedCurve.Evaluate(timer.PercentComplete);
        return maxSpeed * speedMultiplier * direction;
    }

    public float Weight                     => 1f;
    public bool  Active                     => !timer.IsFinished;
    public ControllerMode Mode              => ControllerMode.Ignore;
    public ControllerPriority Priority      => ControllerPriority.Normal;

}

    // DYNAMIC

public class DynamicForceController : IMovementController
{
    Vector2 currentVelocity;
    readonly float friction = Settings.Movement.FRICTION;

    public DynamicForceController(Vector2 force, float mass)
    {
        currentVelocity = force / mass;
    }

    public Vector2 CalculateVelocity(Actor actor)
    {
        currentVelocity *= Mathf.Exp(-friction * Clock.DeltaTime);
        return currentVelocity;
    }

    public float Weight                     => 1f;
    public bool  Active                     => currentVelocity.magnitude > 0.01f;
    public ControllerMode Mode              => ControllerMode.Additive;
    public ControllerPriority Priority      => ControllerPriority.Forced;
}



// REWORK REQUIRED WHEN TARGETING IMPLEMENTED


// public class ChargeController : IMovementController
// {
//     readonly Vector2 direction;
//     readonly float chargeSpeed;
//     readonly float releaseSpeed;
//     readonly ClockTimer chargeTimer;
//     readonly ClockTimer releaseTimer;
    
//     bool isCharging = true;
//     bool isReleased = false;
    
//     public ChargeController(Vector2 direction, float chargeSpeed, float releaseSpeed, float chargeDuration, float releaseDuration)
//     {
//         this.direction = direction.normalized;
//         this.chargeSpeed = chargeSpeed;
//         this.releaseSpeed = releaseSpeed;
//         this.chargeTimer = new ClockTimer(chargeDuration);
//         this.releaseTimer = new ClockTimer(releaseDuration);
//         chargeTimer.Start();
//     }
    
//     public void Release()
//     {
//         if (isCharging)
//         {
//             isCharging = false;
//             isReleased = true;
//             releaseTimer.Start();
//         }
//     }

    
//     public Vector2 CalculateVelocity(Actor actor)
//     {
//         if (isCharging)
//             return direction * chargeSpeed;
        
//         float t = releaseTimer.PercentComplete;
//         return direction * releaseSpeed * (1f - t);
//     }

//     public bool IsActive                    => isCharging || !releaseTimer.IsFinished;
//     public ControllerInputMode InputMode    => ControllerInputMode.Ignore;
//     public float Weight                     => 1f;
//     public ControllerPriority Priority      => ControllerPriority.Normal;
// }

// public class KnockbackController : IMovementController
// {
//     readonly Vector2 initialVelocity;
//     readonly float friction;
//     readonly ClockTimer timer;
//     Vector2 currentVelocity;
    
//     public KnockbackController(Vector2 direction, float force, float duration, float friction = 0.9f)
//     {
//         this.initialVelocity = direction.normalized * force;
//         this.currentVelocity = initialVelocity;
//         this.friction = friction;
//         this.timer = new ClockTimer(duration);
//         timer.Start();
//     }

    
//     public Vector2 CalculateVelocity(Actor actor)
//     {
//         currentVelocity *= friction;
//         return currentVelocity;
//     }

//     public bool IsActive                    => !timer.IsFinished && currentVelocity.sqrMagnitude > 0.01f;
//     public ControllerInputMode InputMode    => ControllerInputMode.Ignore;
//     public float Weight                     => 1f;
//     public ControllerPriority Priority      => ControllerPriority.Normal;
// }


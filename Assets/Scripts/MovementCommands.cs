using UnityEngine;





public enum MovementAction
{
    Lunge,
    Charge,
    Dash,
    // etc,
}

public enum ControllerInputMode
{
    Ignore,
    Blend,
    AllowOverride,
}

public enum ControllerPriority
{
    Normal      = 0,
    Interrupt   = 50,
    Forced      = 100
}

// ============================================================================
// MOVEMENT DIRECTIVE
// ============================================================================

public class MovementDirective
{
    public object Owner                             { get; init; }
    public int Scope                                { get; init; }
    public IMovementController Controller           { get; init; }
    public MovementCommandDefinition Definition     { get; init; }
}


public class MovementCommandDefinition  : Definition
{
    public MovementAction Action                    { get; init; }

    public float Speed                              { get; init; }
    public AnimationCurve SpeedCurve                { get; init; }
    public int DurationFrame                        { get; init; }
    public bool PersistPastScope                    { get; init; }
    public bool PersistPastSource                   { get; init; }

    public WeaponPhase Phase                        { get; init; }
}

// ============================================================================
// MOVEMENT COMMANDS
// ============================================================================



public abstract class MovementCommand
{
    public abstract IMovementController CreateController();
    public abstract MovementCommandDefinition GetDefinition();
}

public class DashMovementCommand : MovementCommand
{
    public MovementCommandDefinition Definition     { get; init; }
    public InputIntentSnapshot InputIntent          { get; init; }
    public override IMovementController CreateController()      => new DashController(InputIntent.Direction, InputIntent.LastDirection, Definition.Speed, Definition.DurationFrame);
    public override MovementCommandDefinition GetDefinition()   => Definition;
} 

public class LungeMovementCommand : MovementCommand
{
    public MovementCommandDefinition Definition     { get; init; }
    public InputIntentSnapshot InputIntent          { get; init; }
    public override IMovementController CreateController()     => new LungeController(InputIntent.AimDirection, Definition.Speed, Definition.DurationFrame, Definition.SpeedCurve);
    public override MovementCommandDefinition GetDefinition()  => Definition;
} 

// public class ChargeCommand : MovementCommand
// {
//     readonly IHasAim actor;
//     readonly MovementActionIntent intent;
//     public override IMovementController CreateController() => new ChargeController(actor.AimDirection, intent.Speed, intent.Duration, intent.SpeedCurve);
// } 

// ============================================================================
// MOVEMENT CONTROLLERS
// ============================================================================

public interface IMovementController
{
    bool IsActive                   { get; }
    ControllerInputMode InputMode   { get; }
    ControllerPriority  Priority    { get; }
    float Weight                    { get; }

    Vector2 CalculateVelocity(Actor actor);
}


public class DashController : IMovementController
{
    readonly Vector2 direction;
    readonly float speed;
    readonly FrameTimer timer;

    Vector2 traveledDistance;
    bool latch = false;

    public DashController(Vector2 direction, Vector2 lastDirection, float speed, int duration)
    {
        Debug.Log($"Creating controller {direction}, {speed}");
        this.direction  = direction != Vector2.zero ? direction.normalized : lastDirection.normalized;
        this.speed      = speed;
        this.timer      = new FrameTimer(duration);
        timer.Start();
    }

    public Vector2 CalculateVelocity(Actor actor)
    {
        if (!latch)
            traveledDistance = actor.Bridge.View.transform.position;
        
        latch = true;

        Log.Debug(LogSystem.Movement, LogCategory.State,  "Movement", "Controller.Dash.Distance",() => Vector2.Distance(traveledDistance, actor.Bridge.View.transform.position ));
        return direction * speed;
    }

    public bool IsActive                    => !timer.IsFinished;
    public ControllerInputMode InputMode    => ControllerInputMode.Ignore;
    public float Weight                     => 1f;
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

    public bool IsActive                    => !timer.IsFinished;
    public ControllerInputMode InputMode    => ControllerInputMode.Ignore;
    public float Weight                     => 1f;
    public ControllerPriority Priority      => ControllerPriority.Normal;

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


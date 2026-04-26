using UnityEngine;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
       
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public interface IMovementController
{
    MovementDefinition Definition           { get; }

    void Enter(Movement controller);
    Vector2 Process(Movement controller);
    void Exit(Movement controller);

    float Weight                            { get; }
    bool  Active                            { get; }
    ControllerMode Mode                     { get; }
    ControllerPriority Priority             { get; }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class MovementDefinition : Definition
{
    public KinematicAction KinematicAction  { get; init; }

    // KINEMATIC
    public float Speed                      { get; init; }
    public float ExitSpeed                  { get; init; }

    public AnimationCurve SpeedCurve        { get; init; }
    public int DurationFrames               { get; init; }

    // Source
    public WeaponPhase Phase                { get; init; }

    // Input
    public IntentSnapshot InputIntent  { get; set;  }

    // Config
    public int  Scope                       { get; set;  }
    public bool PersistPastScope            { get; init; }
    public bool PersistPastSource           { get; init; }

    public ControllerPriority Priority      { get; init; }
}

public class MovementDirective
{
    public object Owner                     { get; init; }
    public MovementDefinition Definition    { get; init; }
    public IMovementController Controller   { get; init; }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum KinematicAction
{
    Lunge,
    Charge,
    Dash,
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


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Behaviours
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                   Dash                                                  
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class DashController : IStateProcessor<Movement, Vector2>, IMovementController
{
    public MovementDefinition Definition          { get; init; }

    readonly float speed;
    readonly Vector2 direction;
    readonly FrameTimer timer;

    // ===============================================================================

    public DashController(MovementDefinition definition)
    {
        Definition = definition;
        direction  = Definition.InputIntent.Direction != Vector2.zero ? Definition.InputIntent.Direction.Vector.normalized : Definition.InputIntent.LastDirection.Vector.normalized;
        speed      = Definition.Speed;
        timer      = new FrameTimer(definition.DurationFrames);
    }

    // ===============================================================================
    
    public void Enter(Movement controller)
    {
        timer.Start();
    }

    public Vector2 Process(Movement controller)
    {
        return speed * direction.normalized;
    }

    public void Exit(Movement controller)
    {
        if (Definition.ExitSpeed > 0)
            controller.SetControlSpeed(Definition.ExitSpeed);
    }

    // ===============================================================================

    public float Weight                     => 1f;
    public bool  Active                     => !timer.IsFinished;
    public ControllerMode Mode              => ControllerMode.Ignore;
    public ControllerPriority Priority      => ControllerPriority.Interrupt;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Lunge                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LungeController : IStateProcessor<Movement, Vector2>, IMovementController
{
    public MovementDefinition Definition          { get; init; }

    readonly float          speed;
    readonly AnimationCurve speedCurve;
    readonly Vector2        direction;
    readonly FrameTimer     timer;

        // ===============================================================================

    public LungeController(MovementDefinition definition)
    {
        Definition  = definition;
        direction   = Definition.InputIntent.Aim.Vector.normalized;
        speed       = Definition.Speed;
        speedCurve  = Definition.SpeedCurve ?? AnimationCurve.EaseInOut(0, 1, 1, 0);
        timer       = new FrameTimer(Definition.DurationFrames);
    }
    
    // ===============================================================================

    public void Enter(Movement controller)
    {
        timer.Start();
    }

    public Vector2 Process(Movement controller)
    {
        return speed * speedCurve.Evaluate(timer.PercentComplete) * direction;
    }
    
    public void Exit(Movement controller)
    {
        
    }

    // ===============================================================================


    public float Weight                     => 1f;
    public bool  Active                     => !timer.IsFinished;
    public ControllerMode Mode              => ControllerMode.Ignore;
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


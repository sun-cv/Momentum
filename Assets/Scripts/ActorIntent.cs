using UnityEngine;



public class IntentSystem : ActorService
{
    readonly CommandSystem      command;
    readonly DirectionIntent    direction;
    readonly FacingIntent       facing; 
    readonly TargetingSystem    targeting;
    readonly AimingSystem       aiming;

    
        // -----------------------------------

    public class TargetingSystem
    {
        public TargetingSystem(IntentSystem intent)
        {

        }
    }

    public class AimingSystem
    {
        public Direction Aim;

        public AimingSystem(IntentSystem intent)
        {

        }
    }

   // ===============================================================================

    public IntentSystem(Actor owner) : base(owner)
    {
        command         = new(this);
        direction       = new(this);
        facing          = new(this);
        targeting       = new(this);
        aiming          = new(this);
    }

    // ===============================================================================

    public IntentSnapshot Snapshot()
    {
        return new()
        {
            Aim                 = aiming.Aim,
            Facing              = facing.Facing,
            Direction           = direction.Direction,
            LastDirection       = direction.LastDirection,
        };
    }

    public TargetingSystem Targeting    => targeting;
    public AimingSystem Aiming          => aiming;
    public FacingIntent Facing          => facing;
    public CommandSystem Command        => command;
    public DirectionIntent Direction    => direction;

    public UpdatePriority Priority      => ServiceUpdatePriority.IntentSystem;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct ActorAim : IMessage 
{
    public Vector2 Vector               { get; init; }

    public ActorAim(Vector2 vector)
    {
        Vector = vector;
    }
}

public readonly struct ActorMovement: IMessage 
{
    public Vector2 Vector               { get; init; }

    public ActorMovement(Vector2 vector)
    {
        Vector = vector;
    }
}


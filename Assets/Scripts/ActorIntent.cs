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

    public class AimingSystem : ActorService, IServiceTick
    {
        public Vector2 rawAim       = new();
        public Direction aim        = new(Vector2.down);

        public AimingSystem(IntentSystem intent) : base(intent.Owner)
        {
            owner.Bus.Link.Local<ActorAim>(WriteRawAim);
        }

        // ===============================================================================

        public void Tick()
        {
            UpdateAim();
        } 

        // ===============================================================================

        void UpdateAim()
        {
            aim = rawAim.normalized;
        }    

        // ===============================================================================
        //  Events
        // ===============================================================================

        void WriteRawAim(ActorAim message)
        {
            rawAim = message.Vector;
        }

        // ===============================================================================

        public Vector2 RawAim               => rawAim;
        public Direction Aim                => aim;

        public UpdatePriority Priority      => ServiceUpdatePriority.IntentSystem;
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

        Debug.Log($"Called Snapshot: {direction.Direction.Vector} Aim: {aiming.Aim.Vector}");

        return new()
        {
            Aim                 = aiming.Aim,
            Direction           = direction.Direction,
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


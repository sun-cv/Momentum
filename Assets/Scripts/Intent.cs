using System;
using System.Collections.Generic;
using UnityEngine;



public class IntentSystem : Service, IServiceTick
{
    readonly Actor          owner;

    readonly InputRouter    inputRouter;
    readonly WorldPosition  worldPosition;

    // -----------------------------------

    readonly InputIntent    input   = new();
    readonly CommandSystem  command = new();

    // SubSystems
    // Targeting    - intent not resolve
    
   // ===============================================================================

    public IntentSystem(Actor owner)
    {
        Services.Lane.Register(this);

        this.owner      = owner;
    
        inputRouter     = Services.Get<InputRouter>();
        worldPosition   = Services.Get<WorldPosition>();

        command .Initialize(this);
        input   .Initialize(this);

        owner.Emit.Link.LocalBinding<PresenceStateEvent>(HandlePresenceStateEvent);
    }

    // ===============================================================================

    public void Tick()
    {
        command .Tick();
        input   .Tick();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
    
    void HandlePresenceStateEvent(PresenceStateEvent message)
    {
        switch (message.State)
        {
            case Presence.State.Entering: Enable();  break;
            case Presence.State.Exiting:  Disable(); break;
            case Presence.State.Disposal: Dispose(); break;
        }
    }

    public override void Dispose()
    {
        input  .Dispose();
        command.Dispose();

        Services.Lane.Deregister(this);
    }

    public Actor         Owner          => owner;
    public CommandSystem Command        => command;
    public InputIntent   Input          => input;
    public InputRouter   InputRouter    => inputRouter;
    public WorldPosition Position       => worldPosition;

    public UpdatePriority Priority      => ServiceUpdatePriority.IntentSystem;
}


public class InputIntent : IDirectionSource, IDisposable
{
    readonly bool normalizeVelocity = Settings.Movement.NORMALIZE_VELOCITY;

        // -----------------------------------

    IntentSystem intent;

        // -----------------------------------

    readonly List<ForcedFacingEvent> queue = new();
    
    TimePredicate facingDiagonal;
        // -----------------------------------


    Direction aim               = new(Vector2.down);
    Direction facing            = new(Vector2.down);
    Direction direction         = new(Vector2.down);
    Direction lastDirection     = new(Vector2.down);
    Direction forcedDirection   = new(Vector2.zero);

    // ===============================================================================

    public void Initialize(IntentSystem intent)
    {
        this.intent         = intent;
        facingDiagonal      = new TimePredicate(TimerUnit.Time, () => IsMovingDiagonal());

        this.intent.Owner.Emit.Link.Local<ForcedFacingEvent>(HandleForcedFacingEvent);
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessQueue();

        UpdateDirection();
        UpdateFacing();
        UpdateAim();
    } 

    

    // ===============================================================================


    public void ProcessQueue()
    {
        foreach(var command in queue)
        {
            ProcessForcedDirection(command);
        }

        queue.Clear();
    }

    void ProcessForcedDirection(ForcedFacingEvent message)
    {
        forcedDirection = message.Type switch
        {
            Request.Set   => message.Direction,
            Request.Clear => Vector2.zero,
            _             => forcedDirection
        };
    }

    void UpdateDirection()
    {
        var input = intent.InputRouter.MovementDirection;

        if (input == Vector2.zero && direction != Vector2.zero)
            lastDirection = direction;

        if (normalizeVelocity && input.sqrMagnitude > 1f)
            input = input.normalized;

        direction = input;
    }

    void UpdateFacing()
    {
        if (forcedDirection.HasValue)
            return;
            
        if (direction.Vector.sqrMagnitude < 0.0001f)
            return;

        bool hasHorizontal  = Mathf.Abs(direction.X) > 0.01f;
        bool hasVertical    = Mathf.Abs(direction.Y) > 0.01f;

        if (!hasHorizontal || !hasVertical)
        {
            facing = direction.Cardinal;
            return;
        }

        bool facingHorizontal = Mathf.Abs(facing.X) > Mathf.Abs(facing.Y);

        if (!facingHorizontal)
        {
            float requiredDelay = Orientation.GetTurnDelay(facing, direction);

            if (facingDiagonal.Duration >= requiredDelay)
                facing = new Vector2(direction.X > 0 ? 1 : -1, 0);
        }
        else
            facing = new Vector2(direction.X > 0 ? 1 : -1, 0);
    }
    
    void UpdateAim()
    {
        aim = intent.Position.MouseDirectionFrom(intent.Owner.Bridge.View.transform.position);
    }

    bool IsMovingDiagonal()
    {
        return Mathf.Abs(direction.X) > 0.01f && Mathf.Abs(direction.Y) > 0.01f;
    }

    public InputIntentSnapshot Snapshot()
    {
        return new()
        {
            Aim                 = Aim,
            Facing              = Facing,
            Direction           = Direction,
            LastDirection       = LastDirection,
        };
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleForcedFacingEvent(ForcedFacingEvent message)
    {
        queue.Add(message);
    }

    // ===============================================================================

    public void Dispose()
    {
        facingDiagonal.Dispose();
    }

    
    public Direction Aim                => aim;
    public Direction Facing             => facing;
    public Direction Direction          => direction;
    public Direction LastDirection      => lastDirection;
    public Direction ForcedDirection    => forcedDirection;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IDirectionSource
{
    Direction Aim               { get; }
    Direction Facing            { get; }
    Direction Direction         { get; }
    Direction LastDirection     { get; }
}

public enum DirectionSource
{
    Aim,
    Facing,
    Direction,
    LastDirection
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Structs                                                   
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct Direction
{
    readonly Vector2 vector;

    public Direction(Vector2 vector)
    {
        this.vector = vector.sqrMagnitude > 0.0001f ? vector.normalized : Vector2.zero;
    }

    public Vector2 Vector                   => vector;
    public Vector2 Cardinal                 => Orientation.NormalizeVectorToCardinal(vector);
    public Vector2 Intercardinal            => Orientation.NormalizeVectorToIntercardinal(vector);

    public Cardinal AsCardinal              => Orientation.CardinalFrom(vector);
    public Intercardinal AsIntercardinal    => Orientation.IntercardinalFrom(vector);

    public float X                          => vector.x;
    public float Y                          => vector.y;
    public float Angle                      => Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

    public bool IsZero                      => vector.sqrMagnitude < 0.0001f;
    public bool HasValue                    => !IsZero;

    public static implicit operator Vector2(Direction direction)    => direction.vector;
    public static implicit operator Direction(Vector2 vector)       => new(vector);
}


public readonly struct InputIntentSnapshot : IDirectionSource
{
    public Direction Aim                { get; init; }
    public Direction Facing             { get; init; }
    public Direction Direction          { get; init; }
    public Direction LastDirection      { get; init; }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct ForcedFacingEvent : IMessage
{
    public Vector2 Direction            { get; init; }
    public Request Type                 { get; init; }

    public ForcedFacingEvent(Request type, Vector2 direction)
    {
        Direction = direction;
        Type      = type;
    }
}

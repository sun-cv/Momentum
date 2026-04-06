using System;
using System.Collections.Generic;
using UnityEngine;



public class InputIntent : ActorService, IServiceTick, IDirectionSource, IDisposable
{
    readonly bool normalizeVelocity = Settings.Movement.NORMALIZE_VELOCITY;

        // -----------------------------------

    readonly IntentSystem intent;

        // -----------------------------------

    readonly List<ForcedFacingEvent> queue = new();
    readonly TimePredicate facingDiagonal;

        // -----------------------------------

    Direction aim               = new(Vector2.down);
    Direction facing            = new(Vector2.down);
    Direction direction         = new(Vector2.down);
    Direction lastDirection     = new(Vector2.down);
    Direction forcedDirection   = new(Vector2.zero);

    // ===============================================================================

    public InputIntent(IntentSystem intent) : base(intent.Owner)
    {
        this.intent     = intent;
        facingDiagonal  = new TimePredicate(TimerUnit.Time, () => IsMovingDiagonal());

        owner.Bus.Link.Local<ForcedFacingEvent>(HandleForcedFacingEvent);
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

    protected override void OnDispose()
    {
        facingDiagonal.Dispose();
    }
    
    public Direction Aim                => aim;
    public Direction Facing             => facing;
    public Direction Direction          => direction;
    public Direction LastDirection      => lastDirection;
    public Direction ForcedDirection    => forcedDirection;

    public UpdatePriority Priority      => ServiceUpdatePriority.CommandSystem;
}


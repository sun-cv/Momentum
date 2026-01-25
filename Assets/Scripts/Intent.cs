using UnityEngine;





public class IntentSystem : IServiceTick
{
    Actor           owner;

    InputRouter     inputRouter;
    WorldPosition   worldPosition;

    CommandSystem   command     = new();
    InputIntent     input       = new();

    // SubSystems
    // Targeting    - intent not resolve

    public IntentSystem(Actor owner)
    {
        this.owner      = owner;
    
        inputRouter     = Services.Get<InputRouter>();
        worldPosition   = Services.Get<WorldPosition>();

        command .Initialize(this);
        input   .Initialize(this);

        GameTick.Register(this);
    }


    public void Tick()
    {
        command .Tick();
        input   .Tick();
    }

    public Actor         Owner          => owner;
    public CommandSystem Command        => command;
    public InputIntent   Input          => input;
    public InputRouter   InputRouter    => inputRouter;
    public WorldPosition Position       => worldPosition;

    public UpdatePriority Priority      => ServiceUpdatePriority.IntentSystem;
}


public class InputIntent : IDirectionSource
{
    readonly bool normalizeVelocity = Settings.Movement.NORMALIZE_VELOCITY;

    IntentSystem intent;

    Direction aim               = new(Vector2.down);
    Direction facing            = new(Vector2.down);
    Direction direction         = new(Vector2.down);
    Direction lastDirection     = new(Vector2.down);

    TimePredicate facingDiagonal;

    public void Initialize(IntentSystem intent)
    {
        this.intent         = intent;
        facingDiagonal      = new TimePredicate(() => IsMovingDiagonal());
    }

    public void Tick()
    {
        UpdateDirection();
        UpdateFacing();
        UpdateAim();
    } 

    void UpdateDirection()
    {
        var input = intent.InputRouter.RemoteMovementDirection;

        if (input == Vector2.zero && direction != Vector2.zero)
            lastDirection = direction;

        if (normalizeVelocity && input.Vector.sqrMagnitude > 1f)
            input.Vector = input.Vector.normalized;

        direction = input.Vector;
    }

    void UpdateFacing()
    {
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
            Aim                = Aim,
            Facing             = Facing,
            Direction          = Direction,
            LastDirection      = LastDirection,
        };
    }

    public Direction Aim                => aim;
    public Direction Facing             => facing;
    public Direction Direction          => direction;
    public Direction LastDirection      => lastDirection;
}


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

    public bool IsZero                      => vector.sqrMagnitude < 0.0001f;
    public float Angle                      => Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

    public float X                          => vector.x;
    public float Y                          => vector.y;

    public static implicit operator Vector2(Direction direction)    => direction.vector;
    public static implicit operator Direction(Vector2 vector)       => new(vector);
}


public interface IDirectionSource
{
    Direction Aim               { get; }
    Direction Facing            { get; }
    Direction Direction         { get; }
    Direction LastDirection     { get; }
}

public readonly struct InputIntentSnapshot
{
    public Direction Aim                { get; init; }
    public Direction Facing             { get; init; }
    public Direction Direction          { get; init; }
    public Direction LastDirection      { get; init; }
}
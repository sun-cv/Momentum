






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
        this.owner = owner;
    
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

    public UpdatePriority Priority  => ServiceUpdatePriority.IntentSystem;
}


public class InputIntent
{
    IntentSystem intent;

    Vector2 facing          = Vector2.down;
    Vector2 direction       = Vector2.down;
    Vector2 aimDirection    = Vector2.down;
    Vector2 lastDirection   = Vector2.down;

    TimePredicate facingDiagonal;

    readonly bool normalizeVelocity = Settings.Movement.NORMALIZE_VELOCITY;

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

    void UpdateAim()
    {
        aimDirection = intent.Position.MouseDirectionFrom(intent.Owner.Bridge.View.transform.position);
    }

    void UpdateDirection()
    {
        var input = intent.InputRouter.RemoteMovementDirection;

        if (input == Vector2.zero && direction != Vector2.zero)
            lastDirection = direction;

        if (normalizeVelocity && input.Vector.sqrMagnitude > 1f)
            input.Vector = input.Vector.normalized;

        direction = input;
    }

    void UpdateFacing()
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        bool hasHorizontal  = Mathf.Abs(direction.x) > 0.01f;
        bool hasVertical    = Mathf.Abs(direction.y) > 0.01f;

        if (!hasHorizontal || !hasVertical)
        {
            facing = Orientation.ToVector(CardinalFacing);
            return;
        }

        bool facingHorizontal = Mathf.Abs(facing.x) > Mathf.Abs(facing.y);

        if (!facingHorizontal)
        {
            float requiredDelay = Orientation.GetTurnDelay(facing, direction);

            if (facingDiagonal.Duration >= requiredDelay)
                facing = new Vector2(direction.x > 0 ? 1 : -1, 0);
        }
        else
            facing = new Vector2(direction.x > 0 ? 1 : -1, 0);
    }


    bool IsMovingDiagonal()
    {
        return Mathf.Abs(direction.x) > 0.01f && Mathf.Abs(direction.y) > 0.01f;
    }

    public InputIntentSnapshot Snapshot()
    {

        Debug.Log($"Snapshot {CardinalAimDirection}");
    
        return new()
        {
            Direction                   = Direction,
            AimDirection                = AimDirection,
            LastDirection               = LastDirection,
            CardinalFacing              = CardinalFacing,
            CardinalAimDirection        = CardinalAimDirection,
            IntercardinalAimDirection   = IntercardinalAimDirection,
        };
    }

    public Vector2 Facing                           => facing;
    public Vector2 Direction                        => direction;
    public Vector2 AimDirection                     => aimDirection;
    public Vector2 LastDirection                    => lastDirection;
    public Cardinal CardinalFacing                  => Orientation.Facing(direction);
    public Cardinal CardinalAimDirection            => Orientation.CardinalFromIntent(aimDirection);
    public Intercardinal IntercardinalAimDirection  => Orientation.IntercardinalFromIntent(aimDirection);
}



public readonly struct InputIntentSnapshot
{
    public Vector2 Direction                        { get; init; }
    public Vector2 AimDirection                     { get; init; }
    public Vector2 LastDirection                    { get; init; }
    public Cardinal CardinalFacing                  { get; init; }
    public Cardinal CardinalAimDirection            { get; init; }
    public Intercardinal IntercardinalAimDirection  { get; init; }
}
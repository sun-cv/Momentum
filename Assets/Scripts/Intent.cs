using UnityEngine;





public class IntentSystem : IServiceTick
{
    Actor           owner;
    InputRouter     input;
    WorldPosition   worldPosition;

    CommandSystem   command     = new();
    MovementIntent  movement    = new();

    // SubSystems
    // Targeting    - intent not resolve

    public IntentSystem(Actor owner)
    {
        this.owner = owner;
    
        input           = Services.Get<InputRouter>();
        worldPosition   = Services.Get<WorldPosition>();

        command .Initialize(this);
        movement.Initialize(this);

        GameTick.Register(this);
    }


    public void Tick()
    {
        Debug.Log("Tick");
        command .Tick();
        movement.Tick();
    }

    public Actor          Owner     => owner;
    public InputRouter    Input     => input;
    public CommandSystem  Command   => command;
    public MovementIntent Movement  => movement;
    public WorldPosition  Position  => worldPosition;

    public UpdatePriority Priority  => ServiceUpdatePriority.IntentSystem;
}


public class MovementIntent
{
    IntentSystem intent;

    Vector2 direction;
    Vector2 aimDirection;
    Vector2 lastDirection;

    readonly bool normalizeVelocity = Settings.Movement.NORMALIZE_VELOCITY;

    public void Initialize(IntentSystem intent)
    {
        this.intent = intent;
    }

    public void Tick()
    {
        UpdateDirection();
        UpdateAim();
    } 

    void UpdateAim()
    {
        Debug.Log(intent.Owner.Bridge.View.transform.position);
        Debug.Log(intent.Position);
        aimDirection = intent.Position.MouseDirectionFrom(intent.Owner.Bridge.View.transform.position);
    }

    void UpdateDirection()
    {
        var input = intent.Input.RemoteMovementDirection;

        if (input == Vector2.zero && direction != Vector2.zero)
            lastDirection = direction;

        if (normalizeVelocity && input.Vector.sqrMagnitude > 1f)
            input.Vector = input.Vector.normalized;

        direction = input;
    }

    public Vector2 Direction                        => direction;
    public Vector2 AimDirection                     => aimDirection;
    public Vector2 LastDirection                    => lastDirection;
    public CardinalDirection CardinalDirection      => Orientation.FromMovement(direction);
    public CardinalDirection CardinalAimDirection   => Orientation.FromIntentZone(aimDirection);
}
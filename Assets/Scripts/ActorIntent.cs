using UnityEngine;



public class IntentSystem : ActorService
{
    readonly InputRouter    inputRouter;
    readonly WorldPosition  worldPosition;

    // -----------------------------------

    readonly InputIntent    input;
    readonly CommandSystem  command;

    // SubSystems
    // Targeting    - intent not resolve
    
   // ===============================================================================

    public IntentSystem(Actor owner) : base(owner)
    {

        inputRouter     = Services.Get<InputRouter>();
        worldPosition   = Services.Get<WorldPosition>();

        command         = new(this);
        input           = new(this);

        this.owner.Bus.Link.LocalBinding<PresenceStateEvent>(HandlePresenceStateEvent);
    }

    // ===============================================================================

    public CommandSystem Command        => command;
    public InputIntent   Input          => input;
    public InputRouter   InputRouter    => inputRouter;
    public WorldPosition Position       => worldPosition;

    public UpdatePriority Priority      => ServiceUpdatePriority.IntentSystem;
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

using System;
using UnityEngine;



public class DirectionIntent : ActorService, IServiceTick, IDirectionSource
{

    Vector2 rawDirection;
    
        // -----------------------------------

    readonly TimePredicate diagonalTravel;

        // -----------------------------------

    Direction direction             = new(Vector2.down);
    Direction lastDirection         = new(Vector2.down);

    // ===============================================================================

    public DirectionIntent(IntentSystem intent) : base(intent.Owner) 
    {
        diagonalTravel = new TimePredicate(TimerUnit.Time, () => direction.IsDiagonal);

        owner.Bus.Link.Local<ActorMovement>(WriteRawDirection);
    }


    // ===============================================================================

    public void Tick()
    {
        UpdateLastDirection();
        UpdateDirection();
    } 

    // ===============================================================================

    void UpdateDirection()
    {
        direction = rawDirection.normalized;
    }    

    void UpdateLastDirection()
    {
        if (RawDirection.normalized == Vector2.zero && direction != Vector2.zero)
            lastDirection = direction;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
    
    void WriteRawDirection(ActorMovement message)
    {
        rawDirection = message.Vector;
    }

    // ===============================================================================

    public Vector2 RawDirection         => rawDirection;
    public Direction Direction          => direction;
    public Direction LastDirection      => lastDirection;
    public TimePredicate DiagonalTravel => diagonalTravel;
    public UpdatePriority Priority      => ServiceUpdatePriority.CommandSystem;
}





// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    
    // REWORK REQUIRED
public interface IAimSource
{
    Direction Aim               { get; }
}

public interface IDirectionSource
{
    Direction Direction         { get; }
    Direction LastDirection     { get; }
}

public interface IFacingSource
{
    Direction Facing            { get; }
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

    public bool HasValue                    => !IsZero;
    public bool IsZero                      => vector.sqrMagnitude < 0.0001f;
    public bool IsCardinal                  => HasValue && (Mathf.Abs(X) < 0.01f || Mathf.Abs(Y) < 0.01f);
    public bool IsDiagonal                  => HasValue &&  Mathf.Abs(X) > 0.01f && Mathf.Abs(Y) > 0.01f;

    public static implicit operator Vector2(Direction direction)    => direction.vector;
    public static implicit operator Direction(Vector2 vector)       => new(vector);
}


public readonly struct IntentSnapshot : IDirectionSource
{
    public Direction Aim                { get; init; }
    public Direction Facing             { get; init; }
    public Direction Direction          { get; init; }
    public Direction LastDirection      { get; init; }
}



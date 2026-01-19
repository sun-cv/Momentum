using Unity.Mathematics;
using UnityEngine;





public enum Cardinal
{
    North,
    South,
    East,
    West,
}

public enum Ordinal
{
    NortEast,
    NorthWest,
    SoutEast,
    SouthWest
}

public enum Intercardinal
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}



public static class Orientation
{
    public static Cardinal Facing(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return input.x > 0 ? Cardinal.East : Cardinal.West;
        else
            return input.y > 0 ? Cardinal.North : Cardinal.South;
    }

    public static Intercardinal FromMovement(Vector2 input)
    {
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return angle switch
        {
            >= 337.5f  or < 22.5f  => Intercardinal.East,
            >= 22.5f  and < 67.5f  => Intercardinal.NorthEast,
            >= 67.5f  and < 112.5f => Intercardinal.North,
            >= 112.5f and < 157.5f => Intercardinal.NorthWest,
            >= 157.5f and < 202.5f => Intercardinal.West,
            >= 202.5f and < 247.5f => Intercardinal.SouthWest,
            >= 247.5f and < 292.5f => Intercardinal.South,
            _ => Intercardinal.SouthEast
        };
    }

    public static Intercardinal FromVector(Vector2 direction)
    {
        return FromMovement(direction);
    }

    public static Vector2 ToVector(Cardinal direction)
    {
        return direction switch
        {
            Cardinal.North => Vector2.up,
            Cardinal.South => Vector2.down,
            Cardinal.East  => Vector2.right,
            Cardinal.West  => Vector2.left,
            _ => Vector2.zero
        };
    }

    public static Vector2 ToVector(Intercardinal direction)
    {
        return direction switch
        {
            Intercardinal.North     => Vector2.up,
            Intercardinal.NorthEast => new Vector2( 1,  1).normalized,
            Intercardinal.East      => Vector2.right,
            Intercardinal.SouthEast => new Vector2( 1, -1).normalized,
            Intercardinal.South     => Vector2.down,
            Intercardinal.SouthWest => new Vector2(-1, -1).normalized,
            Intercardinal.West      => Vector2.left,
            Intercardinal.NorthWest => new Vector2(-1,  1).normalized,
            _ => Vector2.zero
        };
    }

    public static Cardinal ToCardinal(Intercardinal direction)
    {
        return direction switch
        {
            Intercardinal.North     => Cardinal.North,
            Intercardinal.NorthEast => Cardinal.North,
            Intercardinal.East      => Cardinal.East,
            Intercardinal.SouthEast => Cardinal.East,
            Intercardinal.South     => Cardinal.South,
            Intercardinal.SouthWest => Cardinal.South,
            Intercardinal.West      => Cardinal.West,
            Intercardinal.NorthWest => Cardinal.North,
            _ => Cardinal.North
        };
    }
    
    public static Cardinal CardinalFromIntent(Vector2 position)
    {
        float angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
    
        return angle switch
        {
            >= 315f or < 45f   => Cardinal.East,
            >= 45f and < 135f  => Cardinal.North,
            >= 135f and < 225f => Cardinal.West,
            >= 225f and < 315f => Cardinal.South,
            _ => Cardinal.East
        };
    }

    public static Intercardinal IntercardinalFromIntent(Vector2 position)
    {
        float angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return angle switch
        {
            >= 337.5f  or < 22.5f  => Intercardinal.East,
            >= 22.5f  and < 67.5f  => Intercardinal.NorthEast,
            >= 67.5f  and < 112.5f => Intercardinal.North,
            >= 112.5f and < 157.5f => Intercardinal.NorthWest,
            >= 157.5f and < 202.5f => Intercardinal.West,
            >= 202.5f and < 247.5f => Intercardinal.SouthWest,
            >= 247.5f and < 292.5f => Intercardinal.South,
            _ => Intercardinal.SouthEast
        };
    }

    public static Quaternion ToRotation(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0, 0, angle);
    }

    public static float GetTurnDelay(Vector2 currentFacing, Vector2 newDirection)
    {
        if (currentFacing.y > 0.5f)
        {
            return newDirection.x > 0 
                ? Settings.Movement.FACING_CLOCKWISE
                : Settings.Movement.FACING_COUNTER_CLOCKWISE;
        }
        else if (currentFacing.y < -0.5f)
        {
            return newDirection.x < 0 
                ? Settings.Movement.FACING_CLOCKWISE
                : Settings.Movement.FACING_COUNTER_CLOCKWISE;
        }
        else if (currentFacing.x > 0.5f)
        {
            return newDirection.y < 0 
                ? Settings.Movement.FACING_CLOCKWISE
                : Settings.Movement.FACING_COUNTER_CLOCKWISE;
        }
        else if (currentFacing.x < -0.5f)
        {
            return newDirection.y > 0 
                ? Settings.Movement.FACING_CLOCKWISE
                : Settings.Movement.FACING_COUNTER_CLOCKWISE;
        }
        
        return Settings.Movement.FACING_SWITCH_DELAY;
    }
}


public class WorldPosition : RegisteredService, IBind
{
    public Camera Camera                { get; internal set; }
    public RemoteVector2 MousePosition  { get; internal set; }

    public override void Initialize() {}

    public Vector2 Mouse()
    {
        return Camera.ScreenToWorldPoint((Vector2)MousePosition);
    }

    public Vector2 DirectionTo(Vector2 to, Vector2 from)
    {
        return (to - from).normalized;
    }

    public Vector2 MouseDirectionTo(Vector2 position)
    {
        return DirectionTo(position, Mouse());
    }

    public Vector2 MouseDirectionFrom(Vector2 position)
    {
        return DirectionTo(Mouse(), position);
    }
    
    public Intercardinal MouseIntercardinalFrom(Vector2 position)
    {
        return Orientation.FromVector(MouseDirectionFrom(position));
    }

    public float DistanceToMouse(Vector2 position)
    {
        return Vector2.Distance(position, Mouse());
    }

    public void Bind()
    {
        Camera          = Services.Get<CameraRig>().Camera;
        MousePosition   = Services.Get<InputRouter>().RemoteMousePosition;
    }
}
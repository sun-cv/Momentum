using Unity.Mathematics;
using UnityEngine;





public enum CardinalDirection
{
    North,
    South,
    East,
    West,
}



public static class Direction
{
    
    public static CardinalDirection FromMovement(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return input.x > 0 ? CardinalDirection.East     : CardinalDirection.West;
        else
            return input.y > 0 ? CardinalDirection.North    : CardinalDirection.South;
    }

    public static CardinalDirection FromVector(Vector2 direction)
    {
        return FromMovement(direction);
    }

    public static Vector2 ToVector(CardinalDirection direction)
    {
        return direction switch
        {
            CardinalDirection.North => Vector2.up,
            CardinalDirection.South => Vector2.down,
            CardinalDirection.East  => Vector2.right,
            CardinalDirection.West  => Vector2.left,
            _ => Vector2.zero
        };
    }
    
    public static CardinalDirection FromIntent(Vector2 position)
    {
        float angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        float horizontalField   = Config.Input.INTENT_DEFAULT;
        float verticalField     = Config.Input.INTENT_DEFAULT;

        return angle switch
        {
            var direction when angle > 90f  - verticalField   && angle < 90f  + verticalField   => CardinalDirection.North,
            var direction when angle > 270f - verticalField   && angle < 270f + verticalField   => CardinalDirection.South,
            var direction when angle > 180f - horizontalField && angle < 180f + horizontalField => CardinalDirection.West,
            var direction => CardinalDirection.East,
        };
    }

    public static CardinalDirection FromIntentZone(Vector2 position)
    {
        float angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        float horizontalField   = Config.Input.INTENT_HORIZONTAL;
        float verticalField     = Config.Input.INTENT_VERTICAL;
        
        return angle switch
        {
            var direction when angle > 90f  - verticalField   && angle < 90f  + verticalField   => CardinalDirection.North,
            var direction when angle > 270f - verticalField   && angle < 270f + verticalField   => CardinalDirection.South,
            var direction when angle > 180f - horizontalField && angle < 180f + horizontalField => CardinalDirection.West,
            var direction => CardinalDirection.East,
        };
    }

    public static Quaternion ToRotation(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0, 0, angle);
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

    public Vector2 DirectionTo(Vector2 from, Vector2 to)
    {
        return (to - from).normalized;
    }

    public Vector2 MouseDirectionFrom(Vector2 position)
    {
        return DirectionTo(position, Mouse());
    }
    
    public CardinalDirection MouseCardinalFrom(Vector2 position)
    {
        return Direction.FromVector(position);
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
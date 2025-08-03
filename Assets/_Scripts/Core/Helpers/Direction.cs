using UnityEngine;

namespace Momentum
{

    public static class DirectionUtility
    {
        public static Vector2 GetDirectionVector(CardinalDirection direction)
        {
            return direction switch
            {
                CardinalDirection.North         => Vector2.up,
                CardinalDirection.South         => Vector2.down,
                CardinalDirection.East          => Vector2.right,
                CardinalDirection.West          => Vector2.left,
                _ => Vector2.zero,
            };
        }

        public static Vector2 GetDirectionVector(OrdinalDirection direction)
        {
            return direction switch
            {
                OrdinalDirection.NorthEast      => new Vector2(1,  1).normalized,
                OrdinalDirection.SouthEast      => new Vector2(1, -1).normalized,
                OrdinalDirection.SouthWest      => new Vector2(-1,-1).normalized,
                OrdinalDirection.NorthWest      => new Vector2(-1, 1).normalized,
                _ => Vector2.zero,
            };
        }

        public static Vector2 GetDirectionVector(PrincipalDirection direction)
        {
            return direction switch
            {
                PrincipalDirection.North        => Vector2.up,
                PrincipalDirection.South        => Vector2.down,
                PrincipalDirection.East         => Vector2.right,
                PrincipalDirection.West         => Vector2.left,
                PrincipalDirection.NorthEast    => new Vector2(1,  1).normalized,
                PrincipalDirection.SouthEast    => new Vector2(1, -1).normalized,
                PrincipalDirection.SouthWest    => new Vector2(-1,-1).normalized,
                PrincipalDirection.NorthWest    => new Vector2(-1, 1).normalized,
                _ => Vector2.zero,
            };
        }

        public static PrincipalDirection GetPrincipalDirection(Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

            angle = (450f - angle) % 360f;

            int index = Mathf.RoundToInt(angle / 45f) % 8;
            return (PrincipalDirection)index;
        }        
        
        public static CardinalDirection GetCardinalDirection(Vector2 input)
        {
            if (input == Vector2.zero)
            {
                return CardinalDirection.South;
            }

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? CardinalDirection.East : CardinalDirection.West;
            }
            else
            {
                return input.y > 0 ? CardinalDirection.North : CardinalDirection.South;
            }
        }
    }


}
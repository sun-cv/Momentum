using UnityEngine;

namespace Momentum
{

    public static class DirectionUtility
    {
        public static Vector2 GetDirectionVector(Cardinal direction)
        {
            return direction switch
            {
                Cardinal.North         => Vector2.up,
                Cardinal.South         => Vector2.down,
                Cardinal.East          => Vector2.right,
                Cardinal.West          => Vector2.left,
                _ => Vector2.zero,
            };
        }

        public static Vector2 GetDirectionVector(Ordinal direction)
        {
            return direction switch
            {
                Ordinal.NorthEast      => new Vector2(1,  1).normalized,
                Ordinal.SouthEast      => new Vector2(1, -1).normalized,
                Ordinal.SouthWest      => new Vector2(-1,-1).normalized,
                Ordinal.NorthWest      => new Vector2(-1, 1).normalized,
                _ => Vector2.zero,
            };
        }

        public static Vector2 GetDirectionVector(Principal direction)
        {
            return direction switch
            {
                Principal.North        => Vector2.up,
                Principal.South        => Vector2.down,
                Principal.East         => Vector2.right,
                Principal.West         => Vector2.left,
                Principal.NorthEast    => new Vector2(1,  1).normalized,
                Principal.SouthEast    => new Vector2(1, -1).normalized,
                Principal.SouthWest    => new Vector2(-1,-1).normalized,
                Principal.NorthWest    => new Vector2(-1, 1).normalized,
                _ => Vector2.zero,
            };
        }

        public static Principal GetPrincipal(Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

            angle = (450f - angle) % 360f;

            int index = Mathf.RoundToInt(angle / 45f) % 8;
            return (Principal)index;
        }        
        
        public static Cardinal GetCardinal(Vector2 input)
        {
            if (input == Vector2.zero)
            {
                return Cardinal.South;
            }

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Cardinal.East : Cardinal.West;
            }
            else
            {
                return input.y > 0 ? Cardinal.North : Cardinal.South;
            }
        }
    }


}
using UnityEngine;



namespace Game.Common
{

    public static class Orientation
    {

        public static Vector2 Cardinal(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return new Vector2(Mathf.Sign(direction.x), 0f);

            return new Vector2(0f, Mathf.Sign(direction.y));
        }

        public static float Angle(Vector2 direction)            => Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        public static Quaternion Rotation(Vector2 direction)    => Quaternion.Euler(0f, 0f, Angle(direction));
    }
}




namespace Game.Common
{
    public static partial class Config
    {
        static public class World
        {
            static public class Entity
            {
                public const int PoolCapacity   = 10;
            }
            static public class Component
            {
                public const int Capacity       = 10;
            }
            static public class Capability
            {
                public const int Capacity       = 10;
            }
            static public class Mask
            {
                public const int Capacity       = 10;
            }

            static public class Physics 
            {
                public const float Rest         = 0.01f;
                public const float Knockback    = 8;
            }
        }
    }
}

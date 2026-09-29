


using UnityEngine;

namespace Game.Common
{
    public static partial class Config
    {
        public static class Graphics
        {
            public static class Camera
            {
                public const int PPU                    = 16;
                public const float OrthographicSize     = 270/(PPU * 2f);
            }
            public static class Facing
            {
                public const float MinimumSpeed         = .01f;
                public const int   TurnDelay            = 3;
                public const float Axis                 = .01f;
                public const int   Clockwise            = 6;
                public const int   Counterclockwise     = 48;
            }
        }
    }
}

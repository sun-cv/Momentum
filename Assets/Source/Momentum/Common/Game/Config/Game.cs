


namespace Game.Common
{
    public static partial class Config
    {

        static public class Input
        {
            public const float ReleaseThreshold     = 300f;
        }

        static public class Command
        {
            public const int PressWindow            = 40;
            public const int ReleaseWindow          = 20;
        }

        static public class Physics 
        {
            public const float Rest                 = 0.01f;
            public const float Knockback            = 8;
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

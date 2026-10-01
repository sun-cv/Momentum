


namespace Game.Common
{
    public static partial class Config
    {
        public static class Engine
        {
            public static class Clock
            {
                public const int Rate       = 60;
                public const int Scale      = 1;
                public const float Delta    = 1f / Rate ;
                public const float MaxDelta = .25f;
            }

            public static class Tick
            {
                public const int Base       = 60;
                public const int Half       = 30;
                public const int Step       = 15;
                public const int Util       = 15;
                public const int Late       = 60;
            }

            public static class Camera
            {
                public const int PPU                    = 16;
                public const float OrthographicSize     = 270/(PPU * 2f);
            }

            public static class Capacity
            {
                public const int Pool       = 10;
                public const int Component  = 10;
                public const int Capability = 10;
                public const int Mask       = 10;

            }
        }
    }
}




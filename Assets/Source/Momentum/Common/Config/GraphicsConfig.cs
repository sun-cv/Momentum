


namespace Game.Common
{
    public static partial class Config
    {
        public static class Graphics
        {
            public static class Camera
            {
                public const int PPU                = 16;
                public const float OrthographicSize = 270/(PPU * 2f);
            }
            public static class Facing
            {
                public const float MinimumSpeed     = .01f;
                public const float TurnDelay        = .01f;
            }
        }
    }
}


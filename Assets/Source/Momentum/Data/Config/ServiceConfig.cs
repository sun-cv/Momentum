using Game.Common;
        


namespace Game.Content
{
    public static partial class Config
    {
        public static class Service
        {
            public static class LoggingController
            {
                public const TickPhase Phase    = TickPhase.System;
                public const int       Priority = 0;
            }
            public static class Dev
            {
                public const TickPhase Phase    = TickPhase.System;
                public const int       Priority = 10;
            }
        }
    }
}

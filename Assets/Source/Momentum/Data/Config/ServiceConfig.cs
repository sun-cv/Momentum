using Game.Common;
        


namespace Game.Data
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
        }
    }
}

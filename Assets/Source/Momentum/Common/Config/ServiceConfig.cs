using Game.Common;
        


namespace Game.Common
{
    public static partial class Config
    {
        public static class Service
        {
            public static class LoggingController
            {
                public const TickPhase Phase    = TickPhase.System;
                public const int       Priority = 00;
            }

            public static class Watch
            {
                public const TickPhase Phase    = TickPhase.System;
                public const int       Priority = 05;
            }

            public static class Timers
            {
                public const TickPhase Phase    = TickPhase.System;
                public const int       Priority = 10;
            }

            public static class Dev
            {
                public const TickPhase Phase    = TickPhase.System;
                public const int       Priority = 20;
            }

            public static class InputDriver
            {
                public const TickPhase Phase    = TickPhase.Input;
                public const int       Priority = 00;
            }

            public static class PlayerInputSystem
            {
                public const TickPhase Phase    = TickPhase.Input;
                public const int       Priority = 05;
            }

            public static class AiInputSystem
            {
                public const TickPhase Phase    = TickPhase.Input;
                public const int       Priority = 10;
            }

            public static class CapabilitySystem 
            {
                public const TickPhase Phase    = TickPhase.Logic;
                public const int       Priority = 00;
            }

            public static class CommandSystem
            {
                public const TickPhase Phase    = TickPhase.Logic;
                public const int       Priority = 05;
            }

            public static class MovementSystem
            {
                public const TickPhase Phase    = TickPhase.Physics;
                public const int       Priority = 10;
            }
        }
    }
}

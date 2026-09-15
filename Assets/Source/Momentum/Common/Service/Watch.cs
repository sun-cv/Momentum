


namespace Game.Common
{

    public class Watch : RegisteredService, IRealBase, IRealLate, IGameBase
    {
        void IRealBase.Tick()
        {
            Time.Real += Config.Engine.Clock.Delta;
            Tick.Real ++;
        }

        void IGameBase.Tick()
        {
            Time.Game += Config.Engine.Clock.Delta;
            Tick.Game ++;
        }

        void IRealLate.Tick()
        {
            Tick.Late ++;
        }

        public static class Time
        {
            public static float Real  { get; set; }
            public static float Game  { get; set; }
        }

        public static class Tick
        {
            public static int Real  { get; set; }
            public static int Game  { get; set; }
            public static int Late  { get; set; }
        }
    }
}

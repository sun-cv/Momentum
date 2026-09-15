


namespace Game.Common
{

    public class Watch : RegisteredService, IRealBase, IRealLate, IGameBase
    {

        public static int RealTick      { get; private set; }
        public static int GameTick      { get; private set; }
        public static int LateTick      { get; private set; }

        public static float RealTime    { get; private set; }
        public static float GameTime    { get; private set; }

        void IRealBase.Tick()
        {
            RealTime += Config.Engine.Clock.Delta;
            RealTick ++;
        }

        void IGameBase.Tick()
        {
            GameTime += Config.Engine.Clock.Delta;
            GameTick ++;
        }

        void IRealLate.Tick()
        {
            LateTick ++;
        }
    }
}

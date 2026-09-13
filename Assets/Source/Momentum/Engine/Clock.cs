using Game.Content;
using Game.Common;



namespace Game.Core
{

    public class Clock : IRealBase
    {
        private float scale                 = Config.Engine.Clock.Scale;

        private float realDelta             = Config.Engine.Clock.Delta;
        private float gameDelta             = Config.Engine.Clock.Delta;


        private float realTime;
        private float gameTime;

        private int   frame;

        internal Clock()
        {
            UnityEngine.Time.fixedDeltaTime = Delta;
        }

        public void Tick()
        {
            UpdateGameDelta();

            realTime  += realDelta;
            gameTime  += gameDelta;
        }

        public void Late()
        {
            frame++;
        }

        private void UpdateGameDelta()
        {
            gameDelta = realDelta * scale;
        }

        private void AdjustTimeScale(float value)
        {
            scale = value;
        }

        public float Delta          => Config.Engine.Clock.Delta;
        public float RealTime       => realTime;
        public float GameTime       => gameTime;
        public float RealDelta      => realDelta;
        public float GameDelta      => gameDelta;
        public int   Frame          => frame;
    }

    
}


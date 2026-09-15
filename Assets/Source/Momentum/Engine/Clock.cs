using Game.Common;



namespace Game.Core
{

    public class Clock : IRealBase
    {
        private float scale                 = Config.Engine.Clock.Scale;

        private float realDelta             = Config.Engine.Clock.Delta;
        private float gameDelta             = Config.Engine.Clock.Delta;


        internal Clock()
        {
            UnityEngine.Time.fixedDeltaTime = Delta;
        }

        public void Tick()
        {
            UpdateGameDelta();
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
        public float RealDelta      => realDelta;
        public float GameDelta      => gameDelta;
    }

    
}


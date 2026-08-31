using Game.Data;
using Game.Common;



namespace Game.Engine
{

    public class Clock : IRateBase
    {
        private readonly float delta    = 1f / Config.Engine.Clock.Rate;

        private float scale             = 1;
        private float time;
        private float scaledTime;

        internal Clock()
        {
            UnityEngine.Time.fixedDeltaTime = Delta;
        }

        public void Tick()
        {
            time        += Delta;
            scaledTime  += ScaledDelta;
        }

        public void AdjustTimeScale(float value)
        {
            scale = value;
        }

        public float Time           => time;
        public float ScaledTime     => scaledTime;
        public float Delta          => delta;
        public float ScaledDelta    => delta * scale;
    }
}


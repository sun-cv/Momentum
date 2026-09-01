using Game.Data;
using Game.Common;



namespace Game.Engine
{

    public class Clock : IRateBase
    {
        private readonly float delta        = 1f / Config.Engine.Clock.Rate;

        private float scale                 = 1;
        private float time;
        private float scaledTime;
        private int   frame;

        internal Clock()
        {
            UnityEngine.Time.fixedDeltaTime = Delta;
        }

        public void Tick()
        {
            time        += Delta;
            scaledTime  += ScaledDelta;
        }

        public void Late()
        {
            frame++;
        }

        public void AdjustTimeScale(float value)
        {
            scale = value;
        }

        public float Time           => time;
        public float Delta          => delta;
        public float UnscaledDelta  => UnityEngine.Time.unscaledDeltaTime;
        public float ScaledTime     => scaledTime;
        public float ScaledDelta    => delta * scale;
        public int   Frame          => frame;
    }
}


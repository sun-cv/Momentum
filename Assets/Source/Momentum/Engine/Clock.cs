using UnityEngine;

using Game.Common;



namespace Game.Core
{

    public class Clock
    {
        private float scale = Config.Engine.Clock.Scale;

        private float realDelta;
        private float gameDelta;

        public void Tick()
        {
            realDelta = Mathf.Min(Watch.Tick.UnscaledDelta, Config.Engine.Clock.MaxDelta);
            gameDelta = realDelta * scale;
        }

        private void AdjustTimeScale(float value)
        {
            scale = value;
        }

        public float RealDelta      => realDelta;
        public float GameDelta      => gameDelta;
    }
}


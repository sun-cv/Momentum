using UnityEngine;

namespace Momentum
{
    [System.Serializable]
    public class ImpulseChannelParams : MovementChannelParams
    {
        public Vector2 Direction    = Vector2.zero;
        public float Force          = 10f;
        public float Duration       = 0.25f;

        public AnimationCurve ForceCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    }
}
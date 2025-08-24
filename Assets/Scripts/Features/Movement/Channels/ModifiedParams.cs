

using UnityEngine;

namespace Momentum
{


    [System.Serializable]
    public class ModifiedChannelParams : MovementChannelParams
    {
        public float SpeedMultiplier        = 1f;
        public float MaxSpeed               = 0f;
        public bool IgnoreFriction          = false;
        public Vector2 OverrideDirection    = Vector2.zero;
    }


}
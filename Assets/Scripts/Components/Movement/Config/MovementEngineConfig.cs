

using System;
using UnityEngine;

namespace Momentum
{

    public interface IMovementEngineConfig
    {
        public float GroundFriction     { get; }
        public float MaxMovementSpeed   { get; }
        public float AccelerationRate   { get; }
        public float ImpulseDecay       { get; }
        public float MaxMomentum        { get; }
    }


    [CreateAssetMenu(fileName = "MovementEngineConfig", menuName = "Momentum/Component/Movement/EngineConfig")]
    public class MovementEngineConfig : ScriptableObject, IMovementEngineConfig
    {
        [SerializeField] private float groundFriction   = 50f;
        [SerializeField] private float maxMovementSpeed = 20f;
        [SerializeField] private float accelerationRate = 50f;
        [SerializeField] private float impulseDecay     = 10f;
        [SerializeField] private float maxMomentum      = 20f;

        public float GroundFriction     => groundFriction;
        public float MaxMovementSpeed   => maxMovementSpeed;
        public float AccelerationRate   => accelerationRate;
        public float ImpulseDecay       => impulseDecay;
        public float MaxMomentum        => maxMomentum;
    }
}


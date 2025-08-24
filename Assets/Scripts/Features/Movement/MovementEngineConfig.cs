

using System;
using UnityEngine;

namespace Momentum
{

    public interface IMovementEngineConfig
    {
        public float GroundFriction { get; }
        public float MaxSpeedInput  { get; }
        public float AccelRate      { get; }
        public float ImpulseDecay   { get; }
        public float MomentumCap    { get; }
    }


    [CreateAssetMenu(fileName = "MovementEngineConfig", menuName = "ScriptableObjects/System/MovementEngineConfig")]
    public class MovementEngineConfig : ScriptableObject, IMovementEngineConfig
    {
        [SerializeField] private float groundFriction = 50f;
        [SerializeField] private float maxSpeedInput  = 20f;
        [SerializeField] private float accelRate      = 50f;
        [SerializeField] private float impulseDecay   = 10f;
        [SerializeField] private float momentumCap    = 20f;

        public float GroundFriction => groundFriction;
        public float MaxSpeedInput  => maxSpeedInput;
        public float AccelRate      => accelRate;
        public float ImpulseDecay   => impulseDecay;
        public float MomentumCap    => momentumCap;
    }
}


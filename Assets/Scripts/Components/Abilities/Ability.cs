using System;
using System.Collections.Generic;
using UnityEngine;

namespace Momentum.Abilities
{
    
    [Serializable]
    public struct ArbitrationSettings
    {
        [Range(0f, 100f)] public int priority;
        public Mode        mode;
        public List<Token> tokens;
        public bool        preemptable;
        public bool        preemptReserved;
    }

    [Serializable]
    public struct CastingSettings
    {
        public bool requiresState; 
        public State requiredState; 
        [SerializeReference]
        public List<AbilityPredicate> predicates;
    }

    [Serializable]
    public struct BufferSettings
    {
        public bool bufferable;
        [Range(0f, 1f)] public float input;
        [Range(0f, 1f)] public float eligible;
        [Range(0f, 1f)] public float expiration;
    }

    [Serializable]
    public struct RuntimeSettings
    {
        public bool cancellable;
        [Range(0f, 10f)]public float minimumRuntime;
        public bool interruptible;
        [Range(0f, 10f)]public float lingerDuration;
        public bool becomeConcurrent;
    }


    [CreateAssetMenu(fileName = "Ability", menuName = "Momentum/Entity/Ability")]
    public class Ability : ScriptableObject
    {
        [Header("Ability Identity")]
        public string id;

        [Header("Ability Arbitration")]
        public ArbitrationSettings arbitration;

        [Header("Casting Conditions")]
        public CastingSettings casting;

        [Header("Buffering Rules")]
        public BufferSettings buffer;

        [Header("Runtime Rules")]
        public RuntimeSettings runtime;

        [Header("Cooldown configuration")]
        public bool enableCooldown                          = true;
        public Cooldown cooldown;

        [Header("Casting")]
        public Execution execution;
        [Range(0f, 10f)] public float castTime              = 0f;
        [Header("Combos")]
        public bool enableCombo;
        // public List<ComboSequence> combos;

        [Header("Effects")]
        [SerializeReference]
        public List<Effect> effects;

        private void OnEnable() { if (string.IsNullOrEmpty(id)) id = name; effects ??= new List<Effect>(); }
    }
}



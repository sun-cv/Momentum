using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Momentum
{
    
    [Serializable]
    public struct AbilityRuntimeSettings
    {
        public bool cancellable;
        [Range(0f, 10f)]public float minimumRuntime;

        public bool interruptible;
        [Range(0f, 10f)]public float lingerDuration;

        public bool becomeConcurrent;
    }

    [Serializable]
    public struct AbilityQueueing
    {
        public bool bufferable;
        [Range(0f, 10f)] public float inputBuffer;
        [Range(0f, 10f)] public float expiration;
        [Range(0f, 10f)] public float validBuffer;
    }



    [CreateAssetMenu(fileName = "Ability", menuName = "Momentum/Entity/Ability")]
    public class Ability : ScriptableObject
    {
        [Header("Ability Identity")]
        public string id;

        [Header("Ability Classification")]
        [Range(0f, 100f)] public int priority              = 0;
        public AbilityCategory       category;
        public AbilityMode           mode;
        public List<AbilityCategory> overrideCategories;

        [Header("Casting Conditions")]
        public bool requiresState; 
        public AbilityState requiredState; 
        [SerializeReference]
        public List<AbilityPredicate> predicates;

        [Header("Queueing Rules")]
        public AbilityQueueing queueing;

        [Header("Runtime Rules")]
        public AbilityRuntimeSettings runtime;

        [Header("Cooldown configuration")]
        public bool enableCooldown                          = true;
        public Cooldown cooldown;

        [Header("Casting")]
        public AbilityExecution execution;
        [Range(0f, 10f)] public float castTime              = 0f;
        [Header("Combos")]
        public bool enableCombo;
        public List<ComboSequence> combos;

        [Header("Effects")]
        [SerializeReference]
        public List<AbilityEffect> effects;

        private void OnEnable() { if (string.IsNullOrEmpty(id)) id = name; effects ??= new List<AbilityEffect>(); }
    }
}

            // REWORK REQUIRED - Remove once moved internal.
        // public bool HasComboAtStep(int step) => combos != null && combos[step] != null;

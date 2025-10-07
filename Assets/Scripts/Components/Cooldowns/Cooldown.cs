using System;
using System.Collections.Generic;
using UnityEngine;
using Momentum.Abilities;


namespace Momentum
{
    [Serializable]
    public class Cooldown
    {
        public CooldownContext context;

        [SerializeReference]
        public List<CooldownRule> rules         = new();
        public List<IRuntimeCooldown> active    = new();

        public void Initialize(Ability ability)
        {
            context = new(ability);
            
            foreach (var rule in rules)
            {
                active.Add(rule.CreateRuntime(context));
            }
        }

        public void Trigger()
        {
            foreach (var rule in active)
            {
                rule.Trigger();
            }
        }

        public CooldownPhase GetPhase()
        {
            bool AnyBlocking    = false;
            bool AnyTracking    = false;
            bool AllIdle        = true;
            bool AllExpired     = true;

            foreach(var rule in active)
            {
                var phase = rule.GetPhase();
                if (phase == CooldownPhase.Blocking) AnyBlocking    = true;
                if (phase == CooldownPhase.Tracking) AnyTracking    = true;
                if (phase != CooldownPhase.Idle)     AllIdle        = false;
                if (phase != CooldownPhase.Expired)  AllExpired     = false;
            }

            if (AnyBlocking)    return CooldownPhase.Blocking;
            if (AnyTracking)    return CooldownPhase.Tracking;
            if (AllIdle)        return CooldownPhase.Idle;
            if (AllExpired)     return CooldownPhase.Expired;

            return CooldownPhase.Idle;
        }
    }





}
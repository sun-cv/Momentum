using System;
using System.Collections.Generic;
using System.Diagnostics;
using Momentum.Test;
using Unity.VisualScripting;
using Momentum.Abilities;

namespace Momentum.Abilities
{

    public class Cooldowns
    {
        private readonly Dictionary<Ability, Cooldown> cooldowns        = new();
        private readonly List<Ability> expired                          = new();
        private readonly Dictionary<Execution, Cooldown> global  = new();
        
        public void Resolve(Instance instance)
        {
            var ability  = instance.ability;
            var cooldown = instance.ability.cooldown;

            if (IsTracking(ability))
            {
                cooldowns[ability].Trigger();
                return;
            }    
            cooldown.Initialize(ability);
            cooldowns[ability] = cooldown;

            if (ability.execution == Execution.Instant)
            {
                var gcd = new Cooldown();
                // REWORK REQUIRED
                gcd.rules.Add(new FixedDurationRule(){ duration = 1});
                global.Add(Execution.Instant, gcd);
            }
        }

        public void AddGlobalCooldown(Mode model)
        {
            var cooldown = new Cooldown();
            cooldown.rules.Add(new FixedDurationRule(){ duration = 5});
            global.Add(Execution.Instant, cooldown);
        }

        public bool IsTracking(Ability ability)
        {
            if (cooldowns.TryGetValue(ability, out var existing))
            {
                if (existing.GetPhase() == CooldownPhase.Tracking)
                    return true;
            }
            return false;
        }

        public bool IsBlocking(Ability ability)
        {
            if (cooldowns.TryGetValue(ability, out var existing))
            {
                if (existing.GetPhase() == CooldownPhase.Blocking)
                    return true;
            }
            return false;
        }


        public void ProcessCooldowns()
        {
            if (cooldowns.Count == 0) return;

            foreach (var instance in cooldowns)
            {
                var cooldown = instance.Value;

                if (cooldown.GetPhase() == CooldownPhase.Expired)
                    expired.Add(instance.Key);
            }

            foreach (var key in expired)
                cooldowns.Remove(key);

            expired.Clear();
        }

        public bool IsActive(Ability ability)       => IsBlocking(ability);
        public bool GlobalActive(Ability ability)   => global.TryGetValue(ability.execution, out var cooldown);


    }


}
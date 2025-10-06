using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Momentum
{
    public class AbilityExecutionManager
    {
        private readonly List<AbilityExecutor> exclusiveExecutors   = new();
        private readonly List<AbilityExecutor> concurrentExecutors  = new();

        private readonly AbilityFactory  factory;
        private readonly AbilityCooldowns cooldowns;
        private readonly AbilityComboManager combos;

        public AbilityExecutionManager(AbilityFactory factory, AbilityCooldowns cooldowns, AbilityComboManager combos)
        {
            this.factory   = factory;
            this.cooldowns = cooldowns;
            this.combos    = combos;
        }


        public AbilityExecutor Execute(AbilityRequest request)
        {
            var executor = factory.CreateExecutor(request);

            if (request.cancelExecutorID != null)
                CancelExecutor(request.cancelExecutorID);

            if (request.ability.enableCombo && request.ability.combos.Count > 0)
                combos.Handle(executor);

            executor.Activate();

            if (executor.instance.ability.mode == AbilityMode.Exclusive)
                exclusiveExecutors.Add(executor);
            else
                concurrentExecutors.Add(executor);

            if (executor.instance.ability.enableCooldown)
                cooldowns.Resolve(executor.instance);

            return executor;
        }

        public void CancelExecutor(Guid id)
        {
            var executor = ExclusiveExecutors.Concat(concurrentExecutors).ToList().Find(executor => executor.meta.Id == id);
            if (combos.HasRegistered(executor))
                combos.Deregister(executor);

            executor.Cancel();
        }

        public void Tick()
        {
            TickList(exclusiveExecutors, true);
            TickList(concurrentExecutors, false);
        }

        private void TickList(List<AbilityExecutor> list, bool canTransitionToConcurrent)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var executor = list[i];
                executor.Tick();

                if (executor.IsCancelled)
                {
                    list.RemoveAt(i);
                    continue;
                }

                if (executor.IsComplete && !executor.CanAcceptChain())
                {
                    if (canTransitionToConcurrent && executor.ShouldBecomeConcurrent())
                    {
                        list.RemoveAt(i);
                        concurrentExecutors.Add(executor);
                    }
                    else
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }

    
        public bool HasActiveStateExecutor(AbilityState state) => Active.Any((executor) => executor.instance.ability.requiredState == state );
        public IEnumerable<AbilityExecutor> GetRunningExecutors(bool includeExclusive = true, bool includeConcurrent = true) => (includeExclusive  ? exclusiveExecutors  : Enumerable.Empty<AbilityExecutor>()).Concat(includeConcurrent ? concurrentExecutors : Enumerable.Empty<AbilityExecutor>());

        public IReadOnlyList<AbilityExecutor> ExclusiveExecutors  => exclusiveExecutors;
        public IReadOnlyList<AbilityExecutor> ConcurrentExecutors => concurrentExecutors;
        public IReadOnlyList<AbilityExecutor> Active              => ExclusiveExecutors.Concat(concurrentExecutors).ToList();


    }
}

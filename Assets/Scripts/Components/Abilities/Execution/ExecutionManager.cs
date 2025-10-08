using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Momentum.Abilities;

namespace Momentum.Abilities
{
    public class ExecutionManager
    {
        private readonly List<Executor> exclusiveExecutors   = new();
        private readonly List<Executor> concurrentExecutors  = new();

        Factory   factory;
        Arbiter   arbiter;
        Cooldowns cooldowns;

        public ExecutionManager(Factory factory)
        {
            this.factory   = factory;
            this.cooldowns = new();
        }

        public Guid Execute(Request request)
        {
            var executor = factory.Executor(this, request);

            switch(executor.instance.ability.arbitration.mode)
            {
                case Mode.Exclusive:  
                    exclusiveExecutors.Add(executor);
                    break;
                case Mode.Concurrent:
                    concurrentExecutors.Add(executor);
                    break;
            }

            if (executor.instance.ability.enableCooldown)
                cooldowns.Resolve(executor.instance);

            executor.Activate();

            return executor.meta.Id;
        }

        public void CancelExecutor(Guid id)
        {
            var executor = exclusiveExecutors.Concat(concurrentExecutors).ToList().Find(executor => executor.meta.Id == id);
            executor.Cancel();
        }

        public void InterruptExecutor(Guid id)
        {
            var executor = exclusiveExecutors.Concat(concurrentExecutors).ToList().Find(executor => executor.meta.Id == id);
            executor.Interrupt();
        }

        public void DeactivateExecutor(Guid id)
        {
            exclusiveExecutors .RemoveAll(executor => executor.meta.Id == id);
            concurrentExecutors.RemoveAll(executor => executor.meta.Id == id);

            arbiter.Release(id);
        }   

        public void Tick()
        {
            TickList(exclusiveExecutors, true);
            TickList(concurrentExecutors, false);
        }

        private void TickList(List<Executor> list, bool canTransitionToConcurrent)
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
            }
        }

    
        public bool HasActiveStateExecutor(State state) => Active.Any((executor) => executor.instance.ability.casting.requiredState == state );
        public IEnumerable<Executor> GetRunningExecutors(bool includeExclusive = true, bool includeConcurrent = true) => (includeExclusive  ? exclusiveExecutors  : Enumerable.Empty<Executor>()).Concat(includeConcurrent ? concurrentExecutors : Enumerable.Empty<Executor>());

        public IReadOnlyList<Executor> ExclusiveExecutors  => exclusiveExecutors;
        public IReadOnlyList<Executor> ConcurrentExecutors => concurrentExecutors;
        public IReadOnlyList<Executor> Active              => exclusiveExecutors.Concat(concurrentExecutors).ToList();


    }
}

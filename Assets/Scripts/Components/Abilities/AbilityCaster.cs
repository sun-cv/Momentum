

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Momentum
{

    public class AbilityCaster
    {

        private AbilityExecutionManager   manager;
        private Abilitypipeline pipeline;


        public AbilityCaster(Abilitypipeline pipeline, AbilityExecutionManager manager)
        {
            this.pipeline = pipeline;
            this.manager  = manager;
        }

            public void Cast() 
            {
                if(TryConsumeCast(out var request))
                    manager.Execute(request);    
            }

            public void Cast(AbilityState state) 
            {
                if (TryConsumeStateful(state, out var request))
                    manager.Execute(request);
            }

            public bool TryPeekInstant(out AbilityRequest request)
            {
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Instant, out request)) return true;
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Toggle, out request)) return true;
                request = null;
                return false;
            }
            public bool TryConsumeInstant(out AbilityRequest request)
            {
                request = null;
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Instant, out request))
                {
                    pipeline.Resolved.Execution.Remove(AbilityExecution.Instant);
                    return true;
                }

                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Toggle, out request))
                {
                    pipeline.Resolved.Execution.Remove(AbilityExecution.Toggle);
                    return true;
                }

                return false;
            }


            public bool TryPeekCast(out AbilityRequest request) 
            {
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Cast,    out request)) return true;
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Action,  out request)) return true;
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Instant, out request)) return true;
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Channel, out request)) return true;
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Toggle,  out request)) return true;
                
                request = null;
                return false;
            }

            public bool TryConsumeCast(out AbilityRequest request)
            {
                request = null;

                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Cast, out request))
                {
                    pipeline.Resolved.Execution.Remove(AbilityExecution.Cast);
                    return true;
                }
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Action, out request))
                {
                    pipeline.Resolved.Execution.Remove(AbilityExecution.Action);
                    return true;
                }                
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Instant, out request))
                {
                    pipeline.Resolved.Execution.Remove(AbilityExecution.Instant);
                    return true;
                }                
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Channel, out request))
                {
                    pipeline.Resolved.Execution.Remove(AbilityExecution.Channel);
                    return true;
                }                
                if (pipeline.Resolved.Execution.TryGetValue(AbilityExecution.Toggle, out request))
                {
                    pipeline.Resolved.Execution.Remove(AbilityExecution.Toggle);
                    return true;
                }
                return false;
            }

            public bool HasStateful(AbilityState state) => pipeline.Resolved.Execution.Values.Any(request => request.ability.requiredState == state);
            public bool HasStatefulRequest()            => pipeline.Resolved.Execution.Values.Any(request => request.ability.requiresState);
            public bool TryConsumeStateful(AbilityState state, out AbilityRequest request)
            {
                request = null;

                var (execution, abilityRequest) = pipeline.Resolved.Execution.First(resolved => resolved.Value.ability.requiredState == state);

                if (!pipeline.Resolved.Execution.TryGetValue(execution, out request))
                    return false;

                pipeline.Resolved.Execution.Remove(execution);
                return true;
            }

        public bool CastRequest         => TryPeekCast(out var instance);
        public bool StatefulCastRequest => TryPeekCast(out var instance) && HasStatefulRequest();
    }






}
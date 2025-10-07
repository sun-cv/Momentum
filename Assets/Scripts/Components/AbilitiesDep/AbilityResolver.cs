// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// namespace Momentum
// {



//     public class AbilityResolver
//     {
//         public void Resolve(IAbilityPipeline valid, IResolvedPipeline resolved, AbilityExecutionManager manager)
//         {
//             var result        = new List<AbilityRequest>();
//             var pending       = new List<AbilityRequest>();
//             var approvedSoFar = new List<AbilityRequest>();

//             var executionSlots = new Dictionary<AbilityExecution, AbilityRequest>();

//             int count = valid.Queue.Count;

//             for (int i = 0; i < count; i++)
//             {
//                 var candidate       = valid.Queue.Dequeue();
//                 var removeApproved  = new List<AbilityRequest>();
//                 bool blocked        = false;

//                 foreach (var approved in approvedSoFar)
//                 {
//                     if (candidate.ability.overrideCategories.Contains(approved.ability.category) && candidate.Priority > approved.Priority)
//                     {
//                         removeApproved.Add(approved);
//                         pending.Add(approved);
//                     }
//                     else if (candidate.ability.mode == AbilityMode.Exclusive && approved.ability.mode == AbilityMode.Exclusive && candidate.ability.category == approved.ability.category)
//                     {
//                         blocked = true;
//                     }
//                 }

//                 foreach (var resolvedRequest in resolved.AllRequests())
//                 {
//                     if (candidate.ability.overrideCategories.Contains(resolvedRequest.ability.category) && candidate.Priority > resolvedRequest.Priority)
//                     {
//                         removeApproved.Add(resolvedRequest);
//                         pending.Add(resolvedRequest);
//                     }
//                     else if (candidate.ability.mode == AbilityMode.Exclusive && resolvedRequest.ability.mode == AbilityMode.Exclusive && candidate.ability.category == resolvedRequest.ability.category)
//                     {
//                         blocked = true;
//                     }
//                 }

//                 foreach (var approved in removeApproved)
//                 {
//                     approvedSoFar.Remove(approved);
//                     result.Remove(approved);
//                     if (executionSlots.TryGetValue(approved.ability.execution, out var slot) && slot == approved)
//                         executionSlots.Remove(approved.ability.execution);
//                 }

//                 if (blocked)
//                     continue;

//                 if (executionSlots.TryGetValue(candidate.ability.execution, out var existingSlot))
//                 {
//                     if (candidate.Priority > existingSlot.Priority)
//                     {
//                         approvedSoFar.Remove(existingSlot);
//                         result.Remove(existingSlot);
//                         executionSlots[candidate.ability.execution] = candidate;
//                     }
//                     else
//                     {
//                         pending.Add(candidate);
//                         continue;
//                     }
//                 }
//                 else
//                 {
//                     executionSlots[candidate.ability.execution] = candidate;
//                 }

//                 approvedSoFar.Add(candidate);
//                 result.Add(candidate);

//                 foreach (var executor in manager.GetRunningExecutors())
//                 {
//                     var ability = executor.instance.ability;

//                     bool candidateCanOverride = candidate.ability.overrideCategories.Contains(ability.category);
//                     bool targetCancellable    = ability.runtime.cancellable && ability.execution != AbilityExecution.Toggle;
//                     bool executorCancellable  = executor.CanCancel();

//                     if (candidateCanOverride)
//                     {
//                         if (targetCancellable && executorCancellable)
//                         {
//                             candidate.cancelExecutorID = executor.meta.Id;
//                         }
//                         if (targetCancellable && !executorCancellable)
//                         {
//                             pending.Add(candidate);
//                             approvedSoFar.Remove(candidate);
//                             result.Remove(candidate);
//                             executionSlots.Remove(candidate.ability.execution);
//                         }
//                         continue;
//                     }

//                     if (candidate.ability.mode == AbilityMode.Exclusive && ability.mode == AbilityMode.Exclusive && candidate.ability.category == ability.category)
//                     {
//                         if (!(targetCancellable && executorCancellable))
//                         {
//                             pending.Add(candidate);
//                             approvedSoFar.Remove(candidate);
//                             result.Remove(candidate);
//                             executionSlots.Remove(candidate.ability.execution);
//                         }
//                     }
//                 }
//             }

//             foreach (var request in pending)
//             {   
//                 request.Meta.MarkPending();
//                 valid.Enqueue(request);
//             }
//             foreach (var approved in result)
//             {   
//                 approved.Meta.MarkResolved();
//                 resolved.Enqueue(approved);
//             }
//         }
//     }

//     public static class IAbilityPipelineExtensions
//     {
//         public static IEnumerable<AbilityRequest> AllRequests(this IResolvedPipeline pipeline)
//         {
//             foreach (var req in pipeline.Queue)     yield return req;
//             foreach (var req in pipeline.Execution) yield return req.Value;
//         }
//     }
// }

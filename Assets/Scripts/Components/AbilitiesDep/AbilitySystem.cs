// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using UnityEngine;
// using UnityEngine.PlayerLoop;
// using Momentum.Abilities;


// namespace Momentum
// {

//     public interface IAbilityEngine
//     {

//     }

//     public class AbilityEngine : IAbilityEngine
//     {
//         Router router;
//         Processor processor;


//         public void Cast(List<Ability> abilities)   {}
//         public void Cast(Ability ability)           {}



//     }

// }

// namespace Momentum.Abilities
// {




//     public class AbilityBUS
//     {
//         AbilityValidator validator;

//         public IPipeline inbound;
//         public IPipeline buffer;
//         public IPipeline eligible;
//         public IPipeline resolved;
//         public IPipeline pending;

//         public void Tick()
//         {

//         }


//         public class Inbound : PipelineBase
//         {
//             AbilityBUS BUS;

//             public Inbound(AbilityBUS BUS)
//             {
//                 this.BUS = BUS;
//             }

//             public void ProcessItem()
//             {

//             }
//         }

//         public class Buffer : IAbilityPipeline
//         {
//             AbilityBUS BUS;
//             GenericQueue<AbilityRequest> queue = new();

//             public Buffer(AbilityBUS BUS)
//             {
//                 this.BUS = BUS;
//             }

//             public void Enqueue(AbilityRequest request)
//             {
//                 queue.Enqueue(request);
//             }

//             public void Process()
//             {
//                 int count = queue.Count;

//                 for (int i = 0; i < count; i++)
//                 {
//                     var request = queue.Dequeue();

//                     if (BUS.validator.IsExpiredBuffer(request))
//                     {
//                         Expire(request);
//                     }
                    
//                     if (BUS.validator.IsEligible(request))
//                     {
//                         Accept(request);
//                     }
                    
//                     Enqueue(request);
//                 }
//             }

//             public GenericQueue<AbilityRequest> Queue => queue;
//             void Expire(AbilityRequest request) { request.Meta.MarkExpired(); }
//             void Accept(AbilityRequest request) { request.Meta.MarkEligible(); BUS.eligible.Enqueue(request); }
//         }
    
//         public class Accepted : IAbilityPipeline
//         {
//             AbilityBUS BUS;
//             GenericQueue<AbilityRequest> queue = new();

//             public Accepted(AbilityBUS BUS)
//             {
//                 this.BUS = BUS;
//             }

//             public void Enqueue(AbilityRequest request)
//             {
//                 queue.Enqueue(request);
//             }

//             public void Process()
//             {
//                 int count = queue.Count;

//                 for (int i = 0; i < count; i++)
//                 {
//                     var request = queue.Dequeue();

//                     if (BUS.validator.IsExpiredEligible(request))
//                     {
//                         Expire(request);
//                     }
                    
//                     Enqueue(request);
//                 }
//             }
//             public GenericQueue<AbilityRequest> Queue => queue;
//             void Expire(AbilityRequest request) { request.Meta.MarkExpired(); }
//         }

//         public class Resolved : IAbilityPipeline
//         {
//             AbilityBUS BUS;
//             GenericQueue<AbilityRequest> queue = new();

//             public Resolved(AbilityBUS BUS)
//             {
//                 this.BUS = BUS;
//             }

//             public void Enqueue(AbilityRequest request)
//             {
//                 queue.Enqueue(request);
//             }

//             public void Process()
//             {
//                 int count = queue.Count;

//                 for (int i = 0; i < count; i++)
//                 {
//                 }
//             }
//             public GenericQueue<AbilityRequest> Queue => queue;
//         }

//         public class Pending : IAbilityPipeline
//         {
//             AbilityBUS BUS;
//             GenericQueue<AbilityRequest> queue = new();

//             public Pending(AbilityBUS BUS)
//             {
//                 this.BUS = BUS;
//             }

//             public void Enqueue(AbilityRequest request)
//             {
//                 queue.Enqueue(request);
//             }

//             public void Process()
//             {
//                 int count = queue.Count;

//                 for (int i = 0; i < count; i++)
//                 {
//                 }
//             }
//             public GenericQueue<AbilityRequest> Queue => queue;
//         }



//     }
// }

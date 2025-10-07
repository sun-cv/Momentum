// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.AI;
// using UnityEngine.UIElements;

// namespace Momentum
// {

    
//     public interface IAbilityPipeline
//     {
//         public void Enqueue(AbilityRequest request);
//         public void Process();

//         public GenericQueue<AbilityRequest> Queue           { get;}
//     }

//     public interface IResolvedPipeline : IAbilityPipeline
//     {
//         public Dictionary<AbilityExecution, AbilityRequest>  Execution   { get; }
//     }

//     public class AbilityResolutionResult
//     {
//         public readonly List<AbilityRequest> Approved   = new();
//         public readonly List<AbilityRequest> Blocked    = new();
//         public readonly List<AbilityExecutor> ToCancel  = new();
//     }


//     public class Abilitypipeline
//     {
//         private AbilityValidator validator;
//         private AbilityResolver resolver;

//         private Action RequestCast;

//         private IAbilityPipeline inbound;
//         private IAbilityPipeline buffer;
//         private IAbilityPipeline valid;
//         private IAbilityPipeline resolved;

//         public Abilitypipeline(AbilityValidator validator)
//         {
//             this.validator = validator;

//             valid       = new ValidPipeline     (validator);
//             buffer      = new BufferPipeline    (validator, valid);
//             inbound     = new InboundPipeline   (validator, valid, buffer);
//             resolved    = new ResolvedPipeline  (validator);
//         }


//         public class InboundPipeline : IAbilityPipeline
//         {
//             private AbilityValidator validator;

//             private IAbilityPipeline valid;
//             private IAbilityPipeline buffer;

//             private readonly GenericQueue<AbilityRequest> queue;

//             public InboundPipeline(AbilityValidator validator, IAbilityPipeline valid, IAbilityPipeline buffer)
//             {   
//                 this.validator  = validator;
//                 this.valid      = valid;
//                 this.buffer     = buffer;
//                 this.queue      = new(1);
//             }

//             public void Enqueue(AbilityRequest request) => queue.Enqueue(request);

//             public void Process()
//             {
//                 var count = queue.Count;

//                 for (int i = 0; i < count; i++)
//                 {
//                     var instance = queue.Dequeue();

//                     if (validator.IsValid(instance))
//                     {   
//                         Validate(instance);
//                     }

//                     if (validator.IsBufferable(instance))
//                     {
//                         Buffer(instance);
//                     }
//                 }
//             }

//             public void Expire(AbilityRequest request)      { request.Meta.MarkExpired(); }
//             public void Validate(AbilityRequest request)    { request.Meta.MarkValidated(); valid.Enqueue(request);}
//             public void Buffer(AbilityRequest request)      { request.Meta.MarkBuffered(); buffer.Enqueue(request);}

//             public GenericQueue<AbilityRequest> Queue       => queue;
//         }

//         public class BufferPipeline : IAbilityPipeline
//         {
//             private AbilityValidator validator;
//             private IAbilityPipeline valid;

//             private readonly GenericQueue<AbilityRequest> queue;

//             public BufferPipeline(AbilityValidator validator, IAbilityPipeline valid)
//             {   
//                 this.validator  = validator;
//                 this.valid      = valid;
//                 this.queue      = new(1);
//             }

//             public void Enqueue(AbilityRequest request) => queue.Enqueue(request);

//             public void Process()
//             {
//                 var count = queue.Count;

//                 for (int i = 0; i < count; i++)
//                 {
//                     var instance = queue.Dequeue();

//                     if (validator.IsExpired(instance))
//                     {
//                         Expire(instance);
//                         continue;
//                     }

//                     if (validator.IsExpiredBuffer(instance))
//                     {
//                         Expire(instance);
//                         continue;
//                     }

//                     if (validator.IsValid(instance))
//                     {
//                         Validate(instance);
//                     }
//                     queue.Enqueue(instance);
//                 }
//             }

//             public void Expire(AbilityRequest request)      { request.Meta.MarkExpired(); }
//             public void Validate(AbilityRequest request)    { request.Meta.MarkValidated(); valid.Enqueue(request);}
//             public GenericQueue<AbilityRequest> Queue       => queue;
//         }


//         public class ValidPipeline : IAbilityPipeline
//         {
//             private AbilityValidator validator;

//             private readonly GenericQueue<AbilityRequest> queue;

//             public ValidPipeline(AbilityValidator validator)
//             {   
//                 this.validator  = validator;
//                 this.queue      = new(1);
//             }

//             public void Enqueue(AbilityRequest request) => queue.Enqueue(request);

//             public void Process()
//             {
//                 var count = queue.Count;

//                 for (int i = 0; i < count; i++)
//                 {
//                     var instance = queue.Dequeue();

//                     if (validator.IsExpired(instance))
//                     {
//                         Expire(instance);
//                         continue;
//                     }
//                     if (validator.IsExpiredValid(instance))
//                     {                        
//                         Expire(instance);
//                         continue;
//                     }

//                     queue.Enqueue(instance);
//                 }
//             }

//             public void Expire(AbilityRequest request)  { request.Meta.MarkExpired(); }
//             public GenericQueue<AbilityRequest> Queue   => queue;
//         }


//         public class ResolvedPipeline : IResolvedPipeline
//         {
//             private AbilityValidator validator;

//             readonly List<AbilityRequest> incoming = new();

//             private readonly GenericQueue<AbilityRequest> queue;
//             private readonly Dictionary<AbilityExecution, AbilityRequest>  executionMap;
//             private readonly Dictionary<AbilityExecution, AbilityRequest>  comboMap;

//             public ResolvedPipeline(AbilityValidator validator)
//             {
//                 this.validator  = validator;
//                 queue           = new GenericQueue<AbilityRequest>(1);
//                 executionMap    = new Dictionary<AbilityExecution, AbilityRequest>();
//             }

//             public void Enqueue(AbilityRequest request)
//             {
//                 queue.Enqueue(request);
//             }

//             public void Process()
//             {
//                 CleanResolved();

//                 while (queue.Count > 0)
//                 {
//                     var request = queue.Dequeue();

//                     if (request.isCombo)
//                         comboMap[request.ability.execution]     = request;
//                     else
//                         executionMap[request.ability.execution] = request;
//                 }
//             }
        

//             void CleanResolved()
//             {
//                 if (executionMap.TryGetValue(AbilityExecution.Cast,      out var Cinstance) && validator.IsExpiredValid(Cinstance)) Cinstance.Meta.MarkExpired(); executionMap.Remove(AbilityExecution.Cast);
//                 if (executionMap.TryGetValue(AbilityExecution.Action,    out var Ainstance) && validator.IsExpiredValid(Ainstance)) Ainstance.Meta.MarkExpired(); executionMap.Remove(AbilityExecution.Action);
//                 if (executionMap.TryGetValue(AbilityExecution.Instant,   out var Iinstance) && validator.IsExpiredValid(Iinstance)) Iinstance.Meta.MarkExpired(); executionMap.Remove(AbilityExecution.Instant);
//                 if (executionMap.TryGetValue(AbilityExecution.Channel,   out var Linstance) && validator.IsExpiredValid(Linstance)) Linstance.Meta.MarkExpired(); executionMap.Remove(AbilityExecution.Channel);
//                 if (executionMap.TryGetValue(AbilityExecution.Toggle,    out var Tinstance) && validator.IsExpiredValid(Tinstance)) Tinstance.Meta.MarkExpired(); executionMap.Remove(AbilityExecution.Toggle);
//             }

//             public GenericQueue<AbilityRequest>                 Queue       => queue;
//             public Dictionary<AbilityExecution, AbilityRequest> Execution   => executionMap;
//         }

//         public IAbilityPipeline Inbound             => inbound;
//         public IAbilityPipeline Buffer              => buffer;
//         public IAbilityPipeline Valid               => valid;
//         public IResolvedPipeline Resolved           => (IResolvedPipeline)resolved;
//     }

// }
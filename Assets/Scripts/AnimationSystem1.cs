// using System;
// using System.Collections.Generic;



// public class AnimationSystem1 : Service, IServiceLoop
// {
//     readonly Actor owner;
//     readonly AnimationDefinition animations;

//     // -----------------------------------

//     readonly AnimatorController1 animator;

//     // -----------------------------------

//     readonly List<Message<Request, AnimationRequestEvent>> pendingRequests = new();

//     // ===============================================================================

//     public AnimationSystem1(Actor actor)
//     {
//         if (!ValidateOwner(actor, out IDefined defined))
//             return;

//         Services.Lane.Register(this);

//         owner       = actor;
//         animations  = defined.Definition.Animations;

//         animator    = new(owner);

//         owner.Emit.Link.Local<Message<Request, AnimationRequestEvent>>  (HandleAnimationRequest);        
//         owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>     (HandlePresenceStateEvent);
//     }

//     // ===============================================================================

//     public void Loop()
//     {
//         ProcessAnimationRequests();
//     }

//     // ===============================================================================

//     void ProcessAnimationRequests()
//     {
//         if (pendingRequests.Count == 0)
//             return;

//         foreach (var message in pendingRequests)
//         {
//             var request = message.Payload.AnimationRequest;

//             if (!request.IsValid)
//                 Resolve(request);

//                 request.data = new AnimationData
//                 {
//                     Duration = animator.RequestAnimationDuration(request.name)
//                 };

//                 RequestAnimation(request);

//             owner.Emit.Local(message.Id, Response.Completed, new AnimationRequestEvent(request));
//         }

//         pendingRequests.Clear();
//     }
//     AnimationRequest Resolve(AnimationRequest request)
//     {
//         var set         = SelectAnimationSet(request.intent);
//         var animation   = SelectAnimation(set, request);
//         request.name    = animation;

//         return request;
//     }


//     AnimationSet SelectAnimationSet(AnimationIntent intent)
//     {
//         if (!IntentMap.Animations.TryGetValue(intent, out var selector))
//         {
//             Log.Error($"No animation mapping for intent {intent}");
//             return null;
//         }

//         return selector(animations);
//     }

//     string SelectAnimation(AnimationSet set, AnimationRequest request)
//     {

//         if (request.HasContext)
//         {
//             var contextual = ResolveContext(set, request.context);
//             if (contextual != null)
//             {
//                 return contextual;
//             }
//         }

//         if (set.Random?.Length > 0)
//             return set.Random[UnityEngine.Random.Range(0, set.Random.Length)];

//         return set.Default;
//     }


//     void RequestAnimation(AnimationRequest request)
//     {
//         animator.RequestAnimation(request);
//     }

//         // REWORK REQUIRED FOR CONTEXT
//     string ResolveContext(AnimationSet set, AnimationContext context)
//     {
//         return "";
//     }

//     // ===============================================================================
//     //  Events
//     // ===============================================================================

//     void HandleAnimationRequest(Message<Request, AnimationRequestEvent> message)
//     {
//         pendingRequests.Add(message);
//     }

//     void HandlePresenceStateEvent(Message<Publish, PresenceStateEvent> message)
//     {
//         switch(message.Payload.State)
//         {
//             case Presence.State.Entering:
//                 Enable();
//             break;
//             case Presence.State.Exiting:
//                 Disable();
//             break;
//             case Presence.State.Disposal:
//                 Dispose();
//             break;
//         }
//     }

//     // ===============================================================================

//     readonly Logger Log = Logging.For(LogSystem.Animation);

//     public override void Dispose()
//     {
//         Services.Lane.Deregister(this);
//     }

//     bool ValidateOwner(Actor actor, out IDefined defined)
//     {
//         defined = null;

//         if (actor is  not IDefined instance)
//         {
//             Log.Error($"{actor.GetType().Name} Failed Validation. Animation Controller requires defined actor");
//             return false;
//         }

//         defined = instance;
//         return true;
//     }

//     public UpdatePriority Priority      => ServiceUpdatePriority.AnimationSystem;
// }

//     // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//     //                                     Classes
//     // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// public class AnimationRequest
// {
//     public string           name;
//     public AnimationIntent  intent;
//     public AnimationContext context;
//     public AnimationOptions options;
//     public AnimationData    data;

//     public bool IsValid     => name     != null;
//     public bool HasContext  => context  != null;

//     public AnimationRequest(string name)
//     {
//         this.name       = name;
//         options         = new() { AllowInterrupt = true };
//     }   

//     public AnimationRequest(AnimationIntent intent, AnimationContext context = null)
//     {
//         this.intent     = intent;
//         this.context    = context;
//         options         = new() { AllowInterrupt = true };
//     }

//     public string           Name    => name;
//     public AnimationIntent  Intent  => intent;
//     public AnimationContext Context => context;
//     public AnimationOptions Options => options;
//     public AnimationData    Data    => data;
// }

// public class AnimationOptions
// {
//     public bool AllowInterrupt                      { get; init; }
// }

// public class AnimationContext
// {
//     public DamageComponent DamageComponent          { get; init; }
//     public InputIntentSnapshot InputIntent          { get; init; }
// };

// public class AnimationData
// {
//     public float Duration                           { get; set;}
// }

//     // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//     //                                      Enums
//     // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// public enum AnimationIntent
// {
//     Spawn,
//     Death,
//     Teleport,
//     Idle,
//     Interact,
//     Stagger,
//     Stun,
//     Frozen,
//     // etc
// }


// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                                         Events
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// public readonly struct AnimationRequestEvent
// {
//     public AnimationRequest AnimationRequest        { get; init; }

//     public AnimationRequestEvent(AnimationRequest request)
//     {
//         AnimationRequest = request;
//     }
// }

// public readonly struct AnimationTriggerEvent
// {    
//     public readonly string Trigger                  { get; init; }

//     public AnimationTriggerEvent(string trigger)
//     {
//         Trigger = trigger;   
//     }
// }

// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                                          Maps
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// public static partial class IntentMap
// {
//     public static readonly Dictionary<AnimationIntent, Func<AnimationDefinition, AnimationSet>> Animations = new()
//     {
//         { AnimationIntent.Spawn, definition => definition.Spawn },
//         { AnimationIntent.Death, definition => definition.Death },
//     };

// }
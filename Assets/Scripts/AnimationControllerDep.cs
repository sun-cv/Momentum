// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
//
//
// public class AnimationController : ActorService, IServiceTick, IServiceLoop, IDisposable
// {
//     const int LAYER_BASE    = 0;
//     const int LAYER_ACTION  = 1;
//
//     readonly int ANIMATION_STATE_DEFAULT = Animator.StringToHash("Idle");
//         // -----------------------------------
//
//     readonly Animator animator;
//     
//         // -----------------------------------
//
//     readonly List<int> validParameters;
//
//         // -----------------------------------
//
//     readonly List<AnimationAPI> queue;
//
//     readonly Dictionary<int, AnimatorParameter.Override> overrides;
//     readonly Dictionary<int, Action<Animator, Actor>> tickHandlers;
//     readonly Dictionary<int, Action<Animator, Actor>> loopHandlers;
//
//         // -----------------------------------
//
//     ClockTimer              transitionTimer;
//     AnimationAPI            animation;
//
//     bool playing            = false;
//     bool allowInterrupt     = false;
//     bool transitionTrigger  = false;
//     
//     // ===============================================================================
//
//     public AnimationController(Actor actor) : base(actor)
//     {
//         if (!IsValidOwner(actor))
//             return;
//
//         animator            = actor.Bridge.Animator;
//
//         validParameters     = new();
//
//         queue               = new();
//
//         overrides           = new();
//         tickHandlers        = new();
//         loopHandlers        = new();
//
//         CacheAnimatorParameters();
//
//         BuildHandlers();
//
//         Enable();
//     }
//
//     // ===============================================================================
//     //  Public API
//     // ===============================================================================
//
//     public void RequestAnimationAPI(AnimationAPI request)
//     {
//         queue.Add(request);
//     }
//
//     public void RequestAnimationTrigger(string trigger)
//     {
//         animator.SetTrigger(trigger);
//     }
//
//     // ===============================================================================
//
//     public void Tick()
//     {
//         ProcessHandlers(tickHandlers);
//         ProcessAnimationServices();
//         ProcessAnimationRequests();
//     }
//
//     public void Loop()
//     {
//         ProcessHandlers(loopHandlers);
//         DebugLog();
//     }
//
//     // ===============================================================================
//
//     void ProcessAnimationServices()
//     {
//         TransitionTimer();
//     }
//
//     void ProcessAnimationRequests()
//     {
//         if (queue.Count == 0)
//             return;
//
//         foreach (var request in queue)
//         {
//             Process(request);
//         }
//
//         queue.Clear();
//     }
//
//     void Process(AnimationAPI request)
//     {
//             switch(request.Request)
//             {
//                 case Request.Play: ProcessAnimation(request);   break;
//                 case Request.Stop: FinishAnimation(request);     break;
//             }
//     }
//
//
//         // ===================================
//         //  Animation Start
//         // ===================================
//
//     void ProcessAnimation(AnimationAPI request)
//     {
//         if (playing && !allowInterrupt)
//             return;
//
//         ClearTransitionTimer();
//
//         SetInterrupt(request);
//         SetOverrides(request);
//
//         PlayAnimation(request);
//
//         SendAnimationEvent(Publish.Started, request);
//     }
//
//     void SetInterrupt(AnimationAPI request)
//     {
//         allowInterrupt = request.Settings.AllowInterrupt;
//     }
//
//     void SetOverrides(AnimationAPI request)
//     {
//         overrides.Clear();
//
//         if (request.HasOverrides)
//         {
//             foreach (var handler in request.Data.Overrides)
//                 overrides[handler.Parameter] = handler;
//         }
//     }
//     
//     void PlayAnimation(AnimationAPI request)
//     {   
//         animator.SetLayerWeight(LAYER_ACTION, 1f);
//         animator.Play(request.Data.Animation, LAYER_ACTION, 0f);
//
//         playing         = true;
//         transitionTrigger  = true;
//         animation       = request;
//     }
//
//     void TransitionTimer()
//     {
//         if (!TransitionLatchTriggered())
//             return;
//             
//         if (IsHoldUntilRelease())
//             return;
//
//         transitionTimer = new ClockTimer(AnimationLength());
//         transitionTimer.OnTimerStop += () => FinishAnimation(animation);
//
//         transitionTimer.Start();
//     }
//
//     void ClearTransitionTimer()
//     {
//         transitionTimer?.Stop();
//     }
//
//         // ===================================
//         //  Animation Stop
//         // ===================================
//
//     void FinishAnimation(AnimationAPI request)
//     {
//         if (!IsStateActive(LAYER_ACTION, request.Data.Animation))
//             return;
//
//         if (request.Settings.HoldOnPlaybackEnd)
//             return;
//
//         SendAnimationEvent(Publish.Finished, request);
//         ClearAnimatorState();
//     }
//
//     void ClearAnimatorState()
//     {
//         playing         = false;
//         animation       = null;
//         allowInterrupt  = true;
//
//         animator.Play(ANIMATION_STATE_DEFAULT, LAYER_ACTION, 0f);
//         animator.SetLayerWeight(LAYER_ACTION, 0f);
//     }
//
//         // ===================================
//         //  Handlers
//         // ===================================
//
//     void ProcessHandlers(Dictionary<int, Action<Animator, Actor>> handlers)
//     {
//         foreach (var (param, handler) in handlers)
//         {
//             if (overrides.TryGetValue(param, out var overwrite))
//                 ApplyOverride(overwrite);
//             else
//                 handler(animator, owner);
//         }
//     }
//
//     void ApplyOverride(AnimatorParameter.Override overrideHandler)
//     {
//         switch (overrideHandler.Type)
//         {
//             case AnimatorParameter.Override.ParamType.Float: animator.SetFloat  (overrideHandler.Parameter, overrideHandler.Float); break;
//             case AnimatorParameter.Override.ParamType.Bool:  animator.SetBool   (overrideHandler.Parameter, overrideHandler.Bool);  break;
//             case AnimatorParameter.Override.ParamType.Int:   animator.SetInteger(overrideHandler.Parameter, overrideHandler.Int);   break;
//         }
//     }
//
//     // ===============================================================================
//     //  Helpers
//     // ===============================================================================
//
//     void CacheAnimatorParameters()
//     {
//         foreach (var param in animator.parameters)
//         {
//             if (AnimatorParameter.Library.Keys.Contains(param.name))
//                 validParameters.Add(AnimatorParameter.Library[param.name]);
//         }
//     }
//
//     void BuildHandlers()
//     {
//         foreach (var (type, entries) in AnimatorParameter.Handlers)
//         {
//             if (!type.IsAssignableFrom(owner.GetType()))
//                 continue;
//
//             foreach (var entry in entries)
//             {
//                 if (!validParameters.Contains(entry.Parameter))
//                     continue;
//
//                 if (entry.Rate == ServiceRate.Tick)
//                     tickHandlers[entry.Parameter] = entry.Handler;
//                 
//                 if (entry.Rate == ServiceRate.Loop)
//                     loopHandlers[entry.Parameter] = entry.Handler;
//             }
//         }
//     }
//
//     float AnimationLength()
//     {
//         return animator.GetCurrentAnimatorStateInfo(LAYER_ACTION).length;
//     }
//     // string GetCurrentAnimationName(int layer)
//     // {
//     //     var clipInfo = animator.GetCurrentAnimatorClipInfo(layer);
//     //     return clipInfo.Length > 0 ? clipInfo[0].clip.name : null;
//     // }
//
//     // ===============================================================================
//     //  Events
//     // ===============================================================================
//
//     void SendAnimationEvent(Publish type, AnimationAPI request)
//     {
//         owner.Bus.Emit.Local(new AnimationEvent(type, request.Data.Animation));
//     }
//
//     // ===============================================================================
//     //  Predicates
//     // ===============================================================================
//
//     bool TransitionLatchTriggered()
//     {
//         if (!transitionTrigger)
//             return false;
//
//         transitionTrigger = false;
//         return true;
//     }
//
//     private bool IsHoldUntilRelease()
//     {
//          return animation.Settings.HoldUntilReleased;
//     }
//
//     bool IsStateActive(int layer, string stateName)
//     {
//         return animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
//     }
//
//     bool IsValidOwner(Actor actor)
//     {
//         if (actor.Bridge.Animator != null)
//             return true;
//
//         Log.Error($"{actor.GetType().Name} Failed System Validation. Animator Controller requires Animator assigned in Bridge");
//         return false;
//     }
//
//     // ===============================================================================
//
//     readonly Logger Log = Logging.For(LogSystem.Animation);
//
//     void DebugLog()
//     {
//         if (owner is not Hero hero)
//             return;
//
//         Log.Trace("Playing Action", () => 
//         { 
//             if (animator.GetLayerWeight(LAYER_ACTION) < 1)
//                 return "None";
//
//             AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(LAYER_ACTION);
//             return string.Join(", ", clipInfo.Select(clip => clip.clip.name));
//         });
//         
//         Log.Trace("Playing Base", () => 
//         { 
//             AnimatorClipInfo[] baseClipInfo = animator.GetCurrentAnimatorClipInfo(LAYER_BASE);
//             return string.Join(", ", baseClipInfo.Select(clip => clip.clip.name));
//         });
//
//         Log.Trace("rateX", () => hero.ResolvedFacing.X);
//         Log.Trace("rateY", () => hero.ResolvedFacing.Y);
//         Log.Trace("overrides", () => overrides.Count());
//     }
//
//     public UpdatePriority Priority => ServiceUpdatePriority.AnimationController;
// }
//
//
//

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// public class AnimationController : ActorService, IServiceTick, IServiceLoop, IDisposable
// {
//     public enum State { Idle, Loading, Playing }
//
//         // -----------------------------------
//
//     const int LAYER_BASE                    = 0;
//     const int LAYER_ACTION                  = 1;
//
//     readonly int ANIMATION_STATE_DEFAULT    = Animator.StringToHash("Idle");
//
//         // -----------------------------------
//
//     readonly Animator animator;
//     
//         // -----------------------------------
//
//     readonly List<AnimationAPI> queue                               = new();
//     readonly List<int> validParameters                              = new();
//     readonly Dictionary<string, float> clipDurations                = new();
//
//     readonly Dictionary<int, AnimatorParameter.Override> overrides  = new();
//     readonly Dictionary<int, Action<Animator, Actor>> tickHandlers  = new();
//     readonly Dictionary<int, Action<Animator, Actor>> loopHandlers  = new();
//
//         // -----------------------------------
//
//     Animation animation;
//     AnimationAPI pending;
//
//     AnimationStateMachine stateMachine;
//
//     public Animation Animation => animation;
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
//         InitializeState();
//
//         CacheClipDurations();
//         CacheAnimatorParameters();
//         InitializeParameterHandlers();
//
//         Enable();
//     }
//
//     void InitializeState()
//     {
//         stateMachine = new(this);
//
//         stateMachine.Register(State.Idle,       new AnimationIdleState   (stateMachine));
//         stateMachine.Register(State.Loading,    new AnimationLoadingState(stateMachine));
//         stateMachine.Register(State.Playing,    new AnimationPlayingState(stateMachine));
//         
//         stateMachine.Initialize(State.Idle);
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
//        animator.SetTrigger(trigger);
//     }
//
//     // ===============================================================================
//
//     public void Tick()
//     {
//         ProcessRequests();
//         AdvanceState();
//         ProcessState();
//         ProcessServices();
//     }
//
//     public void Loop()
//     {
//         ProcessSubServices();
//     }
//
//     // ===============================================================================
//
//     void ProcessServices()
//     {
//         UpdateParameters(tickHandlers);
//     }
//
//     void ProcessSubServices()
//     {
//         UpdateParameters(loopHandlers);
//         DebugLog();
//     }
//
//     // ===============================================================================
//
//     void AdvanceState()
//     {
//         if (pending == null) 
//             return;
//
//         switch (stateMachine.State)
//         {
//             case State.Idle:    stateMachine.TransitionTo(State.Loading);   break;
//             case State.Loading:                                             break;
//             case State.Playing: stateMachine.TransitionTo(State.Loading);   break;
//             default:            stateMachine.TransitionTo(State.Idle);      break;
//         }
//     }
//
//     // ===============================================================================
//
//     void ProcessState()
//     {
//         stateMachine.Update();
//     }
//
//     // ===============================================================================
//
//     void ProcessRequests()
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
//         switch(request.Request)
//         {
//             case Request.Play: RequestPlayAnimation(request); break;
//             case Request.Stop: RequestStopAnimation(request);  break;
//         }
//     }
//         // =================================
//         //  Start
//         // =================================
//
//     void RequestPlayAnimation(AnimationAPI request)
//     {
//         if (!CanPlayAnimation(request))
//             return;
//
//         SetPendingAnimation(request);
//     }
//
//         // =================================
//         //  Stop
//         // =================================
//
//     void RequestStopAnimation(AnimationAPI request)
//     {
//         if (!CanStopAnimation(request))
//             return;
//
//         stateMachine.TransitionTo(State.Idle);
//     }
//
//         // ===================================
//         //  Animation Load
//         // ===================================
//
//     public void LoadPlayback()
//     {
//         ClearAnimatorState();
//         BuildAnimationState();
//         SetAnimationPlayback(Animation.Playback.Loading);
//     }
//
//     private void BuildAnimationState()
//     {
//         SetOverrides(pending); 
//         animation = CreateAnimation(pending);
//         pending   = null;
//     }
//
//     void SetOverrides(AnimationAPI pending)
//     {
//         overrides.Clear();
//
//         if (pending.HasOverrides)
//         {
//             foreach (var handler in pending.Data.Overrides)
//                 overrides[handler.Parameter] = handler;
//         }
//     }
//
//         // ===================================
//         //  Animation Start
//         // ===================================
//
//     public void StartPlayback()
//     {
//         StartAnimationTimer();
//         PlayAnimation();
//         SetAnimationPlayback(Animation.Playback.Playing);
//     }
//
//     void PlayAnimation()
//     {   
//         animator.SetLayerWeight(LAYER_ACTION, 1f);
//         animator.Play(animation.Name, LAYER_ACTION, 0f);
//     }
//
//     public float AnimationLength(string name)
//     {
//         if (!clipDurations.TryGetValue(name, out var length))
//             return 0;
//
//         return length;
//     }
//         // ===================================
//         //  Animation Stop
//         // ===================================
//
//     public void StopPlayback()
//     {
//         if (!ResolveAnimationPlayback())
//             return;
//
//         ClearAnimatorState();
//     }
//
//     bool ResolveAnimationPlayback()
//     {
//         if (animation.Settings.HoldOnPlaybackEnd)
//         {
//             SetAnimationPlayback(Animation.Playback.Held);
//             return false;
//         }
//     
//         if (animation.Settings.HoldUntilReleased && pending == null)
//         {
//             SetAnimationPlayback(Animation.Playback.Completed);
//             return false;
//         }
//
//         if (animation.PlaybackTimer.IsFinished)
//         {
//             SetAnimationPlayback(Animation.Playback.Completed);
//             return true;
//         }
//
//         if (pending != null)
//         {
//             SetAnimationPlayback(Animation.Playback.Interrupted);
//             return true;
//         }
//
//         return true;
//     }
//
//     // ===================================
//     //  Animation Helpers
//     // ===================================
//
//     public void StartAnimationTimer()
//     {
//         if (animation.Settings.HoldUntilReleased)
//             return;
//
//         animation.PlaybackTimer = new(animation.Duration);
//         animation.PlaybackTimer.Start();
//     }
//
//     void ClearAnimatorState()
//     {
//         animation = null;
//
//         animator.Play(ANIMATION_STATE_DEFAULT, LAYER_ACTION, 0f);
//         animator.SetLayerWeight(LAYER_ACTION, 0f);
//     }
//
//     public void SetAnimationPlayback(Animation.Playback state)
//     {
//         animation.State = state; 
//         PublishAnimationState();
//     }
//         // ===================================
//         //  Handlers
//         // ===================================
//
//
//     void UpdateParameters(Dictionary<int, Action<Animator, Actor>> handlers)
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
//     Animation CreateAnimation(AnimationAPI request)
//     {
//         var duration = AnimationLength(request.Data.Animation);
//
//         return new Animation()
//         {
//             Name            = request.Data.Animation,
//             Duration        = duration,
//             Settings        = request.Settings,
//             PlaybackTimer   = new(duration),
//         };
//     }
//
//     void SetPendingAnimation(AnimationAPI request)
//     {
//         this.pending = request;
//     }
//
//     void CacheAnimatorParameters()
//     {
//         foreach (var param in animator.parameters)
//         {
//             if (!AnimatorParameter.Library.Keys.Contains(param.name))
//             {
//                 Log.Alert($"Animator parameter '{param.name}' is invalid for {owner.Definition.Name}.");
//                 continue;   
//             }
//
//             validParameters.Add(AnimatorParameter.Library[param.name]);
//         }
//     }
//
//     void CacheClipDurations()
//     {
//         foreach (var clip in animator.runtimeAnimatorController.animationClips)
//         {
//             string key = clip.name.Split('_')[0];
//             if (!clipDurations.ContainsKey(key))
//                 clipDurations[key] = clip.length;
//         }
//     }
//
//     void InitializeParameterHandlers()
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
//     // ===============================================================================
//     //  Events
//     // ===============================================================================
//
//     public void PublishState()
//     {
//         owner.Bus.Emit.Local(new AnimationControllerEvent(stateMachine.State));
//     }
//
//     public void PublishAnimationState()
//     {
//         owner.Bus.Emit.Local(new AnimationEvent(animation));
//     }
//
//     // ===============================================================================
//     //  Predicates
//     // ===============================================================================
//
//     bool CanPlayAnimation(AnimationAPI request)
//     {
//         if (stateMachine.Is(State.Idle))
//             return true;
//
//         if (!stateMachine.Is(State.Idle))
//             Debug.Log($"Animation state: {animation} State: {stateMachine.State}");
//
//         if (!animation.Settings.AllowInterrupt)
//             return false;
//
//         return true;
//     }
//
//     bool CanStopAnimation(AnimationAPI request)
//     {
//         if (stateMachine.Is(State.Idle))
//             return false;
//
//         if (!animation.Settings.AllowInterrupt)
//             return false;
//         
//         if (!IsAnimationActive(LAYER_ACTION, request.Data.Animation))
//             return false;
//
//         return true;
//     }
//
//     bool IsAnimationActive(int layer, string stateName)
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
//     public UpdatePriority Priority      => ServiceUpdatePriority.AnimationController;
// }
//
//
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                                      Declarations
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//         //                                 Classes                                                    
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//
// public class Animation
// {
//     public enum Playback { Loading, Playing, Looped, Interrupted, Held, Completed }
//
//     public string Name                      { get; set; }
//     public Playback State                   { get; set; }
//     public float Duration                   { get; set; }
//     public AnimationSettings Settings       { get; set; }
//     public ClockTimer PlaybackTimer         { get; set; }
// }
//
// public class AnimationStateMachine : StateMachine<AnimationController.State>
// {
//     readonly AnimationController controller;
//
//     public AnimationStateMachine(AnimationController controller) : base(controller.PublishState) 
//     {
//         this.controller = controller;
//     }
//
//     public AnimationController Controller => controller;
// }
//
// public class AnimationState : MachineState<AnimationController.State, AnimationStateMachine>
// {
//     public AnimationState(AnimationStateMachine machine) : base(machine) {}
// }
//
//
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                            AnimationController State Handlers                                       
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//         //                                  Idle
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//
// public class AnimationIdleState : AnimationState, IStateHandler
// {
//
//     // ===============================================================================
//
//     public AnimationIdleState(AnimationStateMachine machine) : base(machine)
//     {
//     }
//
//     // ===============================================================================
//
//     public void Enter()     {}
//     public void Update()    {}
//     public void Exit()      {}
//
//     // ===============================================================================
// }
//
//
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//         //                                 Loading 
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//
// public class AnimationLoadingState : AnimationState, IStateHandler
// {
//
//     // ===============================================================================
//
//     public AnimationLoadingState(AnimationStateMachine machine) : base(machine)
//     {
//     }
//
//     // ===============================================================================
//
//     public void Enter()     
//     {
//         machine.Controller.LoadPlayback();
//     }
//     
//     public void Update()
//     {
//         machine.TransitionTo(AnimationController.State.Playing);
//     }
//     
//     public void Exit() {}
//
//     // ===============================================================================
// }
//
//
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//         //                                 Playing 
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//
// public class AnimationPlayingState : AnimationState, IStateHandler
// {
//     // ===============================================================================
//
//     public AnimationPlayingState(AnimationStateMachine machine) : base(machine)
//     {
//     }
//
//     // ===============================================================================
//
//     public void Enter()
//     {
//         machine.Controller.StartPlayback();
//     }
//     
//     public void Update()
//     {
//
//         if (!ExitCondition())
//             return;
//
//         machine.TransitionTo(AnimationController.State.Idle);
//     }
//     
//     public void Exit()
//     {
//         machine.Controller.StopPlayback();
//     }
//     
//     bool ExitCondition()
//     {
//         if (machine.Controller.Animation.Settings.HoldUntilReleased)
//             return false;
//
//         if (machine.Controller.Animation.PlaybackTimer == null)
//             return true;
//
//         if (machine.Controller.Animation.PlaybackTimer.IsFinished)
//             return true;
//
//         return false;
//     }
//
//
//     // ===============================================================================
// }
//













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

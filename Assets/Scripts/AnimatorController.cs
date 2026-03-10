using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class AnimatorController : Service, IServiceTick, IServiceLoop, IServiceStep, IDisposable
{
    readonly Actor owner;
    readonly Animator animator;
    
        // -----------------------------------

    readonly Dictionary<string, float> clipDurations;
    readonly List<int> validParameters;

        // -----------------------------------

    readonly List<AnimationRequest> queue;

    readonly Dictionary<int, AnimatorParameter.Override> overrides;
    readonly Dictionary<int, Action<Animator, Actor>> tickHandlers;
    readonly Dictionary<int, Action<Animator, Actor>> loopHandlers;

        // -----------------------------------

    ClockTimer transitionTimer;

    bool playing            = false;
    bool allowInterrupt     = false;

    const int LAYER_BASE    = 0;
    const int LAYER_ACTION  = 1;

    // ===============================================================================

    public AnimatorController(Actor actor)
    {
        if (!ValidateOwner(actor))
            return;

        Services.Lane.Register(this);

        owner               = actor;
        animator            = actor.Bridge.Animator;

        validParameters     = new();
        clipDurations       = new();

        queue               = new();

        overrides           = new();
        tickHandlers        = new();
        loopHandlers        = new();

        CacheClipDurations();
        CacheAnimatorParameters();

        BuildHandlers();

        owner.Emit.Link.Local<PresenceStateEvent>     (HandlePresenceStateEvent);
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void RequestAnimationChange(AnimationRequest request)
    {
        queue.Add(request);
    }

    public void RequestAnimationTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    public float RequestAnimationDuration(string name)
    {
        return clipDurations[name];
    }

    // ===============================================================================

    public void Tick()
    {
        ProcessHandlers(tickHandlers);
        ProcessAnimatorRequests();
        DebugLog();
    }

    public void Loop()
    {
        ProcessHandlers(loopHandlers);
    }

    public void Step()
    {

    }

    // ===============================================================================

    void ProcessHandlers(Dictionary<int, Action<Animator, Actor>> handlers)
    {
        foreach (var (param, handler) in handlers)
        {
            if (overrides.TryGetValue(param, out var o))
                ApplyOverride(o);
            else
                handler(animator, owner);
        }
    }

    void ApplyOverride(AnimatorParameter.Override overrideHandler)
    {
        switch (overrideHandler.Type)
        {
            case AnimatorParameter.Override.ParamType.Float: animator.SetFloat  (overrideHandler.Parameter, overrideHandler.Float); break;
            case AnimatorParameter.Override.ParamType.Bool:  animator.SetBool   (overrideHandler.Parameter, overrideHandler.Bool);  break;
            case AnimatorParameter.Override.ParamType.Int:   animator.SetInteger(overrideHandler.Parameter, overrideHandler.Int);   break;
        }
    }

    void PlayAnimation(AnimationRequest request)
    {   
        animator.SetLayerWeight(LAYER_ACTION, 1f);
        animator.Play(request.data.Animation, LAYER_ACTION, 0f);

        playing = true;
    }

    void ProcessAnimation(AnimationRequest request)
    {
        if (playing && !allowInterrupt)
            return;

        ClearTransitionTimer();

        SetInterrupt(request);
        SetOverrides(request);

        PlayAnimation(request);

        SendAnimatorPlaybackEvent(Publish.Started, request);

        SetTransitionTimer(request);
    }

    void ProcessAnimatorRequests()
    {
        if (queue.Count == 0)
            return;

        foreach (var request in queue)
        {
            switch(request.options.Request)
            {
                case Request.Play: ProcessAnimation(request);   break;
                case Request.Stop: ClearAnimation(request);     break;
            }
        }

        queue.Clear();
    }

    void CacheClipDurations()
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            string key = clip.name.Split('_')[0];
            if (!clipDurations.ContainsKey(key))
                clipDurations[key] = clip.length;
        }
    }

    void CacheAnimatorParameters()
    {
        foreach (var param in animator.parameters)
        {
            if (AnimatorParameter.Library.Keys.Contains(param.name))
                validParameters.Add(AnimatorParameter.Library[param.name]);
        }
    }

    void BuildHandlers()
    {
        foreach (var (type, entries) in AnimatorParameter.Handlers)
        {
            if (!type.IsAssignableFrom(owner.GetType()))
                continue;

            foreach (var entry in entries)
            {
                if (!validParameters.Contains(entry.Parameter))
                    continue;

                if (entry.Rate == ServiceRate.Tick)
                    tickHandlers[entry.Parameter] = entry.Handler;
                
                if (entry.Rate == ServiceRate.Loop)
                    loopHandlers[entry.Parameter] = entry.Handler;
            }
        }
    }


    void SetInterrupt(AnimationRequest request)
    {
        allowInterrupt = request.options.AllowInterrupt;
    }

    void SetOverrides(AnimationRequest request)
    {
        overrides.Clear();

        if (request.HasOverrides)
        {
            foreach (var handler in request.overrides)
                overrides[handler.Parameter] = handler;
        }
    }

    void ClearTransitionTimer()
    {
        transitionTimer?.Stop();
    }

    void SetTransitionTimer(AnimationRequest request)
    {
        if (clipDurations.TryGetValue(request.data.Animation, out float duration))
        {
            transitionTimer = new ClockTimer(duration);
            transitionTimer.OnTimerStop += () => ClearAnimation(request);
            transitionTimer.Start();
        }
    }

    void ClearAnimation(AnimationRequest request)
    {
        if (!IsStateActive(LAYER_ACTION, request.data.Animation))
        {
            return;
        }
        ClearAnimatorState();
        
        SendAnimatorPlaybackEvent(Publish.Ended, request);
    }

    void ClearAnimatorState()
    {
        animator.Play("Idle", LAYER_ACTION, 0f);
        animator.SetLayerWeight(LAYER_ACTION, 0f);
        allowInterrupt = true;
    }

    bool IsStateActive(int layer, string stateName)
    {
        return animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
    }

    void SendAnimatorPlaybackEvent(Publish type, AnimationRequest request)
    {
        owner.Emit.Local(new AnimatorEvent(type, request.data.Animation));
    }

    string GetCurrentAnimationName(int layer)
    {
        var clipInfo = animator.GetCurrentAnimatorClipInfo(layer);
        return clipInfo.Length > 0 ? clipInfo[0].clip.name : null;
    }

    // ============================================================================
    //  Events
    // ============================================================================

    void HandlePresenceStateEvent(PresenceStateEvent message)
    {
        switch (message.State)
        {
            case Presence.State.Entering: Enable();  break;
            case Presence.State.Exiting:  Disable(); break;
            case Presence.State.Disposal: Dispose(); break;
        }
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Animation);

    void DebugLog()
    {
        if (owner is not Hero)
            return;

        Log.Trace("Playing Action", () => 
        { 
            if (animator.GetLayerWeight(LAYER_ACTION) < 1)
                return "None";

            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(LAYER_ACTION);
            return string.Join(", ", clipInfo.Select(clip => clip.clip.name));
        });
        
        Log.Trace("Playing Base", () => 
        { 
            AnimatorClipInfo[] baseClipInfo = animator.GetCurrentAnimatorClipInfo(LAYER_BASE);
            return string.Join(", ", baseClipInfo.Select(clip => clip.clip.name));
        });
    }

    bool ValidateOwner(Actor actor)
    {
        if (actor.Bridge.Animator == null)
        {
            Log.Error($"{actor.GetType().Name} Failed System Validation. Animator Controller requires Animator assigned in Bridge");
            return false;
        }

        return true;
    }


    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.AnimatorController;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct AnimatorEvent : IMessage
{
    public readonly string Name  { get; init; }
    public readonly Publish Type { get; init; }

    public AnimatorEvent(Publish type, string name)
    {
        Name = name;
        Type = type;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                          Maps
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public static class AnimatorParameter
{

    public struct Entry
    {
        public ServiceRate Rate;
        public int Parameter;
        public Action<Animator, Actor> Handler;
    }

    public struct Override
    {
        public enum ParamType { Float, Bool, Int }

        public int Parameter;
        public float Float;
        public bool Bool;
        public int Int;
        public ParamType Type;
    }

    public static Override Float(int param, float value) => new() { Parameter = param, Float = value, Type = Override.ParamType.Float };
    public static Override Bool (int param, bool value)  => new() { Parameter = param, Bool  = value, Type = Override.ParamType.Bool  };
    public static Override Int  (int param, int value)   => new() { Parameter = param, Int   = value, Type = Override.ParamType.Int   };

    public static int ResolvedAimX          = Animator.StringToHash(nameof(ResolvedAimX));
    public static int ResolvedAimY          = Animator.StringToHash(nameof(ResolvedAimY));
    public static int ResolvedCardinalAimX  = Animator.StringToHash(nameof(ResolvedCardinalAimX));
    public static int ResolvedCardinalAimY  = Animator.StringToHash(nameof(ResolvedCardinalAimY));
    public static int ResolvedFacingX       = Animator.StringToHash(nameof(ResolvedFacingX));
    public static int ResolvedFacingY       = Animator.StringToHash(nameof(ResolvedFacingY));
    public static int Alive                 = Animator.StringToHash(nameof(Alive));
    public static int Dead                  = Animator.StringToHash(nameof(Dead));
    public static int Inactive              = Animator.StringToHash(nameof(Inactive));
    public static int Disabled              = Animator.StringToHash(nameof(Disabled));
    public static int IsMoving              = Animator.StringToHash(nameof(IsMoving));
    public static int Idle                  = Animator.StringToHash(nameof(Idle));
    public static int IdleTime              = Animator.StringToHash(nameof(IdleTime));

    public static Dictionary<string, int> Library = new()
    {
        { nameof(ResolvedAimX),               ResolvedAimX          },
        { nameof(ResolvedAimY),               ResolvedAimY          },
        { nameof(ResolvedCardinalAimX),       ResolvedCardinalAimX  },
        { nameof(ResolvedCardinalAimY),       ResolvedCardinalAimY  },
        { nameof(ResolvedFacingX),            ResolvedFacingX       },
        { nameof(ResolvedFacingY),            ResolvedFacingY       },
        { nameof(Alive),                    Alive               },
        { nameof(Dead),                     Dead                },
        { nameof(Inactive),                 Inactive            },
        { nameof(Disabled),                 Disabled            },
        { nameof(IsMoving),                 IsMoving            },
        { nameof(Idle),                     Idle                },
        { nameof(IdleTime),                 IdleTime            },
    };

    public static readonly Dictionary<Type, Entry[]> Handlers = new()
    {
        { typeof(IAimable), new Entry[]
            {
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedAimX,          Handler = (animator, owner) => animator.SetFloat(ResolvedAimX,            ((IAimable)owner).ResolvedAim.X)          },
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedAimY,          Handler = (animator, owner) => animator.SetFloat(ResolvedAimY,            ((IAimable)owner).ResolvedAim.Y)          },
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedCardinalAimX,  Handler = (animator, owner) => animator.SetFloat(ResolvedCardinalAimX,    ((IAimable)owner).ResolvedAim.Cardinal.x) },
                new() { Rate = ServiceRate.Tick, Parameter = ResolvedCardinalAimY,  Handler = (animator, owner) => animator.SetFloat(ResolvedCardinalAimY,    ((IAimable)owner).ResolvedAim.Cardinal.y) },
            }
        },
        { typeof(IMovableActor), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = ResolvedFacingX,       Handler = (animator, owner) => animator.SetFloat(ResolvedFacingX,         ((IMovableActor)owner).ResolvedFacing.X)  },
                new() { Rate = ServiceRate.Loop, Parameter = ResolvedFacingY,       Handler = (animator, owner) => animator.SetFloat(ResolvedFacingY,         ((IMovableActor)owner).ResolvedFacing.Y)  },
                new() { Rate = ServiceRate.Loop, Parameter = Inactive,              Handler = (animator, owner) => animator.SetBool(Inactive,               ((IMovableActor)owner).Inactive)        },
                new() { Rate = ServiceRate.Loop, Parameter = Disabled,              Handler = (animator, owner) => animator.SetBool(Disabled,               ((IMovableActor)owner).Disabled)        },
                new() { Rate = ServiceRate.Loop, Parameter = IsMoving,              Handler = (animator, owner) => animator.SetBool(IsMoving,               ((IMovableActor)owner).IsMoving)        },
            }
        },
        { typeof(IMortal), new Entry[]
            {
                new() { Rate = ServiceRate.Tick, Parameter = Alive,                 Handler = (animator, owner) => animator.SetBool(Alive,                  ((IMortal)owner).Alive)                 },
                new() { Rate = ServiceRate.Tick, Parameter = Dead,                  Handler = (animator, owner) => animator.SetBool(Dead,                   ((IMortal)owner).Dead)                  },
            }
        },
        { typeof(IIdle), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = Idle,                  Handler = (animator, owner) => animator.SetBool(Idle,                   ((IIdle)owner).IsIdle)                  },
                new() { Rate = ServiceRate.Loop, Parameter = IdleTime,              Handler = (animator, owner) => animator.SetFloat(IdleTime,              ((IIdle)owner).IsIdle.Duration)         },
            }
        },
    };

    public static readonly Dictionary<Type, Func<InputIntentSnapshot, Override[]>> InputIntentSnapshot = new()
    {
        { typeof(IAimable), snapshot => new[]
            {
                Float(ResolvedAimX,         snapshot.Aim.X),
                Float(ResolvedAimY,         snapshot.Aim.Y),
                Float(ResolvedCardinalAimX, snapshot.Aim.Cardinal.x),
                Float(ResolvedCardinalAimY, snapshot.Aim.Cardinal.y),
            }
        },
        { typeof(IMovableActor), snapshot => new[]
            {
                Float(ResolvedFacingX, snapshot.Facing.X),
                Float(ResolvedFacingY, snapshot.Facing.Y),
            }
        },
    };
}


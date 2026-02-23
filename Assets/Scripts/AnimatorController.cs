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

    readonly List<AnimationRequest> animatorRequests;

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

        animatorRequests    = new();

        overrides           = new();
        tickHandlers        = new();
        loopHandlers        = new();

        CacheClipDurations();
        CacheAnimatorParameters();

        BuildHandlers();

        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>     (HandlePresenceStateEvent);
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void RequestAnimation(AnimationRequest request)
    {
        animatorRequests.Add(request);
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
    }

    public void Loop()
    {
        ProcessHandlers(loopHandlers);
    }

    public void Step()
    {
        DebugLog();
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
        animator.Play(request.name, LAYER_ACTION, 0f);

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

        SetTransitionTimer(request);
    }

    void ProcessAnimatorRequests()
    {
        if (animatorRequests.Count == 0)
            return;

        foreach (var request in animatorRequests)
        {
            ProcessAnimation(request);
        }

        animatorRequests.Clear();
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
        if (clipDurations.TryGetValue(request.name, out float duration))
        {
            transitionTimer = new ClockTimer(duration);
            transitionTimer.OnTimerStop += () => animator.SetLayerWeight(LAYER_ACTION, 0f);
            transitionTimer.OnTimerStop += () => allowInterrupt = true;
            transitionTimer.Start();
        }
    }

    // ============================================================================
    //  Events
    // ============================================================================

    void HandlePresenceStateEvent(Message<Publish, PresenceStateEvent> message)
    {
        switch(message.Payload.State)
        {
            case Presence.State.Entering:
                Enable();
            break;
            case Presence.State.Exiting:
                Disable();
            break;
            case Presence.State.Disposal:
                Dispose();
            break;
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

    public static int LockedAimX            = Animator.StringToHash(nameof(LockedAimX));
    public static int LockedAimY            = Animator.StringToHash(nameof(LockedAimY));
    public static int LockedCardinalAimX    = Animator.StringToHash(nameof(LockedCardinalAimX));
    public static int LockedCardinalAimY    = Animator.StringToHash(nameof(LockedCardinalAimY));
    public static int LockedFacingX         = Animator.StringToHash(nameof(LockedFacingX));
    public static int LockedFacingY         = Animator.StringToHash(nameof(LockedFacingY));
    public static int Alive                 = Animator.StringToHash(nameof(Alive));
    public static int Dead                  = Animator.StringToHash(nameof(Dead));
    public static int Inactive              = Animator.StringToHash(nameof(Inactive));
    public static int Disabled              = Animator.StringToHash(nameof(Disabled));
    public static int IsMoving              = Animator.StringToHash(nameof(IsMoving));
    public static int Idle                  = Animator.StringToHash(nameof(Idle));
    public static int IdleTime              = Animator.StringToHash(nameof(IdleTime));

    public static Dictionary<string, int> Library = new()
    {
        { nameof(LockedAimX),               LockedAimX          },
        { nameof(LockedAimY),               LockedAimY          },
        { nameof(LockedCardinalAimX),       LockedCardinalAimX  },
        { nameof(LockedCardinalAimY),       LockedCardinalAimY  },
        { nameof(LockedFacingX),            LockedFacingX       },
        { nameof(LockedFacingY),            LockedFacingY       },
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
                new() { Rate = ServiceRate.Tick, Parameter = LockedAimX,         Handler = (animator, owner) => animator.SetFloat(LockedAimX,            ((IAimable)owner).LockedAim.X)          },
                new() { Rate = ServiceRate.Tick, Parameter = LockedAimY,         Handler = (animator, owner) => animator.SetFloat(LockedAimY,            ((IAimable)owner).LockedAim.Y)          },
                new() { Rate = ServiceRate.Tick, Parameter = LockedCardinalAimX, Handler = (animator, owner) => animator.SetFloat(LockedCardinalAimX,    ((IAimable)owner).LockedAim.Cardinal.x) },
                new() { Rate = ServiceRate.Tick, Parameter = LockedCardinalAimY, Handler = (animator, owner) => animator.SetFloat(LockedCardinalAimY,    ((IAimable)owner).LockedAim.Cardinal.y) },
            }
        },
        { typeof(IMovableActor), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = LockedFacingX,      Handler = (animator, owner) => animator.SetFloat(LockedFacingX,         ((IMovableActor)owner).LockedFacing.X)  },
                new() { Rate = ServiceRate.Loop, Parameter = LockedFacingY,      Handler = (animator, owner) => animator.SetFloat(LockedFacingY,         ((IMovableActor)owner).LockedFacing.Y)  },
                new() { Rate = ServiceRate.Loop, Parameter = Inactive,           Handler = (animator, owner) => animator.SetBool(Inactive,               ((IMovableActor)owner).Inactive)        },
                new() { Rate = ServiceRate.Loop, Parameter = Disabled,           Handler = (animator, owner) => animator.SetBool(Disabled,               ((IMovableActor)owner).Disabled)        },
                new() { Rate = ServiceRate.Loop, Parameter = IsMoving,           Handler = (animator, owner) => animator.SetBool(IsMoving,               ((IMovableActor)owner).IsMoving)        },
            }
        },
        { typeof(ILiving), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = Alive,              Handler = (animator, owner) => animator.SetBool(Alive,                  ((ILiving)owner).Alive)                 },
                new() { Rate = ServiceRate.Loop, Parameter = Dead,               Handler = (animator, owner) => animator.SetBool(Dead,                   ((ILiving)owner).Dead)                  },
            }
        },
        { typeof(IIdle), new Entry[]
            {
                new() { Rate = ServiceRate.Loop, Parameter = Idle,               Handler = (animator, owner) => animator.SetBool(Idle,                   ((IIdle)owner).IsIdle)                  },
                new() { Rate = ServiceRate.Loop, Parameter = IdleTime,           Handler = (animator, owner) => animator.SetFloat(IdleTime,              ((IIdle)owner).IsIdle.Duration)         },
            }
        },
    };

    public static readonly Dictionary<Type, Func<InputIntentSnapshot, Override[]>> InputIntentSnapshot = new()
    {
        { typeof(IAimable), snapshot => new[]
            {
                Float(LockedAimX,         snapshot.Aim.X),
                Float(LockedAimY,         snapshot.Aim.Y),
                Float(LockedCardinalAimX, snapshot.Aim.Cardinal.x),
                Float(LockedCardinalAimY, snapshot.Aim.Cardinal.y),
            }
        },
        { typeof(IMovableActor), snapshot => new[]
            {
                Float(LockedFacingX, snapshot.Facing.X),
                Float(LockedFacingY, snapshot.Facing.Y),
            }
        },
    };
}


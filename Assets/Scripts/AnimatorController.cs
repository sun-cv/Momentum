using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class AnimatorController : Service, IServiceLoop, IDisposable
{
    readonly Actor owner;
    readonly Animator animator;
    
        // -----------------------------------

    readonly Dictionary<string, float> clipDurations    = new();
    readonly List<AnimatorPlayEvent> animatorRequests   = new();

        // -----------------------------------

    ClockTimer transitionTimer;

    bool playing            = false;
    bool allowInterrupt     = false;

    const int LAYER_BASE    = 0;
    const int LAYER_ACTION  = 1;

    // ===============================================================================

    public AnimatorController(Actor actor)
    {
        Services.Lane.Register(this);

        owner       = actor;
        animator    = actor.Bridge.Animator;

        CacheClipDurations();

        owner.Emit.Link.Local<Message<Request, AnimatorPlayEvent>>      (HandleAnimatorPlayEvent);
        owner.Emit.Link.Local<Message<Request, AnimatorTriggerEvent>>   (HandleAnimationTriggerRequest);
        owner.Emit.Link.Local<Message<Request, AnimatorDurationEvent>>  (HandleAnimationDurationRequest);

        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>     (HandlePresenceStateEvent);
    }

    // ===============================================================================

    public void Loop()
    {
        UpdateAnimatorParameters();
        ProcessanimatorRequests();
        DebugLog();
    }

    // ===============================================================================

    void UpdateAnimatorParameters()
    {
        if (owner is ILiving living)
        {
            animator.SetBool("Alive", living.Alive);
            animator.SetBool("Dead",  living.Dead ); 
        }
        
        if (owner is IMovableActor movable)
        {
            animator.SetBool("Inactive", movable.Inactive);
            animator.SetBool("Disabled", movable.Disabled);
            animator.SetBool("IsMoving", movable.IsMoving);

            if (movable is IOrientable orientable && orientable.CanRotate)
            {
                animator.SetFloat("LockedFacingX", movable.LockedFacing.X);
                animator.SetFloat("LockedFacingY", movable.LockedFacing.Y);
            }
        }

        if (owner is IIdle idle)
        {
            animator.SetBool ("Idle",     idle.IsIdle);
            animator.SetFloat("IdleTime", idle.IsIdle.Duration);
        }

        if (owner is IAimable aimable)
        {
            animator.SetFloat("LockedAimX", aimable.LockedAim.X);
            animator.SetFloat("LockedAimY", aimable.LockedAim.Y);

            animator.SetFloat("LockedCardinalAimX", aimable.LockedAim.Cardinal.x);
            animator.SetFloat("LockedCardinalAimY", aimable.LockedAim.Cardinal.y);
        }
    }

    void PlayAnimation(AnimatorRequestEvent request)
    {   
        transitionTimer?.Stop();

        animator.SetLayerWeight(LAYER_ACTION, 1f);
        animator.Play(request.Hash, LAYER_ACTION, 0f);

        playing = true;
        
        if (clipDurations.TryGetValue(request.Name, out float duration))
        {
            transitionTimer = new ClockTimer(duration);
            transitionTimer.OnTimerStop += () => animator.SetLayerWeight(LAYER_ACTION, 0f);
            transitionTimer.OnTimerStop += () => allowInterrupt = true;
            transitionTimer.Start();
        }
    }

    void RequestAnimation(AnimatorRequestEvent request)
    {
        if (playing && !allowInterrupt)
            return;

        SetInterrupt(request);
        PlayAnimation(request);
    }

    void ProcessanimatorRequests()
    {
        if (animatorRequests.Count == 0)
            return;

        foreach (var request in animatorRequests)
        {
            RequestAnimation(request);
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

    void SetInterrupt(AnimatorPlayEvent request)
    {
        allowInterrupt = request.AllowInterrupt;
    }

    // ============================================================================
    //  Events
    // ============================================================================

    void HandleAnimatorPlayEvent(Message<Request, AnimatorPlayEvent> message)
    {
        animatorRequests.Add(message.Payload);
    }

    void HandleAnimationTriggerRequest(Message<Request, AnimatorTriggerEvent> message)
    {
        animator.SetTrigger(message.Payload.Trigger);
    }

    void HandleAnimationDurationRequest(Message<Request, AnimatorDurationEvent> message)
    {
        var duration = clipDurations[message.Payload.Name];
        owner.Emit.Local(message.Id, Response.Completed, new AnimatorDurationEvent(message.Payload.Name, duration));
    }

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

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.AnimationHandler;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct AnimatorPlayEvent
{
    public string Name                              { get; init; }
    public int Hash                                 { get; init; }
    public bool AllowInterrupt                      { get; init; }

    public AnimatorPlayEvent(string name)
    {
        Name            = name;
        Hash            = Animator.StringToHash(Name);
        AllowInterrupt  = true;
    }
}

public readonly struct AnimatorTriggerEvent
{    
    public readonly string Trigger                  { get; init; }

    public AnimatorTriggerEvent(string trigger)
    {
        Trigger = trigger;   
    }
}

public readonly struct AnimatorDurationEvent
{
    public readonly string Name                     { get; init; }
    public readonly float Duration                  { get; init; } 

    public AnimatorDurationEvent(string name, float duration = 0)
    {
        Name        = name;   
        Duration    = duration;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;





public class AnimationController : Service, IServiceLoop, IDisposable
{
    readonly Logger Log = Logging.For(LogSystem.Animation);

    Actor owner;

    readonly Animator animator;
    
    readonly Dictionary<string, float> clipDurations         = new();
    readonly List<AnimationRequestEvent> animationRequests   = new();

    ClockTimer transitionTimer;

    bool playing            = false;
    bool allowInterrupt     = false;

    const int LAYER_BASE    = 0;
    const int LAYER_ACTION  = 1;

    public AnimationController(Actor actor)
    {
        Services.Lane.Register(this);

        owner       = actor;
        animator    = actor.Bridge.Animator;

        CacheClipDurations();

        owner.Emit.Link.Local<Message<Request, AnimationRequestEvent>>  (HandleAnimationRequestEvent);
        owner.Emit.Link.Local<Message<Request, AnimationTriggerEvent>>  (HandleAnimationTriggerRequest);
        owner.Emit.Link.Local<Message<Request, AnimationDurationEvent>> (HandleAnimationDurationRequest);

        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>     (HandlePresenceStateEvent);
    }

    public void Loop()
    {
        UpdateAnimatorParameters();
        ProcessAnimationRequests();
        DebugLog();
    }

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

    void PlayAnimation(AnimationRequestEvent request)
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

    void ProcessAnimationRequests()
    {
        if (animationRequests.Count == 0)
            return;

        foreach (var request in animationRequests)
        {
            RequestAnimation(request);
        }

        animationRequests.Clear();
    }

    void RequestAnimation(AnimationRequestEvent request)
    {
        if (playing && !allowInterrupt)
            return;

        SetInterrupt(request);
        PlayAnimation(request);
    }

    // ============================================================================
    // EVENT HANDLERS
    // ============================================================================


    void HandleAnimationRequestEvent(Message<Request, AnimationRequestEvent> message)
    {
        animationRequests.Add(message.Payload);
    }

    void HandleAnimationTriggerRequest(Message<Request, AnimationTriggerEvent> message)
    {
        animator.SetTrigger(message.Payload.Trigger);
    }

    void HandleAnimationDurationRequest(Message<Request, AnimationDurationEvent> message)
    {
        var duration = clipDurations[message.Payload.Name];
        owner.Emit.Local(message.Id, Response.Completed, new AnimationDurationEvent(message.Payload.Name, duration));
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

    // ============================================================================
    // PREDICATES
    // ============================================================================


    // ============================================================================
    // HELPERS
    // ============================================================================


    void SetInterrupt(AnimationRequestEvent request)
    {
        allowInterrupt = request.AllowInterrupt;
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


// ============================================================================
// ANIMATION EVENTS
// ============================================================================


public class AnimationRequestEvent
{
    public string Name                              { get; init; }
    public int Hash                                 { get; init; }
    public bool AllowInterrupt                      { get; init; }

    public AnimationRequestEvent(string name)
    {
        Name            = name;
        Hash            = Animator.StringToHash(Name);
        AllowInterrupt  = true;
    }
}

public readonly struct AnimationTriggerEvent
{    
    public readonly string Trigger                  { get; init; }

    public AnimationTriggerEvent(string trigger)
    {
        Trigger = trigger;   
    }
}

public readonly struct AnimationDurationEvent
{
    public readonly string Name                     { get; init; }
    public readonly float Duration                  { get; init; } 

    public AnimationDurationEvent(string name, float duration = 0)
    {
        Name        = name;   
        Duration    = duration;
    }
}

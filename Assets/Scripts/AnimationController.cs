using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;




public class AnimatorRequest
{
    public string Name          { get; init; }
    public int Hash             { get; init; }
    public bool AllowInterrupt  { get; init; }

    public AnimatorRequest(string name)
    {
        Name            = name;
        Hash            = Animator.StringToHash(Name);
        AllowInterrupt  = true;
    }
}


public class AnimationController : IServiceTick
{
    readonly Logger Log = Logging.For(LogSystem.Animation);

    Actor owner; 
    Animator animator;
    Dictionary<string, float> clipDurations;
    ClockTimer transitionTimer;

    const int LAYER_BASE    = 0;
    const int LAYER_ACTION  = 1;

    public AnimationController(Actor actor)
    {
        Services.Lane.Register(this);

        owner       = actor;
        animator    = actor.Bridge.Animator;

        CacheClipDurations();

        owner.Emit.Link.Local<Message<Request, MAnimation>>(HandleAnimationRequest);
        owner.Emit.Link.Local<Message<Request, MAnimationDuration>>(HandleAnimationDurationRequest);
    }

    public void Tick()
    {
        UpdateAnimatorParameters();
        DebugLog();
    }

    void UpdateAnimatorParameters()
    {

        
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
            animator.SetBool ("Idle", idle.IsIdle);
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

    void PlayAnimation(AnimatorRequest request)
    {   
        transitionTimer?.Stop();

        animator.SetLayerWeight(LAYER_ACTION, 1f);
        animator.Play(request.Hash, LAYER_ACTION, 0f);
        
        if (clipDurations.TryGetValue(request.Name, out float duration))
        {
            transitionTimer = new ClockTimer(duration);
            transitionTimer.OnTimerStop += () => animator.SetLayerWeight(LAYER_ACTION, 0f);
            transitionTimer.Start();
        }
    }

    // REWORK REQUIRED CANCEL ANIMATION
    void HandleAnimationRequest(Message<Request, MAnimation> evt)
    {
        switch(evt.Action)
        {
            case Request.Start:
                PlayAnimation(evt.Payload.Request);
                break;
            case Request.Cancel:
                break;
        }
    }

    void HandleAnimationDurationRequest(Message<Request, MAnimationDuration> message)
    {
        var duration = clipDurations[message.Payload.Request.Name];
        owner.Emit.Local(message.Id, Response.Completed, new MAnimationDuration(message.Payload.Request, duration));
    }

    void CacheClipDurations()
    {
        clipDurations = new();

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            string key = clip.name.Split('_')[0];
            if (!clipDurations.ContainsKey(key))
                clipDurations[key] = clip.length;
        }
    }

    void DebugLog()
    {
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

    void LinkLocal <T>(Action<T> handler) where T : IEvent  => owner.Emit.Link.Local(handler);


    public UpdatePriority Priority => ServiceUpdatePriority.AnimationHandler;
}
// ============================================================================
// EVENTS
// ============================================================================


public readonly struct MAnimation
{
    public readonly object Owner                { get; init; }
    public readonly AnimatorRequest Request     { get; init; }

    public MAnimation(object owner, AnimatorRequest request)
    {
        Owner   = owner;
        Request = request;   
    }
}

public readonly struct MDisableAnimation
{
    
}

public readonly struct MAnimationDuration
{
    public readonly AnimatorRequest Request     { get; init; }
    public readonly float Duration              { get; init; } 

    public MAnimationDuration(AnimatorRequest request, float duration = 0)
    {
        Request     = request;   
        Duration    = duration;
    }
}
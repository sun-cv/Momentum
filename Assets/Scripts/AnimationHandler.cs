using System;
using System.Collections.Generic;
using UnityEngine;




public class AnimatorRequest
{
    public string Name      { get; init; }
    public int Hash         { get; init; }

    public AnimatorRequest(string name)
    {
        Name = name;
        Hash = Animator.StringToHash(Name);
    }
}


public class AnimationHandler : IServiceTick
{
    Actor owner; 
    Animator animator;
    Dictionary<string, float> clipDurations;
    ClockTimer transitionTimer;

    public AnimationHandler(Actor actor)
    {
        if (actor.Bridge is not ActorBridge bridge)
        {
            Log.Error(LogSystem.Animation, LogCategory.Activation, () => $"Animation Handler activation requires Actor Bridge (actor {actor.RuntimeID} failed)");
            return;
        }
        
        owner       = actor;
        animator    = bridge.Animator;

        CacheClipDurations();
        EventBus<AnimationRequest>.Subscribe(HandleAnimationRequest);
        GameTick.Register(this);
    }

    public void Tick()
    {
        UpdateAnimatorParameters();
    }

    void UpdateAnimatorParameters()
    {
        if (owner is IMovableActor movable)
        {
            animator.SetBool("Inactive", movable.Inactive);
            animator.SetBool("Disabled", movable.Disabled);
            animator.SetBool("IsMoving", movable.IsMoving);
            animator.SetFloat("FacingX", movable.Facing.x);
            animator.SetFloat("FacingY", movable.Facing.y);
        }

        if (owner is IIdle idle)
        {
            animator.SetBool("Idle", idle.IsIdle);
            animator.SetFloat("IdleTime", idle.IsIdle.Duration);
        }

        if (owner is IAimable aimable)
        {
            animator.SetFloat("AimingX", aimable.AimDirection.x);
            animator.SetFloat("AimingY", aimable.AimDirection.y);
        }
    }

    void PlayAnimation(AnimatorRequest request)
    {   
        animator.Play(request.Hash);
        
        if (clipDurations.TryGetValue(request.Name, out float duration))
        {
            Debug.Log("name matches");
            transitionTimer?.Stop();
            transitionTimer = new ClockTimer(duration);
            transitionTimer.OnTimerStop += () => animator.Play("Idle");
            transitionTimer.Start();
        }
    }

    // REWORK REQUIRED CANCEL ANIMATION
    void HandleAnimationRequest(AnimationRequest evt)
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

    public UpdatePriority Priority => ServiceUpdatePriority.AnimationHandler;
}
// ============================================================================
// EVENTS
// ============================================================================

public readonly struct AnimationRequestPayload
{
    public readonly object Owner                { get; init; }
    public readonly AnimatorRequest Request     { get; init; }
}

public readonly struct AnimationRequest : ISystemEvent
{
    public Guid Id                              { get; }
    public Request Action                       { get; }
    public AnimationRequestPayload Payload      { get; }

    public AnimationRequest(Guid id, Request action, AnimationRequestPayload payload)
    {
        Id      = id;
        Action  = action;
        Payload = payload;
    }
}
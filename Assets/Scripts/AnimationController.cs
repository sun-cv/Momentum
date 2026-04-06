using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class AnimationController : ActorService, IServiceTick, IServiceLoop, IDisposable
{
    readonly Animator animator;
    
        // -----------------------------------

    readonly Dictionary<string, float> clipDurations;
    readonly List<int> validParameters;

        // -----------------------------------

    readonly List<AnimationAPI> queue;

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

    public AnimationController(Actor actor) : base(actor)
    {
        if (!ValidateOwner(actor))
            return;

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

        Enable();
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void RequestAnimationChange(AnimationAPI request)
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

    // ===============================================================================

    void ProcessAnimatorRequests()
    {
        if (queue.Count == 0)
            return;

        foreach (var request in queue)
        {
            switch(request.Request)
            {
                case Request.Play: ProcessAnimation(request);   break;
                case Request.Stop: ClearAnimation(request);     break;
            }
        }

        queue.Clear();
    }

    void ProcessHandlers(Dictionary<int, Action<Animator, Actor>> handlers)
    {
        foreach (var (param, handler) in handlers)
        {
            if (overrides.TryGetValue(param, out var overwrite))
                ApplyOverride(overwrite);
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


    void ProcessAnimation(AnimationAPI request)
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

    void PlayAnimation(AnimationAPI request)
    {   
        animator.SetLayerWeight(LAYER_ACTION, 1f);
        animator.Play(request.Data.Animation, LAYER_ACTION, 0f);

        playing = true;
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


    void SetInterrupt(AnimationAPI request)
    {
        allowInterrupt = request.Settings.AllowInterrupt;
    }

    void SetOverrides(AnimationAPI request)
    {
        overrides.Clear();

        if (request.HasOverrides)
        {
            foreach (var handler in request.Data.Overrides)
                overrides[handler.Parameter] = handler;
        }
    }

    void ClearTransitionTimer()
    {
        transitionTimer?.Stop();
    }

    void SetTransitionTimer(AnimationAPI request)
    {
        if (clipDurations.TryGetValue(request.Data.Animation, out float duration))
        {
            transitionTimer = new ClockTimer(duration);
            transitionTimer.OnTimerStop += () => ClearAnimation(request);
            transitionTimer.Start();
        }
    }

    void ClearAnimation(AnimationAPI request)
    {
        SendAnimatorPlaybackEvent(Publish.Ended, request);

        if (!IsStateActive(LAYER_ACTION, request.Data.Animation))
            return;

        if (request.Settings.HoldOnPlaybackEnd)
            return;

        ClearAnimatorState();
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

    void SendAnimatorPlaybackEvent(Publish type, AnimationAPI request)
    {
        owner.Bus.Emit.Local(new AnimatorEvent(type, request.Data.Animation));
    }

    // string GetCurrentAnimationName(int layer)
    // {
    //     var clipInfo = animator.GetCurrentAnimatorClipInfo(layer);
    //     return clipInfo.Length > 0 ? clipInfo[0].clip.name : null;
    // }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    bool ValidateOwner(Actor actor)
    {
        if (actor.Bridge.Animator == null)
        {
            Log.Error($"{actor.GetType().Name} Failed System Validation. Animator Controller requires Animator assigned in Bridge");
            return false;
        }

        return true;
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Animation);

    void DebugLog()
    {
        if (owner is not Hero hero)
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

        Log.Trace("rateX", () => hero.ResolvedFacing.X);
        Log.Trace("rateY", () => hero.ResolvedFacing.Y);
        Log.Trace("overrides", () => overrides.Count());
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



using System;
using System.Collections.Generic;
using UnityEngine;



public class AnimationSystem : Service, IServiceLoop
{
    readonly Actor owner;
    readonly AnimationDefinition animations;

    // -----------------------------------

    readonly AnimatorController animator;

    // -----------------------------------

    readonly List<Message<Request, AnimationAPI>> queue = new();

    // ===============================================================================

    public AnimationSystem(Actor actor)
    {

        Services.Lane.Register(this);

        owner       = actor;
        animations  = actor.Definition.Animations;

        animator    = new(owner);

        owner.Bus.Link.Local<Message<Request, AnimationAPI>>(HandleAnimationRequest);        
        owner.Bus.Link.Local<PresenceStateEvent>(HandlePresenceStateEvent);
    } 

    // ===============================================================================

    public void Loop()
    {
        ProcessAnimationRequests();
    }

    // ===============================================================================

    void ProcessAnimationRequests()
    {
        if (queue.Count == 0)
            return;

        foreach (var message in queue)
        {
            RouteMessage(message);
        }

        queue.Clear();
    }

    void RouteMessage(Message<Request, AnimationAPI> message)
    {
        var request = message.Payload.AnimationRequest;

        if (owner is IMovableDummy) Debug.Log($"Animation request routing {message.Action}");

        switch(message.Payload.AnimationRequest.options.Request)
        {
            case Request.Play: ProcessAnimationRequest(message);    break;
            case Request.Stop: RequestAnimationChange(request);     break;
        }        
    }

    void ProcessAnimationRequest(Message<Request, AnimationAPI> message)
    {
        if (owner is IMovableDummy) Debug.Log("Animation request Processing");
        var request = message.Payload.AnimationRequest;

        if (!request.IsValid)
            Resolve(request);

        if (owner is IMovableDummy) Debug.Log("Animation request Processed");
        RequestAnimationChange(request);

        owner.Bus.Emit.Local(message.Id, Response.Completed, new AnimationAPI(request));
    }

    AnimationRequest Resolve(AnimationRequest request)
    {
        var set                 = SelectAnimationSet(request.intent);
        var animation           = SelectAnimation(set, request);

        SetAnimationData(request, animation);

        Debug.Log("Resolve");

        return request;
    }

    AnimationSet SelectAnimationSet(AnimationIntent intent)
    {
        if (!IntentMap.Animations.TryGetValue(intent, out var selector))
        {
            Log.Error($"No animation mapping for intent {intent}");
            return null;
        }

        return selector(animations);
    }

    string SelectAnimation(AnimationSet set, AnimationRequest request)
    {

        if (request.HasContext)
        {
            var contextual = ResolveContext(set, request.context);
            if (contextual != null)
            {
                return contextual;
            }
        }

        if (set.Random?.Length > 0)
            return set.Random[UnityEngine.Random.Range(0, set.Random.Length)];

        return set.Default;
    }

    void SetAnimationData(AnimationRequest request, string animation)
    {
        request.data.Animation  = animation;
        request.data.Duration   = animator.RequestAnimationDuration(request.data.Animation);
    }

    void RequestAnimationChange(AnimationRequest request)
    {
        animator.RequestAnimationChange(request);
    }

        // REWORK REQUIRED FOR CONTEXT
    string ResolveContext(AnimationSet set, AnimationContext context)
    {
        return "";
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleAnimationRequest(Message<Request, AnimationAPI> message)
    {
        queue.Add(message);
    }

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

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority      => ServiceUpdatePriority.AnimationSystem;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                     Classes
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AnimationRequest
{
    public AnimationIntent                  intent;
    public AnimationContext                 context;
    public AnimationOptions                 options;
    public AnimationData                    data;
    public List<AnimatorParameter.Override> overrides;

    public bool IsValid         => data.Animation   != null;
    public bool HasContext      => context          != null;
    public bool HasOverrides    => overrides        != null && overrides.Count > 0;

    public AnimationRequest(string name)
    {
        this.intent         = new();
        this.context        = new();
        this.options        = new() { AllowInterrupt = true };
        this.data           = new();
        this.overrides      = new();

        data.Animation  = name;
    }

    public AnimationRequest(AnimationIntent intent, AnimationContext context = null)
    {
        this.intent         = intent;
        this.context        = context;
        this.options        = new() { AllowInterrupt = true };
        this.data           = new();
        this.overrides      = new();
    }

    public void Play()
    {
        options.Request = Request.Play;
    }

    public void Stop()
    {
        options.Request = Request.Stop;
    }
}

public class AnimationOptions
{
    public Request Request                          { get; set; }
    public bool AllowInterrupt                      { get; set; }
    public bool HoldUntilReleased                   { get; set; }
}

public class AnimationContext
{
    public DamageComponent DamageComponent          { get; init; }
};

public class AnimationData
{
    public string Animation                         { get; set; }
    public float Duration                           { get; set; }
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                      Enums
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum AnimationIntent
{
    Spawn,
    Death,
    Teleport,
    Idle,
    Interact,
    Stagger,
    Stun,
    Frozen,
    // etc
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AnimationAPI : Payload
{
    public AnimationRequest AnimationRequest        { get; init; }

    public AnimationAPI(AnimationRequest request)
    {
        AnimationRequest = request;
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

public static partial class IntentMap
{

    public static readonly Dictionary<AnimationIntent, Func<AnimationDefinition, AnimationSet>> Animations = new()
    {
        { AnimationIntent.Spawn, definition => definition.Spawn },
        { AnimationIntent.Death, definition => definition.Death },
    };
}

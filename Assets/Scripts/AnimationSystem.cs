using System;
using System.Collections.Generic;



public class AnimationSystem : ActorService, IServiceLoop
{
    readonly AnimationDefinition animations;

    // -----------------------------------

    readonly AnimatorController animator;

    // -----------------------------------

    readonly List<Message<Request, AnimationAPI>> queue = new();

    // ===============================================================================

    public AnimationSystem(Actor actor) : base(actor)
    {
        animations  = actor.Definition.Animations;

        animator    = new(owner);

        owner.Bus.Link.Local<Message<Request, AnimationAPI>>(HandleAnimationRequest);        

        Enable();
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
        var API = message.Payload;

        switch(API.Request)
        {
            case Request.Play: ProcessAnimationRequest(API);    break;
            case Request.Stop: RequestAnimationChange (API);    break;
        }        
    }

    void ProcessAnimationRequest(AnimationAPI request)
    {
        if (!request.IsValid)
            Resolve(request);

        RequestAnimationChange(request);

        owner.Bus.Emit.Local(request.Id, Response.Completed, request);
    }

    AnimationAPI Resolve(AnimationAPI request)
    {
        var set                 = SelectAnimationSet(request.Intent);
        var animation           = SelectAnimation(set, request);

        SetAnimationData(request, animation);

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

    string SelectAnimation(AnimationSet set, AnimationAPI request)
    {

        if (request.HasContext)
        {
            var contextual = ResolveContext(set, request.Context);
            if (contextual != null)
            {
                return contextual;
            }
        }

        if (set.Random?.Length > 0)
            return set.Random[UnityEngine.Random.Range(0, set.Random.Length)];

        return set.Default;
    }

    void SetAnimationData(AnimationAPI request, string animation)
    {
        request.Data.Animation  = animation;
        request.Data.Duration   = animator.RequestAnimationDuration(request.Data.Animation);
    }

    void RequestAnimationChange(AnimationAPI request)
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

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Animation);

    public UpdatePriority Priority => ServiceUpdatePriority.AnimationSystem;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                     Classes
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AnimationAPI : API
{
    public AnimationIntent Intent                   { get; init; }
    public AnimationContext Context                 { get; init; }
    public AnimationSettings Settings               { get; init; }
    public AnimationData Data                       { get; init; }

    // ===============================================================================

    public AnimationAPI(string name)
    {
        Settings           = new() { AllowInterrupt = true };
        Data               = new() { Animation      = name };
    }

    public AnimationAPI(AnimationIntent intent, AnimationContext context = null)
    {
        Intent             = intent;
        Context            = context;
        Settings           = new() { AllowInterrupt = true };
        Data               = new();
    }

    // ===============================================================================

    public bool IsValid         => Data.Animation   != null;
    public bool HasContext      => Context          != null;
    public bool HasOverrides    => Data.Overrides   != null && Data.Overrides.Count > 0;
}

public class AnimationSettings
{
    public bool AllowInterrupt                          { get; set; }
    public bool HoldUntilReleased                       { get; set; }
    public bool HoldOnPlaybackEnd                       { get; set; }
}

public class AnimationContext
{
    public KillingBlow KillingBlow                      { get; set; }
};

public class AnimationData
{
    public string Animation                             { get; set; }
    public float Duration                               { get; set; }
    public List<AnimatorParameter.Override> Overrides   { get; set; } = new();
};

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

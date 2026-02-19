using System.Collections.Generic;



public class AnimationController : Service, IServiceLoop
{
    readonly Actor owner;

    // -----------------------------------

    // -----------------------------------

    readonly LocalEventHandler<Message<Response, AnimatorDurationEvent>> animatorDurationHandler;
    readonly LocalEventHandler<Message<Request, AnimationDurationEvent>> animationDurationHandler;

    // -----------------------------------

    readonly List<AnimationPlayEvent> AnimationRequests = new();

    // ===============================================================================

    public AnimationController(Actor actor)
    {
        this.owner = actor;

        animatorDurationHandler = new(owner.Emit, HandleAnimationDurationResponse);

        owner.Emit.Link.Local<Message<Request, AnimationPlayEvent>>     (HandleAnimationPlayRequest);
        owner.Emit.Link.Local<Message<Request, AnimationIntentEvent>>   (HandleAnimationIntentRequest);
        owner.Emit.Link.Local<Message<Request, AnimationDurationEvent>> (HandleAnimationDurationRequest);
        
        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>     (HandlePresenceStateEvent);

    }

    // ===============================================================================

    public void Loop()
    {
        
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleAnimationPlayRequest(Message<Request, AnimationPlayEvent> message)
    {
        owner.Emit.Local(Request.Start, new AnimatorPlayEvent(message.Payload.Name) { AllowInterrupt = message.Payload.AllowInterrupt });
    }

    void HandleAnimationIntentRequest(Message<Request, AnimationIntentEvent> message)
    {
        AnimationRequests.Add(message.Payload.Intent);
    }

    void HandleAnimationDurationRequest(Message<Request, AnimationDurationEvent> message)
    {
        animatorDurationHandler.Forward(message.Id, Request.Get, new AnimatorDurationEvent(message.Payload.Name));
    }

    void HandleAnimationDurationResponse(Message<Response, AnimatorDurationEvent> message)
    {
        owner.Emit.Local(message.Id, Response.Completed, new AnimationDurationEvent(message.Payload.Name, message.Payload.Duration));
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

    public override void Dispose()
    {
        
    }

    public UpdatePriority Priority      => ServiceUpdatePriority.Lifecycle;
}

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                         Enums
    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

    public class AnimationRequest
    {
        
    }

    // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
    //                                         Enums
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


public readonly struct AnimationContext
{
    public DamageComponent DamageComponent          { get; init; }
};

public readonly struct AnimationPlayEvent
{
    public string Name                              { get; init; }
    public bool AllowInterrupt                      { get; init; }

    public AnimationPlayEvent(string name)
    {
        Name            = name;
        AllowInterrupt  = true;
    }
}

public readonly struct AnimationIntentEvent
{
    public AnimationIntent Intent                   { get; init; }
    public AnimationContext Context                 { get; init; }
    
    public AnimationIntentEvent(AnimationIntent intent, AnimationContext context = new())
    {
        Intent  = intent;
        Context = context;
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

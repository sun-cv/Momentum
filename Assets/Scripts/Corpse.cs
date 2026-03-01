using System.Collections.Generic;



public class Corpse : Service, IServiceStep
{    

    public enum State { Fresh, Decaying, Consumed, Remains, Disposal }

    // ===============================================================================

    readonly Actor owner;
    readonly ActorDefinition definition;

        // -----------------------------------

    Dictionary<State, IStateHandler<Corpse>> stateHandlers;

        // -----------------------------------

    State state         = State.Fresh;

    // ===============================================================================

    public Corpse(Actor actor)
    {
        owner = actor;
    
        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>>(HandlePresenceStateEvent);

        InitializeStateHandlers();
        EnterHandler();
    }

    void InitializeStateHandlers()
    {
        stateHandlers = new()
        {
            { State.Fresh, new CorpseFreshState(owner, definition)}
        };
    }

    // ===============================================================================

    public void Step()
    {
        UpdateHandler();
    }

    // ===============================================================================

    void UpdateHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Update(this);
    }

        // ===================================
        //  State
        // ===================================

    public void TransitionTo(State newState)
    {
        ExitHandler();
        TransitionState(newState);
        EnterHandler();
    }

    void ExitHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Exit(this);
    }

    void TransitionState(State newState)
    {
        state = newState;
        PublishState();
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Enter(this);
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

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

    void PublishState()
    {
        owner.Emit.Local(Publish.Transitioning, new CorpseEvent(owner, state));
    }



    public Corpse.State Condition => state;

    // ===============================================================================

    public override void Dispose()
    {
        
    }

    public UpdatePriority Priority => ServiceUpdatePriority.Corpse;
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Fresh state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseFreshState : IStateHandler<Corpse>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseFreshState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Corpse controller)
    {   
        PublishStateChange();
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Decaying);
    }

    public void Exit(Corpse controller)
    {
    }

    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new CorpseStateEvent(owner, Corpse.State.Fresh));
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Decaying state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseDecayingState : IStateHandler<Corpse>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseDecayingState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Corpse controller)
    {   
        PublishStateChange();
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Remains);
    }

    public void Exit(Corpse controller)
    {
    }

    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new CorpseStateEvent(owner, Corpse.State.Decaying));
    }
}



        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Consumed state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseConsumedState : IStateHandler<Corpse>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseConsumedState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Corpse controller)
    {   
        PublishStateChange();
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Remains);
    }

    public void Exit(Corpse controller)
    {
    }

    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new CorpseStateEvent(owner, Corpse.State.Consumed));
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Remains state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseRemainsState : IStateHandler<Corpse>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseRemainsState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Corpse controller)
    {   
        PublishStateChange();
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Disposal);
    }

    public void Exit(Corpse controller)
    {
    }

    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new CorpseStateEvent(owner, Corpse.State.Remains));
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Disposal state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseDisposalState : IStateHandler<Corpse>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseDisposalState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Corpse controller)
    {   
        PublishStateChange();
    }

    public void Update(Corpse controller)
    {
        owner.Emit.Local(Request.Transition, new LifecycleTargetEvent(Lifecycle.State.Disposal));
    }

    public void Exit(Corpse controller)
    {
        
    }

    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new CorpseStateEvent(owner, Corpse.State.Disposal));
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events                                         
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CorpseEvent
{
    public readonly Actor Owner             { get; init; }
    public readonly Corpse.State State      { get; init; }

    public CorpseEvent(Actor owner, Corpse.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

public readonly struct CorpseStateEvent
{
    public readonly Actor Owner             { get; init; }
    public readonly Corpse.State State    { get; init; }

    public CorpseStateEvent(Actor owner, Corpse.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

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
    
        owner.Emit.Link.Local<PresenceStateEvent>(HandlePresenceStateEvent);

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
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Enter(this);
    
        PublishState();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandlePresenceStateEvent(PresenceStateEvent message)
    {
        switch (message.State)
        {
            case Presence.State.Entering: Enable();  break;
            case Presence.State.Exiting:  Disable(); break;
            case Presence.State.Disposal: Dispose(); break;
        }
    }

    void PublishState()
    {
        owner.Emit.Local(new CorpseEvent(owner, state));
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
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Decaying);
    }

    public void Exit(Corpse controller)
    {
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
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Remains);
    }

    public void Exit(Corpse controller)
    {
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
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Remains);
    }

    public void Exit(Corpse controller)
    {
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
    }

    public void Update(Corpse controller)
    {
        controller.TransitionTo(Corpse.State.Disposal);
    }

    public void Exit(Corpse controller)
    {
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
    }

    public void Update(Corpse controller)
    {
        owner.Emit.Local(new LifecycleTargetEvent(Lifecycle.State.Disposal));
    }

    public void Exit(Corpse controller)
    {
        
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events                                         
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CorpseEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly Corpse.State State      { get; init; }

    public CorpseEvent(Actor owner, Corpse.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

public readonly struct CorpseStateEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly Corpse.State State    { get; init; }

    public CorpseStateEvent(Actor owner, Corpse.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

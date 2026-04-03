using System.Collections.Generic;



public class Decomposition : ActorService, IServiceStep
{    
    public enum State { Fresh, Decaying, Consumed, Remains, Disposal }

    // ===============================================================================

    Dictionary<State, IStateHandler> stateHandlers;

        // -----------------------------------

    int occupancy   = 0;
    State state     = State.Fresh;

    // ===============================================================================

    public Decomposition(Actor actor) : base(actor)
    {
        InitializeStateHandlers();
        EnterHandler();
    }

    void InitializeStateHandlers()
    {
        stateHandlers = new()
        {
            { State.Fresh, new CorpseFreshState(this, owner)}
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
            handler.Update();
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
            handler.Exit();
    }

    void TransitionState(State newState)
    {
        state = newState;
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Enter();
    
        PublishState();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void PublishState()
    {
        owner.Bus.Emit.Local(new CorpseEvent(owner, state));
    }

    // ===============================================================================

    public int Occupancy    => occupancy;
    public State Condition  => state;

    public UpdatePriority Priority => ServiceUpdatePriority.Corpse;
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Fresh state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseFreshState : IStateHandler
{
    readonly Actor      owner;
    readonly ICorpse    corpse;
    readonly Decomposition     controller;
    readonly ActorDefinition definition;

    // -----------------------------------

    float entryTime;

    // ===============================================================================

    public CorpseFreshState(Decomposition controller, Actor owner)
    {
        this.owner      = owner;
        this.corpse     = owner as ICorpse;
        this.definition = owner.Definition;
        this.controller = controller;
    }

    // ===============================================================================

    public void Enter()
    {
        entryTime = Clock.Time;
    }

    public void Update()
    {
        controller.TransitionTo(Decomposition.State.Decaying);
    }

    public void Exit()
    {
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Decaying state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseDecayingState : IStateHandler
{
    readonly Actor      owner;
    readonly ICorpse    corpse;
    readonly Decomposition     controller;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseDecayingState(Decomposition controller, Actor owner)
    {
        this.owner      = owner;
        this.corpse     = owner as ICorpse;
        this.definition = owner.Definition;
        this.controller = controller;
    }

    // ===============================================================================

    public void Enter()
    {   
    }

    public void Update()
    {
        controller.TransitionTo(Decomposition.State.Remains);
    }

    public void Exit()
    {
    }
}



        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Consumed state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseConsumedState : IStateHandler
{
    readonly Actor      owner;
    readonly ICorpse    corpse;
    readonly Decomposition     controller;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseConsumedState(Decomposition controller, Actor owner)
    {
        this.owner      = owner;
        this.corpse     = owner as ICorpse;
        this.definition = owner.Definition;
        this.controller = controller;
    }

    // ===============================================================================

    public void Enter()
    {   
    }

    public void Update()
    {
        controller.TransitionTo(Decomposition.State.Remains);
    }

    public void Exit()
    {
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Remains state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseRemainsState : IStateHandler
{
    readonly Actor      owner;
    readonly ICorpse    corpse;
    readonly Decomposition     controller;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseRemainsState(Decomposition controller, Actor owner)
    {
        this.owner      = owner;
        this.corpse     = owner as ICorpse;
        this.definition = owner.Definition;
        this.controller = controller;
    }

    // ===============================================================================

    public void Enter()
    {   
    }

    public void Update()
    {
        controller.TransitionTo(Decomposition.State.Disposal);
    }

    public void Exit()
    {
    }

}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Disposal state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseDisposalState : IStateHandler
{
    readonly Actor      owner;
    readonly ICorpse    corpse;
    readonly Decomposition     controller;
    readonly ActorDefinition definition;

    // ===============================================================================

    public CorpseDisposalState(Decomposition controller, Actor owner)
    {
        this.owner      = owner;
        this.corpse     = owner as ICorpse;
        this.definition = owner.Definition;
        this.controller = controller;
    }

    // ===============================================================================

    public void Enter()
    {   
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Absent));
    }

    public void Update()
    {
    }

    public void Exit()
    {
        
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events                                         
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct CorpseEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly Decomposition.State State      { get; init; }

    public CorpseEvent(Actor owner, Decomposition.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

public readonly struct CorpseStateEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly Decomposition.State State    { get; init; }

    public CorpseStateEvent(Actor owner, Decomposition.State state)
    {
        Owner   = owner;
        State   = state;
    }
}

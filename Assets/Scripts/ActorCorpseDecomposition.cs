


public class Decomposition : ActorService, IServiceStep
{    
    public enum State { Fresh, Decaying, Consumed, Remains, Disposal }

    // ===============================================================================

    // int occupancy   = 0;
    readonly DecompositionStateMachine stateMachine;

    // ===============================================================================

    public Decomposition(Actor actor) : base(actor)
    {
        InitializeStateHandlers();
    }

    void InitializeStateHandlers()
    {
        stateMachine.Register(State.Fresh,      new CorpseFreshState    (stateMachine));
        stateMachine.Register(State.Decaying,   new CorpseDecayingState (stateMachine));
        stateMachine.Register(State.Consumed,   new CorpseConsumedState (stateMachine));
        stateMachine.Register(State.Remains,    new CorpseRemainsState  (stateMachine));
        stateMachine.Register(State.Disposal,   new CorpseDisposalState (stateMachine));
    }

    // ===============================================================================

    public void Step()
    {
        UpdateHandler();
    }

    // ===============================================================================

    void UpdateHandler()
    {
        stateMachine.Update();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    public void PublishState()
    {
        owner.Bus.Emit.Local(new CorpseEvent(owner, stateMachine.State));
        Log.Debug($"{stateMachine.State}");
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Corpse);


    // public int Occupancy    => occupany;
    public State Condition  => stateMachine.State;

    public UpdatePriority Priority => ServiceUpdatePriority.Corpse;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Classes                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class DecompositionStateMachine : StateMachine<Decomposition.State>
{
    readonly Decomposition controller;

    public DecompositionStateMachine(Decomposition controller) : base(controller.PublishState) 
    {
        this.controller = controller;
    }

    public Decomposition Controller => controller;
}

public class DecompositionState : MachineState<Decomposition.State, DecompositionStateMachine>
{
    public DecompositionState(DecompositionStateMachine machine) : base(machine) {}
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Fresh state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseFreshState : DecompositionState, IStateHandler
{
    readonly ActorDefinition definition;

    // -----------------------------------

    ClockTimer duration;

    // ===============================================================================

    public CorpseFreshState(DecompositionStateMachine machine) : base(machine)
    {
        this.definition = machine.Controller.Owner.Definition;
    }

    // ===============================================================================

    public void Enter()
    {
        duration = new(definition.Lifecycle.Corpse.FreshDuration);
        duration.Start();
    }

    public void Update()
    {
        AdvanceOnDurationEnd();
    }

    public void Exit()
    {

    }

    // ===============================================================================

    void AdvanceOnDurationEnd()
    {
        if (duration.IsFinished)
            machine.TransitionTo(Decomposition.State.Decaying);
    }

}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Decaying state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseDecayingState : DecompositionState, IStateHandler
{
    readonly ActorDefinition definition;

    // -----------------------------------

    ClockTimer duration;

    // ===============================================================================

    public CorpseDecayingState(DecompositionStateMachine machine) : base(machine)
    {
        this.definition = machine.Controller.Owner.Definition;
    }

    // ===============================================================================

    public void Enter()
    {
        duration = new(definition.Lifecycle.Corpse.DecayDuration);
        duration.Start();
    }

    public void Update()
    {
        AdvanceOnDurationEnd();
    }

    public void Exit()
    {
    }

    // ===============================================================================

    void AdvanceOnDurationEnd()
    {
        if (duration.IsFinished)
            machine.TransitionTo(Decomposition.State.Consumed);
    }

}



        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Consumed state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseConsumedState : DecompositionState, IStateHandler
{
    readonly ActorDefinition definition;

    // -----------------------------------

    ClockTimer duration;

    // ===============================================================================

    public CorpseConsumedState(DecompositionStateMachine machine) : base(machine)
    {
        this.definition = machine.Controller.Owner.Definition;
    }

    // ===============================================================================

    public void Enter()
    {
        duration = new(definition.Lifecycle.Corpse.ConsumeDuration);
        duration.Start();
    }

    public void Update()
    {
        AdvanceOnDurationEnd();
    }

    public void Exit()
    {
    }

    // ===============================================================================

    void AdvanceOnDurationEnd()
    {
        if (duration.IsFinished)
            machine.TransitionTo(Decomposition.State.Remains);
    }

}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Remains state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseRemainsState : DecompositionState, IStateHandler
{
    readonly ActorDefinition definition;

    // -----------------------------------

    ClockTimer duration;

    // ===============================================================================

    public CorpseRemainsState(DecompositionStateMachine machine) : base(machine)
    {
        this.definition = machine.Controller.Owner.Definition;
    }

    // ===============================================================================

    public void Enter()
    {
        duration = new(definition.Lifecycle.Corpse.RemainsDuration);
        duration.Start();
    }

    public void Update()
    {
        AdvanceOnDurationEnd();
    }

    public void Exit()
    {
    }

    // ===============================================================================

    void AdvanceOnDurationEnd()
    {
        if (duration.IsFinished)
            machine.TransitionTo(Decomposition.State.Disposal);
    }


}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                             Disposal state
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseDisposalState : DecompositionState, IStateHandler
{
    readonly Actor owner;

    // ===============================================================================

    public CorpseDisposalState(DecompositionStateMachine machine) : base(machine)
    {
        owner = machine.Controller.Owner;
    }

    // ===============================================================================

    public void Enter()
    {   
        owner.Bus.Emit.Local(new PresenceTargetEvent(Presence.Target.Disposal));
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

using UnityEngine;



public class Presence : ActorService, IServiceLoop
{
    public enum Target      { Initialize,                      Present, Simulated, Absent, Disposal }
    public enum State       {             Resetting, Entering, Present, Simulated, Absent, Disposal }

        // -----------------------------------

    Target target           = Target.Present;
    Target current          = Target.Initialize;

    PresenceStateMachine stateMachine;

    // ===============================================================================

    public Presence(Actor actor) : base(actor)
    {
        owner.Bus.Link.Local<PresenceTargetEvent>(HandlePresenceTargetEvent);

        InitializeState();

        Enable();
    }

    void InitializeState()
    {
        stateMachine = new(this);

        stateMachine.Register(State.Entering,    new PresenceEnteringState   (stateMachine));
        stateMachine.Register(State.Present,     new PresencePresentState    (stateMachine));
        stateMachine.Register(State.Simulated,   new PresenceSimulatedState  (stateMachine));
        stateMachine.Register(State.Absent,      new PresenceAbsentState     (stateMachine));
        stateMachine.Register(State.Disposal,    new PresenceDisposalState   (stateMachine));
        
        stateMachine.Initialize(State.Entering);
    }

    // ===============================================================================

    public void Loop()
    {
        AdvanceState();
        ProcessState();
        DebugLog();
    }

    // ===============================================================================

    void ProcessState()
    {
        stateMachine.Update();
    }

    void AdvanceState()
    {
        if (current == target)
            return;

        if (stateMachine.State == State.Disposal)
            return;

        switch (target)
        {
            case Target.Present: 
            case Target.Simulated: 
                switch(current)
                {
                    case Target.Absent: stateMachine.TransitionTo(State.Resetting);  break;
                    default:            stateMachine.TransitionTo(State.Entering);   break;
                }
            break;
            case Target.Absent:
                stateMachine.TransitionTo(State.Absent);
            break;
            case Target.Disposal:
                stateMachine.TransitionTo(State.Disposal);
            break;
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
    
    public void PublishState()
    {
        owner.Bus.Emit.Local(new PresenceStateEvent(owner, stateMachine.State));
    }

    void HandlePresenceTargetEvent(PresenceTargetEvent message)
    {
        target = message.Target;
    }

    protected override void HandlePresenceStateEvent(PresenceStateEvent message)
    {
        switch (message.State)
        {
            case State.Disposal: Dispose(); break;
        }
    }
    
    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Presence);

    void DebugLog()
    {
        Log.Debug($"Presence.State.{owner.GetType().Name}", () => stateMachine.State, clean: true);
    }

    public Target Current           { get => current; set => current = value; }
    public Target Desired           { get => target;  set =>  target = value; }

    public UpdatePriority Priority  => ServiceUpdatePriority.Presence;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Classes                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class PresenceStateMachine : StateMachine<Presence.State>
{
    readonly Presence controller;

    public PresenceStateMachine(Presence controller) : base(controller.PublishState) 
    {
        this.controller = controller;
    }

    public Presence Controller => controller;
}

public class PresenceState : MachineState<Presence.State, PresenceStateMachine>
{
    public PresenceState(PresenceStateMachine machine) : base(machine) {}
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Initialize
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceResettingState : PresenceState, IStateHandler
{
    // ===============================================================================

    public PresenceResettingState(PresenceStateMachine machine) : base(machine)
    {
    }

    // ===============================================================================

    public void Enter()
    {   
    }

    public void Update()
    {
        AdvanceState();
    }

    public void Exit()  {}


    // ===============================================================================
    
    void AdvanceState()
    {
        machine.TransitionTo(Presence.State.Entering);
    }

}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Entering
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceEnteringState : PresenceState, IStateHandler
{
    // ===============================================================================

    public PresenceEnteringState(PresenceStateMachine machine) : base(machine)
    {
    }
    // ===============================================================================

    public void Enter()
    {   
    }

    public void Update()
    {
        AdvanceState();
    }

    public void Exit()
    {
    }

    // ===============================================================================
    
    void AdvanceState()
    {
        switch(machine.Controller.Desired)
        {
            case Presence.Target.Present:   machine.TransitionTo(Presence.State.Present);   break;
            case Presence.Target.Simulated: machine.TransitionTo(Presence.State.Simulated); break;
        }
    }

}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Present
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresencePresentState : PresenceState, IStateHandler
{
    // ===============================================================================

    public PresencePresentState(PresenceStateMachine machine) : base(machine)
    {
    }

    // ===============================================================================

    public void Enter()
    {   
        SetCurrentTarget();
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }

    // ===============================================================================

    void SetCurrentTarget()
    {
        machine.Controller.Current = Presence.Target.Present;
    }

}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Simulated
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceSimulatedState : PresenceState, IStateHandler
{
    // ===============================================================================

    public PresenceSimulatedState(PresenceStateMachine machine) : base(machine)
    {
    }

    // ===============================================================================

    public void Enter()
    {   
        SetCurrentTarget();
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }

    // ===============================================================================

    void SetCurrentTarget()
    {
        machine.Controller.Current = Presence.Target.Simulated;
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Absent
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceAbsentState : PresenceState, IStateHandler
{
    // ===============================================================================

    public PresenceAbsentState(PresenceStateMachine machine) : base(machine)
    {
    }

    // ===============================================================================

    public void Enter()
    {   
        SetCurrentTarget();
    }

    public void Update()
    {
        
    }

    public void Exit()
    {

    }

    // ===============================================================================

    void SetCurrentTarget()
    {
        machine.Controller.Current = Presence.Target.Absent;
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Disposal
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceDisposalState : PresenceState, IStateHandler
{
    readonly Actor owner;

    // ===============================================================================

    public PresenceDisposalState(PresenceStateMachine machine) : base(machine)
    {
        owner = machine.Controller.Owner;
    }

    // ===============================================================================

    public void Enter()
    {
        SetCurrentTarget();
    }

    public void Update()
    {   
        Exit();
    }

    public void Exit()
    {
        owner.Bus.Dispose();

        Object.Destroy(owner.Bridge.View);
    }

    // ===============================================================================

    void SetCurrentTarget()
    {
        machine.Controller.Current = Presence.Target.Disposal;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct PresenceTargetEvent : IMessage
{
    public readonly Presence.Target Target  { get; init; }

    public PresenceTargetEvent(Presence.Target target)
    {
        Target = target;
    }
}


public readonly struct PresenceStateEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly Presence.State State    { get; init; }

    public PresenceStateEvent(Actor owner, Presence.State state)
    {
        Owner   = owner;
        State   = state;
    }
}



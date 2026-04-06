using System.Collections.Generic;
using UnityEngine;



public class Presence : ActorService, IServiceLoop
{
    public enum Target      { Initialize,                      Present, Simulated, Absent, Disposal }
    public enum State       {             Resetting, Entering, Present, Simulated, Absent, Disposal }

        // -----------------------------------

    Dictionary<State, IStateHandler<Presence>> stateHandlers;

        // -----------------------------------

    Target target           = Target.Present;
    Target current          = Target.Initialize;

    State state             = State.Entering;

    // ===============================================================================

    public Presence(Actor actor) : base(actor)
    {
        owner.Bus.Link.Local<PresenceTargetEvent>(HandlePresenceTargetEvent);

        InitializeStateHandlers();

        Enable();

        EnterHandler();
    }

    void InitializeStateHandlers()
    {
        stateHandlers = new();

        Register(State.Entering,    new PresenceEnteringState   (owner, definition));
        Register(State.Present,     new PresencePresentState    (owner, definition));
        Register(State.Simulated,   new PresenceSimulatedState  (owner, definition));
        Register(State.Absent,      new PresenceAbsentState     (owner, definition));
        Register(State.Disposal,    new PresenceDisposalState   (owner, definition));
    }

    // ===============================================================================

    public void Loop()
    {
        AdvanceState();
        UpdateHandler();
        DebugLog();
    }

    // ===============================================================================

    void UpdateHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Update(this);
    }

        // =================================
        //  State
        // =================================

    public void TransitionTo(State newState)
    {
        ExitHandler();
        TransitionState(newState);
        EnterHandler();
    }

    void EnterHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Enter(this);

        PublishState();
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

    void AdvanceState()
    {
        if (current == target)
            return;

        if (state   == State.Disposal)
            return;

        switch (target)
        {
            case Target.Present: 
            case Target.Simulated: 
                switch(current)
                {
                    case Target.Absent: TransitionTo(State.Resetting);  break;
                    default:            TransitionTo(State.Entering);   break;
                }
            break;
            case Target.Absent:
                TransitionTo(State.Absent);
            break;
            case Target.Disposal:
                TransitionTo(State.Disposal);
            break;
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
    
    void PublishState()
    {
        owner.Bus.Emit.Local(new PresenceStateEvent(owner, state));
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

    void Register(State state, IStateHandler<Presence> handler)
    {
        stateHandlers[state] = handler;
    }
    
    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Presence);

    void DebugLog()
    {
        Log.Debug($"Presence.State.{owner.GetType().Name}", () => state, clean: true);
    }

    public Target Current           { get => current; set => current = value; }
    public Target Desired           { get => target;  set =>  target = value; }

    public UpdatePriority Priority  => ServiceUpdatePriority.Presence;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Initialize
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceResettingState : IStateHandler<Presence>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresenceResettingState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
    }

    public void Update(Presence controller)
    {
        AdvanceState(controller);
    }

    public void Exit(Presence controller)
    {
        
    }

    // ===============================================================================
    
    void AdvanceState(Presence controller)
    {
        controller.TransitionTo(Presence.State.Entering);
    }

}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Entering
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceEnteringState : IStateHandler<Presence>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresenceEnteringState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
    }

    public void Update(Presence controller)
    {
        AdvanceState(controller);
    }

    public void Exit(Presence controller)
    {
        
    }

    // ===============================================================================
    
    void AdvanceState(Presence controller)
    {
        switch(controller.Desired)
        {
            case Presence.Target.Present:   controller.TransitionTo(Presence.State.Present);   break;
            case Presence.Target.Simulated: controller.TransitionTo(Presence.State.Simulated); break;
        }
    }

}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Present
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresencePresentState : IStateHandler<Presence>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresencePresentState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
    }

    public void Update(Presence controller)
    {
        
    }

    public void Exit(Presence controller)
    {
        
    }

    // ===============================================================================

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Present;
    }

}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Simulated
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceSimulatedState : IStateHandler<Presence>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresenceSimulatedState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
    }

    public void Update(Presence controller)
    {
    }

    public void Exit(Presence controller)
    {
    }

    // ===============================================================================

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Simulated;
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Absent
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceAbsentState : IStateHandler<Presence>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresenceAbsentState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
    }

    public void Update(Presence controller)
    {
        
    }

    public void Exit(Presence controller)
    {

    }

    // ===============================================================================

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Absent;
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Disposal
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceDisposalState : IStateHandler<Presence>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresenceDisposalState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {
        SetCurrentTarget(controller);
    }

    public void Update(Presence controller)
    {   
        Exit(controller);
    }

    public void Exit(Presence controller)
    {

        if (owner is MovableDummy) Debug.Log("Exit disposal");
        owner.Bus.Dispose();
        Object.Destroy(owner.Bridge.View);
    }

    // ===============================================================================

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Disposal;
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



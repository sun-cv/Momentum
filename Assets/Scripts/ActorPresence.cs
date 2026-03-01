using System.Collections.Generic;
using UnityEngine;



public class Presence : Service, IServiceLoop
{
    public enum Target      { Present, Absent }
    public enum State       { Entering, Present, Exiting, Absent, Disposal }

        // -----------------------------------

    readonly Actor           owner;
    readonly ActorDefinition definition;

        // -----------------------------------

    Dictionary<State, StateHandler<Presence, State>> stateHandlers;

        // -----------------------------------

    Target target           = Target.Present;
    Target current          = Target.Absent;

    State state             = State.Entering;

    // ===============================================================================

    public Presence(Actor actor)
    {
        Services.Lane.Register(this);

        if (actor is not IDefined defined)
            return;

        owner       = actor;
        definition  = defined.Definition;

        owner.Emit.Link.Local<Message<Request, PresenceTargetEvent>>(HandlePresenceTargetEvent);
        owner.Emit.Link.Local<Message<Publish, PresenceStateEvent>> (HandlePresenceStateEvent);

        InitializeStateHandlers();
        EnterHandler();
    }

    void InitializeStateHandlers()
    {
        stateHandlers = new();

        Register(State.Entering,    new PresenceEnteringState(owner, definition));
        Register(State.Present,     new PresencePresentState (owner, definition));
        Register(State.Exiting,     new PresenceExitingState (owner, definition));
        Register(State.Absent,      new PresenceAbsentState  (owner, definition));
        Register(State.Disposal,    new PresenceDisposalState(owner, definition));
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
                TransitionTo(State.Entering);
            break;
            case Target.Absent:
                TransitionTo(State.Exiting);
            break;
        }
    }

    // ===============================================================================
    //  Events
    // ===============================================================================
        
    void HandlePresenceTargetEvent(Message<Request, PresenceTargetEvent> message)
    {
        target = message.Payload.Target;
    }

    void HandlePresenceStateEvent(Message<Publish, PresenceStateEvent> message)
    {
        switch(message.Payload.State)
        {
            case State.Disposal:
                Dispose();
            break;
        }
    }

    // ===============================================================================

    void Register(State state, StateHandler<Presence, State> handler)
    {
        handler.Transition += TransitionTo;
        stateHandlers[state] = handler;
    }
    
    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Presence);

    void DebugLog()
    {
        Log.Debug($"Presence.State.{owner.GetType().Name}", () => state);
    }

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public Target Current           { get => current; set => current = value; }
    public UpdatePriority Priority  => ServiceUpdatePriority.Presence;
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                     State Handlers                                       
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Entering
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceEnteringState : StateHandler<Presence, Presence.State>
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

    public override void Enter(Presence controller)
    {   
        PublishStateChange();
    }

    public override void Update(Presence controller)
    {
        controller.TransitionTo(Presence.State.Present);
    }

    public override void Exit(Presence controller)
    {
        
    }

    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Entering));
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Present
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresencePresentState : StateHandler<Presence, Presence.State>
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

    public override void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
        PublishStateChange();
    }

    public override void Update(Presence controller)
    {
        
    }

    public override void Exit(Presence controller)
    {
        
    }

    // ===============================================================================

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Present;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Present));
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Exiting
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceExitingState : StateHandler<Presence, Presence.State>
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresenceExitingState(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public override void Enter(Presence controller)
    {   
        PublishStateChange();
    }

    public override void Update(Presence controller)
    {
        if (CanBeAbsent())
            controller.TransitionTo(Presence.State.Absent);
        else
            controller.TransitionTo(Presence.State.Disposal);
    }

    public override void Exit(Presence controller)
    {
    }

    // ===============================================================================
    //  PREDICATES
    // ===============================================================================

    bool CanBeAbsent()
    {
        return definition.Presence.CanBeSetAbsent;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Exiting));
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Absent
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceAbsentState : StateHandler<Presence, Presence.State>
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

    public override void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
        PublishStateChange();
    }

    public override void Update(Presence controller)
    {
        
    }

    public override void Exit(Presence controller)
    {

    }

    // ===============================================================================

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Absent;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Absent));
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Disposal
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PresenceDisposalState : StateHandler<Presence, Presence.State>
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

    public override void Enter(Presence controller)
    {   
        PublishStateChange();

        owner.Emit.Dispose();
    
        Object.Destroy(owner.Bridge.View);
    }

    public override void Update(Presence controller)
    {   
    }

    public override void Exit(Presence controller)
    {
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Disposal));
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct PresenceTargetEvent
{
    public readonly Presence.Target Target  { get; init; }

    public PresenceTargetEvent(Presence.Target target)
    {
        Target = target;
    }
}


public readonly struct PresenceStateEvent
{
    public readonly Actor Owner             { get; init; }
    public readonly Presence.State State    { get; init; }

    public PresenceStateEvent(Actor owner, Presence.State state)
    {
        Owner   = owner;
        State   = state;
    }
}



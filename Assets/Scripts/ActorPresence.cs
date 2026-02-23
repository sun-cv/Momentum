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

    Dictionary<State, IPresenceStateHandler> stateHandlers;

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
        stateHandlers = new()
        {
            { State.Entering,   new EnteringStateHandler(owner, definition) },
            { State.Present,    new PresentStateHandler (owner, definition) },
            { State.Exiting,    new ExitingStateHandler (owner, definition) },
            { State.Absent,     new AbsentStateHandler  (owner, definition) },
            { State.Disposal,   new DisposalStateHandler(owner, definition) },
        };
    }

    // ===============================================================================

    public void Loop()
    {
        AdvanceState();
        LoopHandler();
        DebugLog();
    }

    // ===============================================================================

    void LoopHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Loop(this);
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

public interface IPresenceStateHandler
{
    void Enter(Presence controller);
    void Loop (Presence controller);
    void Exit (Presence controller);
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Entering
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class EnteringStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public EnteringStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        PublishStateChange();
    }

    public void Loop(Presence controller)
    {
        controller.TransitionTo(Presence.State.Present);
    }

    public void Exit(Presence controller)
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

public class PresentStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public PresentStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
        PublishStateChange();
    }

    public void Loop(Presence controller)
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

public class ExitingStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public ExitingStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        PublishStateChange();
    }

    public void Loop(Presence controller)
    {
        if (CanBeAbsent())
            controller.TransitionTo(Presence.State.Absent);
        else
            controller.TransitionTo(Presence.State.Disposal);
    }

    public void Exit(Presence controller)
    {
    }

    // ===============================================================================
    //  PREDICATES
    // ===============================================================================

    bool ExitingComplete()
    {
        return true;
    }

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

public class AbsentStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public AbsentStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
        PublishStateChange();
    }

    public void Loop(Presence controller)
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

public class DisposalStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    // ===============================================================================

    public DisposalStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }

    // ===============================================================================

    public void Enter(Presence controller)
    {   
        PublishStateChange();

        owner.Emit.Dispose();
    
        Object.Destroy(owner.Bridge.View);
    }

    public void Loop(Presence controller)
    {   
    }

    public void Exit(Presence controller)
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



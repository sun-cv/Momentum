using System.Collections.Generic;
using UnityEngine;





public class Presence : Service, IServiceLoop
{
    readonly Logger Log = Logging.For(LogSystem.Presence);

    public enum Target      { Present, Absent }
    public enum State       { Entering, Present, Exiting, Absent, Disposal }

    Actor           owner;
    ActorDefinition definition;

    Target target           = Target.Present;
    Target current          = Target.Absent;

    State state             = State.Entering;

    Dictionary<State, IPresenceStateHandler> stateHandlers;

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


    public void Loop()
    {
        AdvanceState();
        LoopHandler();
        DebugLog();
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

    void LoopHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Loop(this);
    }

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

    void TransitionState(State newState)
    {
        state = newState;
    }

    void ExitHandler()
    {
        if (stateHandlers.TryGetValue(state, out var handler))
            handler.Exit(this);
    }

    void HandlePresenceTargetEvent(Message<Request, PresenceTargetEvent> message)
    {
        target = message.Payload.Target;
    }

    void HandlePresenceStateEvent(Message<Publish, PresenceStateEvent> message)
    {
        switch(message.Payload.State)
        {
            case Presence.State.Disposal:
                Dispose();
            break;
        }
    }
    
    void DebugLog()
    {
        Log.Debug($"Presence.State.{owner.GetType().Name}",        () => state);
    }

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public Target Current           { get => current; set => current = value; }
    public UpdatePriority Priority  => ServiceUpdatePriority.Presence;

}

// ============================================================================
// STATE HANDLERS
// ============================================================================


public interface IPresenceStateHandler
{
    void Enter(Presence controller);
    void Loop (Presence controller);
    void Exit (Presence controller);
}

// ============================================================================
// ENTERING STATE HANDLER
// ============================================================================

public class EnteringStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    public EnteringStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }


    public void Enter(Presence controller)
    {   
        PublishStateChange();
        if (owner is Hero)
    }

    public void Loop(Presence controller)
    {
        if (owner is Hero)

        controller.TransitionTo(Presence.State.Present);
    }

    public void Exit(Presence controller)
    {
        
    }

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Entering));
    }
}

// ============================================================================
// PRESENT STATE HANDLER
// ============================================================================

public class PresentStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    public PresentStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }


    public void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
        PublishStateChange();

        if (owner is Hero)
    }

    public void Loop(Presence controller)
    {
        if (owner is Hero)
    }

    public void Exit(Presence controller)
    {
        
    }

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Present;
    }

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Present));
    }
}

// ============================================================================
// EXITING STATE HANDLER
// ============================================================================

public class ExitingStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    public ExitingStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }


    public void Enter(Presence controller)
    {   
        PublishStateChange();
            if (owner is Hero)
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


    // ============================================================================
    //  PREDICATES
    // ============================================================================

    bool ExitingComplete()
    {
        return true;
    }

    bool CanBeAbsent()
    {
        return definition.Presence.EnableAbsentState;
    }

    // ============================================================================
    //  HELPERS
    // ============================================================================

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Exiting));
    }
}

// ============================================================================
// ABSENT STATE HANDLER
// ============================================================================

public class AbsentStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    public AbsentStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }


    public void Enter(Presence controller)
    {   
        SetCurrentTarget(controller);
        PublishStateChange();
            if (owner is Hero)
    }

    public void Loop(Presence controller)
    {
        
    }

    public void Exit(Presence controller)
    {

    }

    void SetCurrentTarget(Presence controller)
    {
        controller.Current = Presence.Target.Absent;
    }

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Absent));
    }
}

// ============================================================================
// DISPOSAL STATE HANDLER
// ============================================================================

public class DisposalStateHandler : IPresenceStateHandler
{
    readonly Actor owner;
    readonly ActorDefinition definition;

    public DisposalStateHandler(Actor owner, ActorDefinition definition)
    {
        this.owner      = owner;
        this.definition = definition;
    }


    public void Enter(Presence controller)
    {   
            if (owner is Hero)
            
        PublishStateChange();

        // owner.Emit.Dispose();
    
        Object.Destroy(owner.Bridge.View);
    }

    public void Loop(Presence controller)
    {   
    }

    public void Exit(Presence controller)
    {
    }

    void PublishStateChange()
    {
        owner.Emit.Local(Publish.Transitioned, new PresenceStateEvent(owner, Presence.State.Disposal));
    }
}



// ============================================================================
// PRESENCE EVENTS
// ============================================================================


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



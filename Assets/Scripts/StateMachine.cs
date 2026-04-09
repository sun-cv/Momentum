using System;
using System.Collections.Generic;



public abstract class StateMachine<TState>
{
    TState state;

    readonly Action onPublish;
    readonly Dictionary<TState, IStateHandler> handlers = new();

    public StateMachine(Action publish)
    {
        onPublish = publish;
    }

    public void Initialize(TState state)
    {
        this.state = state;
        EnterHandler();
    }

    public void Update()
    {
        if (handlers.TryGetValue(state, out var handler))
            handler.Update();
    }

    public void TransitionTo(TState newState)
    {
        ExitHandler();
        TransitionState(newState);
        EnterHandler();
    }

    void ExitHandler()
    {
        if (handlers.TryGetValue(state, out var handler))
            handler.Exit();
    }

    void TransitionState(TState newState)
    {
        state = newState;
    }

    void EnterHandler()
    {
        if (handlers.TryGetValue(state, out var handler))
            handler.Enter();

        PublishState();
    }

    void PublishState()
    {
        onPublish?.Invoke();
    }

    public void Register(TState state, IStateHandler handler)
    {
        handlers[state] = handler;
    }

    public bool Is(TState state)
    {
        return this.state.Equals(state);
    }

    public bool IsNot(TState state)
    {
        return !this.state.Equals(state);
    }

    public TState State => state;
}


public abstract class MachineState<TState, TMachine> where TMachine : StateMachine<TState>
{ 
    protected readonly TMachine machine;

    public MachineState(TMachine machine)
    {
        this.machine = machine;
    }
}


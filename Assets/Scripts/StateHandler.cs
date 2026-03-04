using System;

public abstract class StateHandler<TController, TState> : IStateHandler<TController>
{
    public Action<TState> Transition;

    public abstract void Enter  (TController controller);
    public abstract void Update (TController controller);
    public abstract void Exit   (TController controller);
}

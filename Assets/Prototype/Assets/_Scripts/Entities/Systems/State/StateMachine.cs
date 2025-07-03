

using character;

public abstract class BaseStateMachine<Context> : BaseState<Context>
{
    public BaseState<Context> ActiveState { get; protected set; }
    public BaseState<Context> QueuedState { get; protected set; }
    public BaseState<Context> CachedState { get; protected set; }

    public virtual void Initialize(BaseState<Context> _initialState)
    {
        ActiveState = _initialState;
        ActiveState.Enter();
    }

    public virtual void Set(BaseState<Context> _state)
    {
        QueuedState = _state;
        CachedState = ActiveState;

        ActiveState.Exit();
        ActiveState = QueuedState;
        ActiveState.Enter();

        QueuedState = null;
    }

    public override void Tick()
    {
        ActiveState?.Tick();
    }

    public override void TickFixed()
    {
        ActiveState?.TickFixed();
    }
}
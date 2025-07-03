

namespace character
{

public class StateMachine : BaseStateMachine<Context>
{
    public Context   Context    { get; private set; }
    public Character Character  { get; private set; }

    public StateMachine(Character _character)
    {
        Character   = _character;
        Context     = _character.Context;
    }

    public virtual void Initialize(State _state)
    {
        ActiveState = _state;
        ActiveState.Enter();
    }


    public virtual void Set(State _state)
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
        ActiveState.Tick();
    }

    public override void TickFixed()
    {
        ActiveState.TickFixed();
    }
}
}
using UnityEngine;


public class BaseStateMachine<Owner> : BaseState<Owner>
{

    public BaseState<Owner> QueuedState   { get; protected set; }
    public BaseState<Owner> ActiveState   { get; protected set; }
    public BaseState<Owner> CachedState   { get; protected set; }

    public BaseStateMachine(Owner _owner) : base(_owner) {}

    public virtual void Initialize(BaseState<Owner> _state)
    {
        ActiveState = _state;
        ActiveState.Enter();
    }

    public virtual void Set(BaseState<Owner> _state)
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

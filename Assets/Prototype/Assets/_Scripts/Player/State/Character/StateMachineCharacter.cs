using state.character.attached;
using state.character.detached;

namespace state.character
{
    


public class StateMachineCharacter : StateMachine
{

    public StateAttached StateAttached   { get; protected set; }
    public StateDetached StateDetached   { get; protected set; }

    public StateMachineCharacter(Character _character) : base(_character)
    {
        StateAttached = new StateAttached(_character);
        StateDetached = new StateDetached(_character);

        StateAttached.Reference(this);
        StateDetached.Reference(this);
    }

    public override void Initialize(State _state)
    {
        ActiveState = _state;
        ActiveState.Enter();
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
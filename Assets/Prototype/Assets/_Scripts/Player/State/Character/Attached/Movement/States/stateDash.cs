
namespace state.character.attached.movement
{
    

public class StateDash : State
{
    public StateDash(Character _character) : base(_character) {}
    
    public void Reference(StateMachineMovement _stateMachine)
    {
        stateMachine = _stateMachine;
    }   
}
}
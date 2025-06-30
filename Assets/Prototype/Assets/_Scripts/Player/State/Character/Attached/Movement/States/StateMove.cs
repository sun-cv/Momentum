
namespace state.character.attached.movement
{
    

public class StateMove : State
{
    public StateMove(Character _character) : base(_character) {}
    public new StateMachineMovement stateMachine;

    public override void Enter()
    {
    }

    public void Reference(StateMachineMovement _stateMachine)
    {
        stateMachine = _stateMachine;
    }   

    public override void Tick()
    {
        AdvanceState();
    }


    public override void AdvanceState()
    {
        if (ShouldTransitionToStateIdle())
        {
            stateMachine.Set(stateMachine.StateIdle);
        }

        if (ShouldTransitionToStateDash())
        {
            stateMachine.Set(stateMachine.StateDash);
        }

    }

    private bool ShouldTransitionToStateIdle()
    {
        return !context.Movement.IntentMove;
    }

    private bool ShouldTransitionToStateDash()
    {
        return context.Movement.RequestDash.Consume();
    }

}
}
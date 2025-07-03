using character.state;



namespace character
{
    

public class SprintState : State
{
    protected MovementStateMachine stateMachine;

    public SprintState(MovementStateMachine _stateMachine) : base(_stateMachine.Character) 
    {
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        character.Movement.Sprint.Start();
        context  .Movement.Sprint.Active.Set();
    }

    public override void Exit()
    {
        character.Movement.Sprint.Stop();
        context  .Movement.Sprint.Active.Clear();        
    }

    public override void Tick()
    {
        IsInterrupted();
        AdvanceState();
    }

    public void IsInterrupted()
    {
        
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
        return !context.Movement.Intent;
    }

    private bool ShouldTransitionToStateDash()
    {
        return context.Movement.Dash.Request.Consume();
    }

}
}
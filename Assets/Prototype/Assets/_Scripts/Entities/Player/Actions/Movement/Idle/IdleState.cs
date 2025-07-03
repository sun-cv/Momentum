using character.state;


namespace character
{
    

public class IdleState : State
{
    protected MovementStateMachine stateMachine;

    public IdleState(MovementStateMachine _stateMachine) : base(_stateMachine.Character) 
    {
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        character.Movement.Idle.Start();
        context  .Movement.Idle.Active.Set();
    }

    public override void Exit()
    {
        character.Movement.Idle.Stop();
        context  .Movement.Idle.Active.Clear();
    }

    public override void Tick()
    {
        AdvanceState();
    }


    public override void AdvanceState()
    {
        if (ShouldTransitionToSprintState())
        {
            stateMachine.Set(stateMachine.StateSprint);
        }

        if (ShouldTransitionToDashState())
        {
            stateMachine.Set(stateMachine.StateDash);
        }

    }

    private bool ShouldTransitionToSprintState()
    {
        return context.Movement.Intent;
    }

    private bool ShouldTransitionToDashState()
    {
        return context.Movement.Dash.Request.Consume();
        //  && character.Movement.Dash.Valid();
    }

}
}
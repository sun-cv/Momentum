using UnityEngine;
using character.state;
using character.context;


namespace character
{
    

public class DashState : State
{
    protected MovementStateMachine stateMachine;

    private DashContext dash;

    private float dashTimer;
    private float dashLength = 0.15f;
    private bool  DashActive => dashTimer < dashLength;


    public DashState(MovementStateMachine _stateMachine) : base(_stateMachine.Character) 
    {
        stateMachine = _stateMachine;
        dash         = context.Movement.Dash;
    }

    public override void Enter()
    {
        character.Movement.Dash.Start();
        context  .Movement.Dash.Active.Set();

        dashTimer = 0f;
    }

    public override void Exit()
    {
        character.Movement.Dash.Stop();
        context  .Movement.Dash.Active.Clear();        
    }

    public override void Tick()
    {
        IsInterrupted();
        ControlDash();
        AdvanceState();
    }

    public void ControlDash()
    {
        dashTimer += Time.deltaTime;
    }

    public void IsInterrupted()
    {
        
    }

    public override void AdvanceState()
    {
        if (DashActive)
        {
            return;
        }

        if (ShouldTransitionToStateIdle())
        {
            stateMachine.Set(stateMachine.StateIdle);
        }

        if (ShouldTransitionToStateSprinting())
        {
            stateMachine.Set(stateMachine.StateSprint);
        }

        if (!ShouldTransitionToStateIdle() && !ShouldTransitionToStateSprinting())
        {
            stateMachine.Set(stateMachine.StateIdle);
        }


    }

    private bool ShouldTransitionToStateIdle()
    {
        return !context.Movement.Intent;
    }

    private bool ShouldTransitionToStateSprinting()
    {
        return context.Movement.Intent;
    }

}
}
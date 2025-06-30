
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace state.character.attached.movement
{
    

public class StateIdle : State
{
    public StateIdle(Character _character) : base(_character) {}
    public new StateMachineMovement stateMachine;

    public override void Enter()
    {
        context.Movement.SetDirection(Vector2.zero);
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
        if (ShouldTransitionToStateMove())
        {
            stateMachine.Set(stateMachine.StateMove);
        }

        if (ShouldTransitionToStateDash())
        {
            stateMachine.Set(stateMachine.StateDash);
        }

    }

    private bool ShouldTransitionToStateMove()
    {
        return context.Movement.IntentMove;
    }

    private bool ShouldTransitionToStateDash()
    {
        return context.Movement.RequestDash.Consume();
    }

}
}
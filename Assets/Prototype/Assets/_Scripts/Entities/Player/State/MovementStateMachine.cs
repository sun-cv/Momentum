

namespace character.state
{


public class MovementStateMachine : StateMachine
{
    public CharacterStateMachine StateMachine   { get; protected set; }


    public IdleState    StateIdle               { get; protected set; }
    public DashState    StateDash               { get; protected set; }
    public SprintState  StateSprint             { get; protected set; }

    public MovementStateMachine(CharacterStateMachine _stateMachine) : base(_stateMachine.Character)
    {
        StateMachine    = _stateMachine;

        StateIdle       = new IdleState(this);
        StateDash       = new DashState(this);
        StateSprint     = new SprintState(this);
    }
    
}
}
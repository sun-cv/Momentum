


namespace character.state
{
    
public class CharacterStateMachine : StateMachine
{

    public MovementStateMachine Movement { get; protected set; }

    public CharacterStateMachine(Character _character) : base(_character)
    {
        Movement = new MovementStateMachine(this);
        Movement.Initialize(Movement.StateIdle);
    }


}
}
using state.character.attached.movement;

namespace state.character.attached
{


public class StateAttached : State
{

    public StateMachineMovement StateMachineMovement { get; protected set; }

    public StateAttached(Character _character) : base(_character) 
    {
        StateMachineMovement = new StateMachineMovement(_character);

        StateMachineMovement.Initialize(StateMachineMovement.StateIdle);
    }

    public override void Tick()
    {
    
        StateMachineMovement.Tick();

    }

    public override void TickFixed()
    {
        StateMachineMovement.TickFixed();
    }
}
}
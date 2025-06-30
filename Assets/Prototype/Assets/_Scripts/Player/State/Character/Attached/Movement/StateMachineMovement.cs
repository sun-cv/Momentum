using UnityEngine;

namespace state.character.attached.movement
{


public class StateMachineMovement : StateMachine
{
    
    public StateIdle StateIdle { get; protected set; }
    public StateMove StateMove { get; protected set; }
    public StateDash StateDash { get; protected set; }

    public StateMachineMovement(Character _character) : base(_character)
    {
        StateIdle = new StateIdle(_character);
        StateMove = new StateMove(_character);
        StateDash = new StateDash(_character);

        StateIdle.Reference(this);
        StateMove.Reference(this);
        StateDash.Reference(this);
    }
    
    

}
}
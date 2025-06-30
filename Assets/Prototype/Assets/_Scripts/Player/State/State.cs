using UnityEngine;

namespace state.character {



public class State : BaseState<Character>
{

    protected Character character;
    protected CharacterContext context;

    public State(Character _character) : base(_character)
    {
        character   = _character;
        context     = _character.Context;
    }

    public virtual void Reference(StateMachine _stateMachine)
    {
        stateMachine = _stateMachine;
    }

}
}


namespace character
{


public class State : BaseState<Context>
{
    protected Character character;

    public State(Character _character)
    {
        character   = _character;
        context     = _character.Context;
    }
}
}
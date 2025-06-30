
namespace state.character {

public class StateMachine : BaseStateMachine<Character>
{

    protected Character character;
    protected CharacterContext context;

    public StateMachine(Character _character) : base(_character)
    {
        character = _character;
        context   = _character.Context;
    }

    public virtual void Initialize(State _state)
    {
        ActiveState = _state;
        ActiveState.Enter();
    }

    public virtual void Set(State _state)
    {
        QueuedState = _state;
        CachedState = ActiveState;

        ActiveState.Exit();
        ActiveState = QueuedState;
        ActiveState.Enter();

        QueuedState = null;
    }

    public override void Tick()
    {
        ActiveState.Tick();

    }


    public override void TickFixed()
    {
        ActiveState.TickFixed();
    }
}
}
using UnityEngine;




public class CharacterStateMachine
{

    public Character Character          { get; private set; }
    
    public CharacterState QueuedState   { get; private set; }
    public CharacterState ActiveState   { get; private set; }
    public CharacterState CachedState   { get; private set; }
    public CharacterState CancelState   { get; private set; }
    public CharacterState FrozenState   { get; private set; }
    public CharacterState ForcedState   { get; private set; }

    public CharacterStateAttached stateAttached;
    public CharacterStateDetached stateDetached;

    public CharacterStateMachine(Character _character)
    {
        Character = _character;
    
        stateAttached = new CharacterStateAttached(Character, this);
        stateDetached = new CharacterStateDetached(Character, this);
    }

    public void Initialize(CharacterState _state)
    {
        ActiveState = _state;
        ActiveState.Enter();
    }

    public void Set(CharacterState _state)
    {
        QueuedState = _state;
        CachedState = ActiveState;

        ActiveState.Exit();
        ActiveState = QueuedState;
        ActiveState.Enter();

        QueuedState = null;
    }

    public void Cancel(CharacterState _state)
    {
        if (!ActiveState.IsCancellable)
        {
            return;
        }

        QueuedState = _state;
        CancelState = ActiveState;
        ActiveState = QueuedState;

        ActiveState.Enter();        
    }

}

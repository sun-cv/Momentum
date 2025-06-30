using UnityEngine;
using state.character;


public class Character : MonoBehaviour
{
    public CharacterContext Context                                 { get; private set; }

    public StateMachineCharacter StateMachine                       { get; private set; }

    [Header("Character Components")]
    [field: SerializeField] public Rigidbody2D          Body        { get; private set; }
    [field: SerializeField] public Collider2D           Hitbox      { get; private set; }
    [field: SerializeField] public CharacterMovement    Movement    { get; private set; }
    [field: SerializeField] public CharacterHealth      Health      { get; private set; }

    [Header("Character weapons")]
    [SerializeField] private Shield             shield;


    public void Initialize()
    {
        Context         = new CharacterContext(CharacterContextCore.From(this));
        StateMachine    = new StateMachineCharacter(this);

        StateMachine.Initialize(StateMachine.StateAttached);

        Movement    .Initialize(Context);
        Health      .Initialize(Context);
    }


    public void Tick()
    {
        StateMachine.Tick();
    }


    public void TickFixed()
    {
        StateMachine.TickFixed();
    }


    private void OnDisable()
    {
        Context.Deconstruct();
    }
}

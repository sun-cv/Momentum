using UnityEngine;

using character;
using character.state;
using character.context;


public class Character : MonoBehaviour
{
    public Config  Config                                           { get; private set; }
    public Context Context                                          { get; private set; }

    public CharacterStateMachine StateMachine                       { get; private set; }

    [Header("Character Components")]
    [field: SerializeField] public Rigidbody2D          Body        { get; private set; }
    [field: SerializeField] public Collider2D           Hitbox      { get; private set; }
    [field: SerializeField] public Movement             Movement    { get; private set; }
    [field: SerializeField] public Health               Health      { get; private set; }


    public void Initialize()
    {
        Context         = new Context(ContextCore.From(this));

        Movement    .Initialize(Context);
        Health      .Initialize(Context);

        StateMachine    = new CharacterStateMachine(this);
        StateMachine.Initialize(StateMachine.Movement);
    }


    public void Tick()
    {
        Movement.Tick();
        StateMachine.Tick();
    }


    public void TickFixed()
    {
        Movement.TickFixed();
        StateMachine.TickFixed();
    }


    private void OnDisable()
    {
        Context.Deconstruct();
    }
}

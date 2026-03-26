


using Mono.Cecil;

public class Dummy : Agent, IDummy
{
    public Resources            Resource            { get; private set; }
    public Presence             Presence            { get; private set; }
    public Lifecycle            Lifecycle           { get; private set; }
    public AnimationSystem      Animation           { get; private set; }

    //========================================
    //  Properties
    //========================================

    public float Health                             { get => Resource.Health; }
    public float MaxHealth                          { get; } = 100;

    //========================================
    //  State
    //========================================

    public bool Inactive                            { get; set; }
    public bool Invulnerable                        { get; set; }
    public bool Impervious                          { get; set; }

    public bool Alive                               => Lifecycle.IsAlive;
    public bool Dead                                => Lifecycle.IsDead;

    public bool Disabled                            { get; set; }
    public bool Stunned                             { get; set; }
    
    public void Initialize(ActorDefinition definition)
    {
        Definition  = definition;

        Bus         = new();

        Resource    = new(this);
        Animation   = new(this);
        Presence    = new(this);
        Lifecycle   = new(this);
    }
}


public class DummyCorpse : Actor, ICorpse
{
    public Presence             Presence            { get; set; }
    public Corpse               Corpse              { get; set; }
    public AnimationSystem      Animation           { get; set; }

    public Corpse.State Condition                   => Corpse.Condition;

    public void Initialize(ActorDefinition definition)
    {
        Definition = definition;

        Presence    = new(this);
        Corpse      = new(this);
        Animation   = new(this);
    }
}




public class Dummy : Agent, IDummy
{
    public Presence             Presence            { get; private set; }
    public Lifecycle            Lifecycle           { get; private set; }
    public AnimationSystem      Animation           { get; private set; }

    //========================================
    //  Properties
    //========================================

    public float MaxHealth                          { get; set; }
    public float Health                             { get; set; }

    //========================================
    //  State
    //========================================

    public bool Inactive                            { get; set; } = false;
    public bool Invulnerable                        { get; set; } = false;
    public bool Impervious                          { get; set; } = false;

    public bool Alive                               => Lifecycle.IsAlive;
    public bool Dead                                => Lifecycle.IsDead;

    public bool Disabled                            { get; set; } = false;
    public bool Stunned                             { get; set; } = false;
    
    public void Initialize(ActorDefinition definition)
    {
        Definition  = definition;
        MaxHealth   = definition.Stats.MaxHealth;

        Emit        = new();

        Animation   = new(this);
        Presence    = new(this);
        Lifecycle   = new(this);
    }
}


public class DummyCorpse : Actor, IDefined, ICorpse
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
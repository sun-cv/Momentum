





using UnityEngine;

public class Dummy : Agent, IDummy
{
    public ActorDefinition      Definition          { get; private set; }
    public AnimatorController   Animator            { get; private set; }
    public Presence             Presence            { get; private set; }
    public Lifecycle            Lifecycle           { get; private set; }

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



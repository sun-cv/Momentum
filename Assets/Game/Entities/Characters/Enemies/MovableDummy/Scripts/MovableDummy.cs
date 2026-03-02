using UnityEngine;



public class MovableDummy : Agent, IMovableDummy
{
    public MovementEngine       Movement            { get; private set; }
    public Presence             Presence            { get; private set; }
    public Lifecycle            Lifecycle           { get; private set; }
    public AnimationSystem      Animation           { get; private set; }

    //========================================
    //  Accessors
    //========================================

    public float MaxHealth                          { get; set; }
    public float Health                             { get; set; }

    public float Speed                              { get; set; }
    public float SpeedMultiplier                    { get; set; }

    public float Mass                               { get; set; } = 100;

    //========================================
    //  State
    //========================================

    public bool Inactive                            { get; set; }
    public bool Invulnerable                        { get; set; }
    public bool Impervious                          { get; set; }
    public bool ImmuneToForce                       { get; set; }

    public bool Alive                               => Lifecycle.IsAlive;
    public bool Dead                                => Lifecycle.IsDead;

    public bool Disabled                            { get; set; }
    public bool Stunned                             { get; set; }

    public bool Constrained                         { get; set; }

    public bool CanMove                             { get; set; }
    public bool CanRotate                           { get; set; }

    public Direction Facing                         { get; set; }
    public Direction Direction                      { get; set; }
    public Direction LastDirection                  { get; set; }
    
    public Direction LockedFacing                   { get; set; }
    public Direction LockedDirection                { get; set; }

    public Vector2 Velocity                         => Movement.Velocity;
    public Vector2 Momentum                         => Movement.Momentum;
    
    public Vector2 Normal                           { get; set; }
    public Vector2 Force                            { get; set; }

    public bool IsMoving                            { get; set; }
    public TimePredicate IsIdle                     { get; set; }


    
    public void Initialize(ActorDefinition definition)
    {
        Definition  = definition;
        MaxHealth   = definition.Stats.MaxHealth;

        Emit        = new();

        Movement    = new(this);
        Animation   = new(this);
        Presence    = new(this);
        Lifecycle   = new(this);
    }
}


public class MovableDummyCorpse : Actor, IDefined, ICorpse
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
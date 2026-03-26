using UnityEngine;



public class MovableDummy : Agent, IMovableDummy
{
    public Resources            Resource            { get; private set; }
    public Movement             Movement            { get; private set; }
    public Presence             Presence            { get; private set; }
    public Lifecycle            Lifecycle           { get; private set; }
    public AnimationSystem      Animation           { get; private set; }

    //========================================
    //  Accessors
    //========================================

    public float Health                             => Resource.Health; 
    public float MaxHealth                          => Definition.Stats.MaxHealth;

    public float Speed                              { get; }
    public float SpeedMultiplier                    { get; }

    public float Impact                             { get; } = 1;

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
    public Direction CommandDirection               { get; set; }
    
    public Direction ResolvedFacing                 { get; set; }
    public Direction ResolvedDirection              { get; set; }

    public float Friction                           => Definition.Physics.Friction;
    public float Mass                               => Definition.Physics.Mass;

    public Vector2 Velocity                         { get; set; }
    public Vector2 Control                          { get; set; }
    public Vector2 Momentum                         { get; set; }
    
    public Vector2 Normal                           { get; set; }
    public Vector2 Force                            { get; set; }

    public bool IsMoving                            { get; set; }
    public TimePredicate IsIdle                     { get; set; }


    
    public void Initialize(ActorDefinition definition)
    {
        Definition  = definition;

        Bus         = new();

        Resource    = new(this);
        Movement    = new(this);
        Animation   = new(this);
        Presence    = new(this);
        Lifecycle   = new(this);
    }
}


public class MovableDummyCorpse : Actor, ICorpse
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

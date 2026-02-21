using UnityEngine;



public class Hero : Player, IHero
{
    // ===============================================================================
    //  Systems
    // ===============================================================================
    
    public ActorDefinition      Definition          { get; set; }
    public IntentSystem         Intent              { get; set; }
    public HeroState            State               { get; set; }
    public ActorStats           Stats               { get; set; }
    public EquipmentManager     Equipment           { get; set; }
    public WeaponSystem         Weapons             { get; set; }
    public MovementEngine       Movement            { get; set; }
    public EffectRegister       Effects             { get; set; }
    public Lifecycle            Lifecycle           { get; set; }
    public Presence             Presence            { get; set; }
    public AnimationSystem      Animation           { get; set; }


    // ===============================================================================
    //  Accessors
    // ===============================================================================

    public float MaxHealth                          { get => Stats.MaxHealth;               }
    public float Health                             { get => Stats.Health;  
                                                      set => Stats.Health           = value;}
    public float MaxMana                            { get => Stats.MaxMana;                 }
    public float Mana                               { get => Stats.Mana;    
                                                      set => Stats.Mana             = value;}
    public float Speed                              { get => Stats.Speed;                   }
    public float SpeedMultiplier                    { get => Stats.SpeedMultiplier;         }
    public float Attack                             { get => Stats.Attack;                  }
    public float AttackMultiplier                   { get => Stats.AttackMultiplier;        }

    public float Mass                               { get => Stats.Mass;                    }


    // ===============================================================================
    //  State
    // ===============================================================================

    public bool Inactive                            { get => State.Inactive;        
                                                      set => State.Inactive         = value;}
    public bool Invulnerable                        { get => State.Invulnerable;    
                                                      set => State.Invulnerable     = value;}
    public bool Impervious                          { get => State.Impervious;    
                                                      set => State.Impervious       = value;}

    public bool Alive                               => State.Alive;
    public bool Dead                                => State.Dead;
    
    public bool Disabled                            => State.Disabled;
    public bool Stunned                             { get => State.Stunned;         
                                                      set => State.Stunned          = value;}

    public bool Parrying                            => State.Parrying;
    public bool Blocking                            => State.Blocking;

    public bool CanMove                             => State.CanMove;
    public bool CanAttack                           => State.CanAttack;
    public bool CanRotate                           => State.CanRotate; 

    public Direction Aim                            => State.Aim;
    public Direction Facing                         => State.Facing;
    public Direction Direction                      => State.Direction;
    public Direction LastDirection                  => State.LastDirection;

    public Direction LockedAim                      => State.LockedAim;
    public Direction LockedFacing                   => State.LockedFacing;
    public Direction LockedDirection                => State.LockedDirection;

    public Vector2 Velocity                         => State.Velocity;
    public Vector2 Momentum                         => State.Momentum;
    
    public bool IsMoving                            => State.IsMoving;
    public TimePredicate IsIdle                     => State.IsIdle;


    // ===============================================================================

    public void Initialize(ActorDefinition definition)
    {
        Definition  = definition;

        Emit        = new();

        Intent      = new(this);
        Stats       = new(this);
        Equipment   = new(this);
        Weapons     = new(this);
        Movement    = new(this);
        Effects     = new(this);
        Animation   = new(this);
        Presence    = new(this);
        Lifecycle   = new(this);

        State       = new(this);
    }
}


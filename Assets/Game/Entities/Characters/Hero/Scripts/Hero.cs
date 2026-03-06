using Mono.Cecil;
using UnityEngine;



public class Hero : Player, IHero
{
    // ===============================================================================
    //  Systems
    // ===============================================================================
    
    public IntentSystem         Intent              { get; set; }
    public HeroState            State               { get; set; }
    public ActorStats           Stats               { get; set; }
    public Resources            Resource            { get; set; }
    public EquipmentManager     Equipment           { get; set; }
    public WeaponSystem         Weapons             { get; set; }
    public Movement             Movement            { get; set; }
    public EffectRegister       Effects             { get; set; }
    public Presence             Presence            { get; set; }
    public Lifecycle            Lifecycle           { get; set; }
    public AnimationSystem      Animation           { get; set; }


    // ===============================================================================
    //  Accessors
    // ===============================================================================

    public float Health                             { get => Resource.Health;                                               }
    public float MaxHealth                          { get => Stats.MaxHealth;                                               }
    public float Armor                              { get => Resource.Armor;                                                }
    public float MaxArmor                           { get => Stats.MaxArmor;                                                }
    public float Shield                             { get => Resource.Shield;                                               }
    public float MaxShield                          { get => Stats.MaxShield;                                               }
    public float Mana                               { get => Resource.Mana;                                                 }
    public float MaxMana                            { get => Stats.MaxMana;                                                 }
    public float Strength                           { get => Stats.Strength;                                                }
    public float StrengthMultiplier                 { get => Stats.StrengthMultiplier;                                      }
    public float Speed                              { get => Stats.Speed;                                                   }
    public float SpeedMultiplier                    { get => Stats.SpeedMultiplier;                                         }


    // ===============================================================================
    //  State
    // ===============================================================================

    public bool Inactive                            { get => State.Inactive;            set => State.Inactive       = value;}
    public bool Invulnerable                        { get => State.Invulnerable;        set => State.Invulnerable   = value;}
    public bool Impervious                          { get => State.Impervious;          set => State.Impervious     = value;}

    public bool ImmuneToForce                       => State.ImmuneToForce;


    public bool Alive                               => State.Alive;
    public bool Dead                                => State.Dead;
    
    public bool Disabled                            => State.Disabled;
    public bool Stunned                             { get => State.Stunned;             set => State.Stunned        = value;}

    public bool Constrained                         { get => State.Constrained;         set => State.Constrained    = value;}

    public bool Parrying                            => State.Parrying;
    public bool Blocking                            => State.Blocking;

    public bool CanMove                             => State.CanMove;
    public bool CanRotate                           => State.CanRotate; 
    public bool CanAttack                           => State.CanAttack;

    public Direction Aim                            => State.Aim;
    public Direction Facing                         => State.Facing;
    public Direction Direction                      => State.Direction;
    public Direction LastDirection                  => State.LastDirection;

    public Direction LockedAim                      => State.LockedAim;
    public Direction LockedFacing                   => State.LockedFacing;
    public Direction LockedDirection                => State.LockedDirection;

    public float Friction                           { get => State.Friction;                                                }
    public float Mass                               { get => Definition.Physics.Mass;                                       }

    public Vector2 Momentum                         => State.Momentum;

    public Vector2 Velocity                         { get => State.Velocity;            set => State.Velocity       = value;}
    public Vector2 Control                          { get => State.Control;             set => State.Control        = value;}
    public Vector2 Force                            { get => State.Force;               set => State.Force          = value;}
    public Vector2 Normal                           { get => State.Normal;              set => State.Normal         = value;}

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
        Presence    = new(this);
        Lifecycle   = new(this);
        Animation   = new(this);

        State       = new(this);

    }
}

public class HeroCorpse : Actor, ICorpse
{
    public Presence             Presence            { get; set; }
    public Corpse               Corpse              { get; set; }
    public AnimationSystem      Animation           { get; set; }

    public Corpse.State Condition                   => Corpse.Condition;

    public void Initialize(ActorDefinition definition)
    {
        Definition  = definition;

        Presence    = new(this);
        Corpse      = new(this);
        Animation   = new(this);
    }
}
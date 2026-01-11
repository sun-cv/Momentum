using UnityEngine;



public class Hero : Actor, IHero
{
    //========================================
    // Systems
    //========================================

    public HeroDefinition   Definition          { get; private set; }
    public HeroState        State               { get; private set; }
    public HeroStats        Stats               { get; private set; }
    public EquipmentManager Equipment           { get; private set; }
    public WeaponSystem     Weapons             { get; private set; }
    public MovementEngine   Movement            { get; private set; }
    public EffectRegister   Effects             { get; private set; }

    //========================================
    // Properties
    //========================================

    public float MaxHealth                      { get => Stats.MaxHealth;           }
    public float Health                         { get => Stats.Health;  
                                                  set => Stats.Health       = value;}
    public float MaxMana                        { get => Stats.MaxMana;             }
    public float Mana                           { get => Stats.Mana;    
                                                  set => Stats.Mana         = value;}
    public float Speed                          { get => Stats.Speed;               }
    public float SpeedMultiplier                { get => Stats.SpeedMultiplier;     }
    public float Attack                         { get => Stats.Attack;              }
    public float AttackMultiplier               { get => Stats.AttackMultiplier;    }

    //========================================
    // State
    //========================================
        
    public bool Stunned                         { get => State.Stunned;         set => State.Stunned        = value; }
    public bool Disabled                        { get => State.Disabled;        set => State.Disabled       = value; }
    public bool Invulnerable                    { get => State.Invulnerable;    set => State.Invulnerable   = value; }

    public bool CanMove                         => State.CanMove;
    public bool CanAttack                       => State.CanAttack;
    public bool CanRotate                       => State.CanRotate; 

    public CardinalDirection FacingDirection    => State.FacingDirection;
    public CardinalDirection IntentDirection    => State.IntentDirection;
    public Vector2 MovementDirection            => State.MovementDirection;
    public Vector2 Velocity                     => State.Velocity;
    public Vector2 Momentum                     => State.Momentum;

    public bool IsMoving                        => State.IsMoving;
    public TimePredicate IsIdle                 => State.IsIdle;


    public void Initialize(HeroDefinition definition)
    {
        Definition  = definition;

        Stats       = new(this);
        Equipment   = new(this);
        Weapons     = new(this);
        Movement    = new(this);
        Effects     = new(this);

        State       = new(this);

        Health      = MaxHealth;
        Mana        = MaxMana;
    }
}


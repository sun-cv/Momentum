




using UnityEngine;

public class Hero : Entity
{
    //========================================
    // Systems
    //========================================

    public HeroDefinition   Definition  { get; private set; }
    public HeroController   Character   { get; private set; }
    public HeroStats        Stats       { get; private set; }
    public EquipmentManager Equipment   { get; private set; }
    public WeaponSystem     Weapons     { get; private set; }
    public MovementEngine   Movement    { get; private set; }
    public EffectRegister   Effects     { get; private set; }

    //========================================
    // Properties
    //========================================

    public float MaxHealth              { get => Stats.MaxHealth;     }
    public float Health                 { get => Stats.Health;  
                                          set => Stats.Health = value;}
    public float MaxMana                { get => Stats.MaxMana;       }
    public float Mana                   { get => Stats.Mana;    
                                          set => Stats.Mana = value;  }
    public float Speed                  { get => Stats.Speed;         }

    //========================================
    // State
    //========================================
    
    TimePredicate idle;


    public bool CanMove                 => Effects.Get<IDisableMove>  (effect => !effect.DisableMove  ); 
    public bool CanAttack               => Effects.Get<IDisableAttack>(effect => !effect.DisableAttack); 
    public bool CanRotate               => Effects.Get<IDisableRotate>(effect => !effect.DisableRotate); 

    public bool IsDisabled;
    public bool IsStunned;
    public bool IsInvulnerable;

    public Vector2 MovementDirection    => Services.Get<InputRouter>().MovementDirection;
    public Vector2 Velocity             => Movement.Velocity;
    public Vector2 Momentum             => Movement.Momentum;

    public bool IsMoving                => Velocity != Vector2.zero;
    public TimePredicate IsIdle         => idle ??= new (() => !IsMoving);


    public void Initialize(HeroController controller, HeroDefinition data)
    {
        Definition  = data;
        Character   = controller;
        
        Effects     = new();
        Equipment   = new();
        Stats       = new(this);
        Weapons     = new(this);
        Movement    = new(this);

        Health      = MaxHealth;
        Mana        = MaxMana;
    }
}
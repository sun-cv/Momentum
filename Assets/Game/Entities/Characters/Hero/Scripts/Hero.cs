using UnityEngine;



public class Hero : Player, IHero
{
    // ===============================================================================
    //  Systems
    // ===============================================================================
    
    public IntentSystem         Intent              { get; private set; }
    public HeroState            State               { get; private set; }
    public ActorStats           Stats               { get; private set; }
    public Resources            Resource            { get; private set; }
    public EquipmentManager     Equipment           { get; private set; }
    public WeaponSystem         Weapons             { get; private set; }
    public Movement             Movement            { get; private set; }
    public EffectRegister       Effects             { get; private set; }
    public Presence             Presence            { get; private set; }
    public Lifecycle            Lifecycle           { get; private set; }
    public AnimationSystem      Animation           { get; private set; }

    // ===============================================================================
    //  Resource
    // ===============================================================================

    public float Health                             => Resource.Health;         
    public float Armor                              => Resource.Armor;          
    public float Shield                             => Resource.Shield;         
    public float Energy                             => Resource.Energy;           

    // ===============================================================================
    //  Stats
    // ===============================================================================

    public float MaxHealth                          => Stats.MaxHealth;         
    public float HealthRegen                        => Stats.HealthRegen;       
    
    public float MaxArmor                           => Stats.MaxArmor;          
    
    public float MaxShield                          => Stats.MaxShield;         
    public float ShieldRegen                        => Stats.ShieldRegen;       
    
    public float MaxEnergy                          => Stats.MaxEnergy;           
    public float EnergyRegen                        => Stats.EnergyRegen;         
    
    public float Strength                           => Stats.Strength;          
    public float StrengthMultiplier                 => Stats.StrengthMultiplier;
   
    public float Speed                              => Stats.Speed;             
    public float SpeedMultiplier                    => Stats.SpeedMultiplier;   

    public float Impact                             => Stats.Impact;

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

    public TimePredicate Parrying                   => State.Parrying;
    public bool Blocking                            => State.Blocking;

    public bool CanMove                             => State.CanMove;
    public bool CanRotate                           => State.CanRotate; 
    public bool CanAttack                           => State.CanAttack;

    public Direction Aim                            => State.Aim;
    public Direction Facing                         => State.Facing;
    public Direction Direction                      => State.Direction;
    public Direction LastDirection                  => State.LastDirection;

    public Direction ResolvedAim                    => State.ResolvedAim;
    public Direction ResolvedFacing                 => State.ResolvedFacing;
    public Direction ResolvedDirection              => State.ResolvedDirection;

    public float Mass                               => Definition.Physics.Mass;
    public float Friction                           => Definition.Physics.Friction;

    public Vector2 Momentum                         => State.Momentum;

    public Vector2 Velocity                         { get => State.Velocity;            set => State.Velocity       = value;}
    public Vector2 Control                          { get => State.Control;             set => State.Control        = value;}
    public Vector2 Force                            { get => State.Force;               set => State.Force          = value;}
    public Vector2 Normal                           { get => State.Normal;              set => State.Normal         = value;}

    public bool IsMoving                            => State.IsMoving;
    public TimePredicate IsIdle                     => State.IsIdle;

    public bool ShieldEquipped                      => State.ShieldEquipped;

    // ===============================================================================

    public void Initialize(ActorDefinition definition)
    {
        Definition  = definition;

        Intent      = new(this);
        Stats       = new(this);
        Resource    = new(this);
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

public class HeroCorpse : Corpse {}

using System;
using System.Diagnostics;
using UnityEngine;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IInitialize                    { public void Initialize(); }
public interface IBind                          { public void Bind();       } 

public interface IStateHandler<TController>
{    
    void Enter      (TController controller);
    void Update     (TController controller);
    void Exit       (TController controller);
}

public interface IStateProcessor<TController, TValue>
{
    void Enter      (TController controller);
    TValue Process  (TController controller);
    void Exit       (TController controller);
}

public interface IHandler                       { bool Handle();            }   // claims and stops. One handler in the chain responds, the rest are skipped.
public interface IProcessor<T>                  { T Process(T value);       }   // transforming. Takes a value, returns a modified value, passes it along.
public interface IResolver                      { void Resolve();           }   // terminal. Takes a request, produces a side effect, doesn't return a value
public interface IConsumer<T>                   { void Consume(T request);  }   // consumes what it can handle and leaves the remainder for the next

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public abstract class Definition
{ 
    public string Id                            { get; set; }
    public string Name                          { get; set; }
}

public class Payload                            { public Guid Id                        { get; init; } = Guid.NewGuid();}

public class Runtime                            { public Guid RuntimeId                 { get; init; } = Guid.NewGuid();}
public class Instance           : Runtime       {}

public class Zone               : Runtime       { public string Name                    { get; set;  }
                                                  public Anchor Anchor                  { get; set;  }
                                                  public Collider2D Area                { get; set;  }}
public class Portal             : Zone          { public string Location                { get; set;  }
                                                  public string Region                  { get; set;  }}
public class SpawnPoint         : Zone          { public Spawner Spawner                { get; set;  }}
public class AudioPoint         : Zone          {}
public class CameraPoint        : Zone          {}

public class Entity             : Runtime       {}
public class Actor              : Entity        { 
                                                  public Emit   Emit                    { get; set;  }
                                                  public Bridge Bridge                  { get; set;  }
                                                  public ActorDefinition Definition     { get; set;  }}

public class Agent              : Actor         {}

public class Player             : Agent         {}
public class Enemy              : Agent         {}
public class NPC                : Agent         {}

public class Prop               : Actor         {}
public class Explosion          : Prop {}
public class Trap               : Prop {}
public class Destructible       : Prop {}
public class Projectile         : Prop {}

public class Environmental      : Actor         {}
public class Hazard             : Environmental {}

public class Item               : Entity        {}

public class Equipment          : Item          { public EquipmentSlotType SlotType     { get; init; }}
public class Weapon             : Equipment     { public WeaponDefinition Definition    { get; init; }}
public class Armor              : Equipment     { public ArmorDefinition Definition     { get; init; }}


public class Controller         : MonoBehaviour {}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Maps                                                  
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public static class Layers
{
    public static readonly int Player       = LayerMask.NameToLayer("Player");
    public static readonly int Enemy        = LayerMask.NameToLayer("Enemy");
    public static readonly int NPC          = LayerMask.NameToLayer("NPC");
    public static readonly int Prop         = LayerMask.NameToLayer("Prop");
    public static readonly int Environment  = LayerMask.NameToLayer("Environment");
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                 Definition Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public interface IDepthSorted               {}
public interface IDepthColliding            {}

public interface IControllable
{
    bool Inactive                           { get; set; }
}

public interface IMortal : IHealth
{
    bool Alive                              { get; }
    bool Dead                               { get; }

    bool Invulnerable                       { get; set; }
    bool Impervious                         { get; set; }
}

public interface IHealth
{
    float Health                            { get; }
    float MaxHealth                         { get; }
}

public interface IHealthRegen
{
    float HealthRegen                       { get; }
}


public interface IArmor
{
    float Armor                             { get; }
    float MaxArmor                          { get; }
}

public interface IShield
{
    float Shield                            { get; }
    float MaxShield                         { get; }
}

public interface IShieldRegen
{
    float ShieldRegen                       { get; }
}

public interface ICaster            
{           
    float Energy                            { get; }
    float MaxEnergy                         { get; }
    float EnergyRegen                       { get; }
}           

public interface IEnergyRegen
{
    float EnergyRegen                       { get; }
}

public interface IMovable           
{           
    bool CanMove                            { get; }
    float Speed                             { get; }
    float SpeedMultiplier                   { get; }
    bool IsMoving                           { get; }
    bool Disabled                           { get; }
}           

public interface IAttacker          
{           
    bool CanAttack                          { get; }
}           

public interface IParryable
{
    TimePredicate Parrying                  { get; }
}

public interface IBlockable
{
    bool Blocking                           { get; }
}

public interface IDirectional           
{   
    Direction Direction                     { get; }
    Direction LastDirection                 { get; }
    Direction ResolvedDirection             { get; }
}           

public interface IOrientable            
{        
    Direction Facing                        { get; }
    Direction ResolvedFacing                { get; }
    bool CanRotate                          { get; }
}

public interface IAimable
{
    Direction Aim                           { get; }
    Direction ResolvedAim                   { get; }
}

public interface IDynamic
{
    Vector2 Momentum                        { get; }
    Vector2 Velocity                        { get; set; }
    Vector2 Control                         { get; set; }
    float Mass                              { get; }
    float Friction                          { get; }
    float Impact                            { get; }
}

public interface IPhysicsBody
{
    float   Friction                        { get; }
    Vector2 Normal                          { get; set; }
    Vector2 Force                           { get; set; }
    bool    Constrained                     { get; set; }
    bool    ImmuneToForce                   { get; }
}

public interface IAfflictable
{
    bool Disabled                           { get; }
    bool Stunned                            { get; set; }
    bool Invulnerable                       { get; set; }
    // Frozen, Poisoned, Slowed, Burning, etc.
}

public interface ICorpse
{
    Corpse.State Condition                  { get; }
}

public interface IIdle
{
    TimePredicate IsIdle                    { get; }
}


public interface IActor :
    IDepthSorted,
    IDepthColliding
{ }

public interface IAgent :
    IActor,
    IMortal,
    IControllable
{ }

public interface IMovableActor :
    IAgent,
    IMovable,
    IDynamic,
    IDirectional,
    IOrientable,
    IIdle
{ }

public interface IHero :
    // Core
    IMovableActor, IPhysicsBody,
    // Combat
    IAttacker, IParryable, IBlockable, IAimable,
    // Resources
    IHealthRegen, IShield, IShieldRegen, IArmor, ICaster, IEnergyRegen,
    // Status
    IAfflictable
{ }

public interface IEnemy :
    // Core
    IMovableActor, IPhysicsBody, IControllable,
    // Combat
    IAttacker,
    // Status
    IAfflictable
{ }

public interface IDummy :
    // Core
    IAgent,
    // Status
    IAfflictable
{ }

public interface IMovableDummy :
    // Core
    IMovableActor, IPhysicsBody,
    // Status
    IAfflictable
{ }


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum Route 
{
    Direct,
    Distributed,
    Broadcast, 
}


public enum Request
{
    Create,
    Destroy,
    Start,
    Stop,
    Enable,
    Disable,
    Set,
    Get,
    Lock,
    Unlock,
    Queue,
    Interrupt,
    Cancel,
    Consume,
    Clear,
    Trigger,
    Transition,
    Dispose,
    Equip,
    Play,
}

public enum Response
{
    Accepted,
    Declined,
    Success,
    Failure,
    Started,
    Completed,
    Pending,
    Canceled,
    Denied,
}

public enum Publish
{
    Created,
    Destroyed,

    Enabled,
    Disabled,

    Started,
    Ended,

    Triggered,
    Fired,

    Activated,
    Deactivated,

    Canceled,
    Released,

    Equipped,
    Unequipped,

    Changed,
    Transitioned,
}

public enum Status
{
    Idle,
    Enabling,
    Enabled,
    Cancelling,
    Cancelled,
    Interrupting,
    Interrupted,
    Disabling,
    Disabled,
}

public enum InputCondition
{
    None,
    PressedThisFrame,
    Pressed,
    Held,
    ReleasedThisFrame,
    ReleasedRecently,
}

public enum PlayerAction
{
    None,
    Interact,
    Action,
    Attack1,
    Attack2,
    Modifier,
    Dash,
}

public enum Capability
{
    None,
    Interact,
    Action,
    Attack1,
    Attack2,
    Modifier,
    Movement,
    Dash,
    Sprint,
    Rotation,
    ItemUse,
    MenuAccess,
}

public enum ThresholdTrigger
{
    OnEnter,
    OnExit,
    OnCross,
}
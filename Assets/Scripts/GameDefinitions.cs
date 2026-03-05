using System;
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

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public abstract class Definition
{ 
    public string ID                            { get; set; }
    public string Name                          { get; set; }
}


public class Runtime                            { public Guid RuntimeID                 { get; init; } = Guid.NewGuid();}
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


public interface IDepthSorted       {}
public interface IDepthColliding    {}

public interface IDefined
{
    ActorDefinition Definition             { get; }
}

public interface IControllable
{
    bool Inactive                           { get; set; }
}
public interface IDamageable            
{           
    bool Invulnerable                       { get; set; }
    bool Impervious                         { get; set; }
    float Health                            { get; set; }
    float MaxHealth                         { get; }
}           

public interface ICaster            
{           
    float Mana                              { get; set; }
    float MaxMana                           { get; }
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
    float Attack                            { get; }
    float AttackMultiplier                  { get; }
}           

public interface IDefender          
{               
    bool Parrying                           { get; }
    bool Blocking                           { get; }
}           

public interface IDirectional           
{   
    Direction Direction                     { get; }
    Direction LastDirection                 { get; }
    Direction LockedDirection               { get; }
}           

public interface IOrientable            
{        
    Direction Facing                        { get; }
    Direction LockedFacing                  { get; }
    bool CanRotate                          { get; }
}

public interface IAimable
{
    Direction Aim                           { get; }
    Direction LockedAim                     { get; }
}

public interface IDynamic
{
    Vector2 Momentum                        { get; }
    Vector2 Velocity                        { get; set; }
    Vector2 Control                         { get; set; }
    float Mass                              { get; }
    float Friction                          { get; }
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

public interface ILiving
{
    bool Alive                              { get; }
    bool Dead                               { get; }
}

public interface ICorpse : IDefined
{
    Corpse.State Condition                  { get; }
}

public interface IIdle
{
    
    TimePredicate IsIdle                    { get; }
}



public interface IActor         : IDepthSorted, IDepthColliding {}
public interface IAgent         : IActor, IControllable, ILiving, IDefined {}
public interface IMovableActor  : IAgent, IIdle, IMovable, IDynamic, IDirectional, IOrientable { }

public interface IHero          : IMovableActor, IPhysicsBody, IAttacker, ICaster, IDefender, IAimable, IDamageable, IAfflictable { }
public interface IEnemy         : IMovableActor, IPhysicsBody, IControllable, IAttacker, IDamageable, IAfflictable { }
public interface IDummy         : IAgent, IDamageable, IAfflictable {}
public interface IMovableDummy  : IMovableActor, IPhysicsBody, IDamageable, IAfflictable {}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

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
    Creating,
    Created,
    Destroying,
    Destroyed,

    Enabling,
    Enabled,
    Disabling,
    Disabled,

    Starting,
    Started,
    Ending,
    Ended,

    Triggering,
    Triggered,
    Firing,
    Fired,

    Activating,
    Activated,
    Deactivating,
    Deactivated,

    Canceling,
    Canceled,
    Releasing,
    Released,

    Equipped,
    Unequipped,

    Changing,
    Changed,
    Transitioning,
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

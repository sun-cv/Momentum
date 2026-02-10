using System;
using UnityEngine;

//
//  Interfaces & Base Abstracts
//

public interface IInitialize    { public void Initialize(); }
public interface IBind          { public void Bind(); } 

public interface IService                { };
public interface IServiceTick : IService { public void Tick(); UpdatePriority Priority { get; } };
public interface IServiceLoop : IService { public void Loop(); UpdatePriority Priority { get; } };
public interface IServiceStep : IService { public void Step(); UpdatePriority Priority { get; } };
public interface IServiceUtil : IService { public void Util(); UpdatePriority Priority { get; } };
public interface IServiceLate : IService { public void Late(); UpdatePriority Priority { get; } };


public abstract class Service : IService
{
    public Guid RuntimeID               = Guid.NewGuid();
}

[Service]
public abstract class RegisteredService : Service, IInitialize
{
    public abstract void Initialize();
}




//
//  Definition
//


public class Definition
{ 
    public string ID                    { get; set; }
    public string Name                  { get; set; }
}


//
//  Runtime
//

public class Runtime                            { public Guid RuntimeID                 { get; init; } = Guid.NewGuid();}
public class Instance           : Runtime       {}
public class Entity             : Runtime       {}
public class Actor              : Entity        { 
                                                  public Emit Emit                      { get; set;  }
                                                  public Bridge Bridge                  { get; set;  }}


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




//
//  Unity Lifecle/bridge
//
public class Controller : MonoBehaviour {}






//
//  Events 
//





//
//  Actor
//


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
    Vector2 Velocity                        { get; }
    float Mass                              { get; }
}

public interface IAfflictable
{
    bool Disabled                           { get; }
    bool Stunned                            { get; set; }
    bool Invulnerable                       { get; set; }
    // Frozen, Poisoned, Slowed, Burning, etc.
}

public interface IIdle
{
    TimePredicate IsIdle { get; }
}

public interface IEventTarget
{
    Emit Emit                               { get; }   
}

public interface IActor {}
public interface IAgent : IControllable {}
public interface IMovableActor : IAgent, IDepthSorted, IDepthColliding, IMovable, IDynamic, IDirectional, IOrientable, IControllable { }

public interface IHero  : IEventTarget, IMovableActor, IDefined, IControllable, IAttacker, ICaster, IDefender, IAimable, IDamageable, IAfflictable { }
public interface IEnemy : IMovableActor, IControllable, IAttacker, IDamageable, IAfflictable { }
public interface IBoss  : IEnemy {}
// public interface ITurret        : IAttacker, IDamageable, IOrientable                   {} 


public interface IDummy : IControllable, IDamageable, IDefined, IAfflictable, IDepthSorted, IDepthColliding {}
//
//  Items
//





//
// Effects
//





//
// Enums
//

public enum Request
{
    Create,
    Destroy,
    Start,
    Stop,
    Set,
    Get,
    Lock,
    Unlock,
    Queue,
    Interrupt,
    Cancel,
    Consume,
    Clear,
    Enable,
    Disable
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
    Changed,
    Canceled,
    Equipped,
    Unequipped,
    Released,
    PhaseChange,
    StateChange,
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
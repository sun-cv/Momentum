using System;
using UnityEngine;

//
//  Interfaces & Base Abstracts
//

public interface IInitialize { public void Initialize(); }

public interface IService                { };
public interface IServiceTick : IService { public void Tick(); UpdatePriority Priority { get; } };
public interface IServiceLoop : IService { public void Loop(); UpdatePriority Priority { get; } };
public interface IServiceStep : IService { public void Step(); UpdatePriority Priority { get; } };
public interface IServiceUtil : IService { public void Util(); UpdatePriority Priority { get; } };


public abstract class Service
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

public class Runtime                        {public Guid RuntimeID                  { get; set; } = Guid.NewGuid();}
public class Instance           : Runtime   {}
public class Entity             : Runtime   { public GameObject Instance            { get; set; }}
public class Enemy              : Entity    {}
public class Item               : Entity    {}

public abstract class Equipment : Item      { public EquipmentSlotType SlotType     { get; init; }}
public class Weapon             : Equipment { public WeaponDefinition Definition    { get; init; }}
public class Armor              : Equipment { public ArmorDefinition Definition     { get; init; }}




//
//  Unity Lifecle/bridge
//
public class Controller : MonoBehaviour {}






//
//  Events 
//

public class EventHandler {}



//
//  Entity
//



public interface IDamageable
{
    bool Invulnerable           { get; }
    float Health                { get; set; }
    float MaxHealth             { get; }
}

public interface ICaster
{
    float Mana                  { get; set; }
    float MaxMana               { get; }
}

public interface IAttacker
{
    bool CanAttack              { get; }
    float Attack                { get; }
    float AttackMultiplier      { get; }
}

public interface IMovable
{
    bool CanMove                { get; }
    float Speed                 { get; }
    float SpeedMultiplier       { get; }
}

public interface IHero : IMovable, IAttacker, IDamageable {}



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

public enum InputIntent
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
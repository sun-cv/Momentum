using System;
using UnityEditor;
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
//  Data Definitions
//


public class Data
{ 
    public string Name                  { get; set; }
}

//
//  Runtime Definitions
//

public class Runtime
{
    public Guid RuntimeID               = Guid.NewGuid();
}

public class Instance   : Runtime   {}
public class Entity     : Instance  {}
public class Item       : Entity    {}


//
//  MonoBeviours
//

public class Controller : MonoBehaviour {}


//
//  Events 
//

public class EventHandler {}



//
//  Entity
//

// Health

public interface IHealthSet : IHasHealth {}
public interface IHasHealth
{
    public float Health                 { get; set; }
    public float MaxHealth              { get; }
}



// Mana

public interface IManaSet : IHasMana, IHasManaRegen {}
public interface IHasMana
{
    public float Mana                   { get; set; }
    public float MaxMana                { get; }
}

public interface IHasManaRegen
{
    public float ManaRegen              { get; }
}



// Movement

public interface IMovementSet : IHasMovement, IHasSprint {}
public interface IHasMovement
{
    public float SpeedMultiplierCap     { get; }
}

public interface IHasSprint
{
    public float AutoSprintBuffer       { get; }
}



// Combat

public interface IDamageable : IHasHealth
{
    public bool Invulnerable            { get; }
};

public interface IAttacker {}



// Entities

public interface IHero : IHealthSet, IManaSet, IMovementSet, IDamageable, IAttacker {}





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

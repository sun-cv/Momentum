using System;
using UnityEngine;


//
//  MonoBeviours
//

public class Controller : MonoBehaviour {}



//
//  Data Definitions
//


public class Data       : ScriptableObject 
{ 
    public string Name { get; set; }
}

public class EntityData : Data {}
public class ItemData   : Data {}
public class WeaponData : Data {}
public class EffectData : Data {}




//
//  Runtime Definitions
//

public class RuntimeInstance
{
    public string Name;
    public int RuntimeID;
}

public class Entity     : RuntimeInstance {}
public class Item       : RuntimeInstance {}
public class Effect     : RuntimeInstance {}

public class Weapon     : Item {}



// Health

public interface IHealthSet : IHasHealth, IHasHealthRegen, IHasHealthReserves {}
public interface IHasHealth
{
    public float Health                 { get; set; }
    public float MaxHealth              { get;}
}

public interface IHasHealthRegen
{
    public float HealthRegen            { get; }
    public float BaseHealthRegen        { get; }
    public float HealthRegenCooldown    { get; }
}

public interface IHasHealthReserves
{
    public object HealthReserves        { get; }
}


// Mana

public interface IManaSet : IHasMana, IHasManaRegen, IHasManaReserves {}
public interface IHasMana
{
    public float Mana                   { get; set; }
    public float MaxMana                { get; }
    public float MaxManaMultiplier      { get; }
    public float MinManaTickRate        { get; }
}

public interface IHasManaRegen
{
    public float ManaRegen              { get; }
    public float BaseManaRegen          { get; }
    public float ManaRegenCooldown      { get; }
}

public interface IHasManaReserves
{
    public object ManaReserves          { get; }
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


// Effects

public interface IHasEffects
{
    public Effect[] Effects             { get;}
}



// Combat

public interface IDamageable : IHasHealth
{
    public bool Invulnerable            { get; }
};

public interface IAttacker : IHasWeapons {}
public interface IHasWeapons 
{
    public string DefaultMainHand       { get;}
    public string DefaultOffHand        { get;}
}



// Entities

public interface IHero : IHealthSet, IManaSet, IMovementSet, IDamageable, IAttacker {}


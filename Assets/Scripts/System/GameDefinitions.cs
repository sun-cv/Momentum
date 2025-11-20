using UnityEngine;


public class Controller : MonoBehaviour {}

public class Data       : ScriptableObject 
{ 
    public string Name { get; set; }
}

public class EntityData : Data {}
public class ItemData   : Data {}
public class WeaponData : Data {}
public class EffectData : Data {}

public class Runtime
{
    public string Name;
    public int RuntimeID;
}

public class Entity     : Runtime {}
public class Item       : Runtime {}
public class Effect     : Runtime {}

public class Weapon     : Item {}





public interface IHasAllHealth : IHasHealth, IHasHealthRegen, IHasHealthReserves {}
public interface IHasHealth
{
    public int Health               { get; set; }
    public int MaxHealth            { get; set; }
}

public interface IHasHealthRegen
{
    public int HealthRegen          { get; set; }
    public int BaseHealthRegen      { get; set; }
    public int HealthRegenCooldown  { get; set; }
}

public interface IHasHealthReserves
{
    public object Reserves          { get; set; }
}




public interface IHasAllMana : IHasMana, IHasManaRegen, IHasManaReserves {}
public interface IHasMana
{
    public int Mana                 { get; set; }
    public int MaxMana              { get; set; }
    public int MaxManaMultiplier    { get; set; }
    public int MinManaTickRate      { get; set; }
}

public interface IHasManaRegen
{
    public int ManaRegen            { get; set; }
    public int BaseManaRegen        { get; set; }
    public int ManaRegenCooldown    { get; set; }
}

public interface IHasManaReserves
{
    public object Reserves          { get; set; }
}



public interface IHasAllMovement : IHasMovement, IHasSprint {}
public interface IHasMovement
{
    public int SpeedMultiplierCap   { get; set; }
}

public interface IHasSprint
{
    public int AutoSprintBuffer     { get; set; }
}

public interface IHasEffects
{
    public Effect[] Effects         { get; set; }
}

public interface IDamageable : IHasHealth
{
    public bool Invulnerable        { get; set; }
};

public interface IHasWeapons
{
    public string DefaultMainHand   { get; set; }
    public string DefaultOffHand    { get; set; }

    public Weapon MainHand          { get; set; }
    public Weapon OffHand           { get; set; }
}

using UnityEngine;


[CreateAssetMenu(fileName = "HeroData", menuName = "Entity/HeroData")]
public class HeroData : EntityData
{
    public string Name              = "Hero";
    
    [Header("Weapons")]
    public string DefaultMainHand   = "Sword";
    public string DefaultOffHand    = "Shield";

    [Header("Damageable")]
    public int Health               = 0;
    public int MaxHealth            = 0;
    public int HealthRegen          = 0;
    public int BaseHealthRegen      = 0;
    public int HealthRegenCooldown  = 0;
    public int MaxHealthMultiplier  = 0;
    public int MinHealthTickRate    = 0;

    [Header("Resources")]
    public int Mana                 = 0;
    public int MaxMana              = 0;
    public int ManaRegen            = 0;
    public int BaseManaRegen        = 0;
    public int ManaRegenCooldown    = 0;
    public int MaxManaMultiplier    = 0;
    public int MinManaTickRate      = 0;

    public Effect[] Effects;

};


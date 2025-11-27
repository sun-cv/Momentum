using UnityEngine;


[CreateAssetMenu(fileName = "HeroData", menuName = "Entity/HeroData")]
public class HeroData : EntityData
{
    public new string name              = "Hero";
    
    [Header("Weapons")]
    public string DefaultMainHand       = "Sword";
    public string DefaultOffHand        = "Shield";

    [Header("Damageable")]
    public float Health                 = 0;
    public float MaxHealth              = 0;
    public float HealthRegen            = 0;
    public float BaseHealthRegen        = 0;
    public float HealthRegenCooldown    = 0;
    public float MaxHealthMultiplier    = 0;
    public float MinHealthTickRate      = 0;

    [Header("Resources")]
    public float Mana                   = 0;
    public float MaxMana                = 0;
    public float ManaRegen              = 0;
    public float BaseManaRegen          = 0;
    public float ManaRegenCooldown      = 0;
    public float MaxManaMultiplier      = 0;
    public float MinManaTickRate        = 0;

    [Header("Movement")]    
    public float SpeedMultiplierCap     = 0;
    public float AutoSprintBuffer       = 0;

    public Effect[] Effects;
    
};

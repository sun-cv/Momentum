using UnityEngine;


public class HeroController : Controller
{
    Hero hero;

    public Rigidbody2D          body;
    public CapsuleCollider2D    hitbox;

    public void Initialize(HeroData data)
    {
        hero = new(data);
        hero.Stats.Mediator.AddModifier(new BasicStatModifier("MaxHealth", (int)1, (a) => a + 10 ));
    }

    public Hero Hero => hero;
}


public class Hero : Entity, IHero
{
    HeroStats stats;

    public string DefaultMainHand       { get; set; }
    public string DefaultOffHand        { get; set; }

    public bool Invulnerable            { get; set; }

    public float Health                 { get; set; }
    public float MaxHealth              => stats.MaxHealth;
    public float HealthRegen            => stats.HealthRegen;
    public float BaseHealthRegen        => stats.BaseHealthRegen;
    public float HealthRegenCooldown    => stats.HealthRegenCooldown;
    public float MaxHealthMultiplier    => stats.MaxHealthMultiplier;
    public float MinHealthTickRate      { get; }
    public object HealthReserves        { get; set; }   

    public float Mana                   { get; set; }
    public float MaxMana                => stats.MaxMana;
    public float ManaRegen              => stats.ManaRegen;
    public float BaseManaRegen          => stats.BaseManaRegen;
    public float ManaRegenCooldown      => stats.ManaRegenCooldown;
    public float MaxManaMultiplier      => stats.MaxManaMultiplier;
    public float MinManaTickRate        { get; }
    public object ManaReserves          { get; set; }

    public float SpeedMultiplierCap     { get; set; }
    public float AutoSprintBuffer       { get; set; }

    public Hero(HeroData definition)
    {
        DataMapper.Map(definition, this);
        stats = new(definition);
    }

    public Stats Stats => stats;
}

public static class HeroFactory
{
    public static HeroController Create()
    {
        var data = Registry.Data.Get<HeroData>("HeroData");
        if (data == null)
        {
            Debug.Log("Failed HeroData load");
            return null;
        }

        var prefab = Registry.Prefab.Get<GameObject>("Hero");
        if (data == null)
        {
            Debug.Log("Failed HeroPrefab load");
            return null;
        }

        var obj = GameObject.Instantiate(prefab);

        var controller = obj.GetComponent<HeroController>();
        controller.Initialize(data);

        return controller;
    }
}

public class HeroStats : Stats
{
    public HeroStats(HeroData hero)
    {
        stats.Add("MaxHealth"          , hero.MaxHealth);
        stats.Add("HealthRegen"        , hero.HealthRegen);
        stats.Add("BaseHealthRegen"    , hero.BaseHealthRegen);
        stats.Add("HealthRegenCooldown", hero.HealthRegenCooldown);


        stats.Add("MaxMana"            , hero.MaxMana);
        stats.Add("ManaRegen"          , hero.ManaRegen);
        stats.Add("BaseManaRegen"      , hero.BaseManaRegen);
        stats.Add("ManaRegenCooldown"  , hero.ManaRegenCooldown);

    }

    public float MaxHealth              => this["MaxHealth"];
    public float HealthRegen            => this["HealthRegen"];
    public float BaseHealthRegen        => this["BaseHealthRegen"];
    public float HealthRegenCooldown    => this["HealthRegenCooldown"];
    public float MaxHealthMultiplier    => this["MaxHealthMultiplier"];

    public float MaxMana                => this["MaxMana"];
    public float ManaRegen              => this["ManaRegen"];
    public float BaseManaRegen          => this["BaseManaRegen"];
    public float ManaRegenCooldown      => this["ManaRegenCooldown"];
    public float MaxManaMultiplier      => this["MaxManaMultiplier"];
}
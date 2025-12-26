using UnityEngine;


public class HeroController : Controller
{
    readonly Hero        hero           = new();
    readonly Context     context        = new();

    public Rigidbody2D          body;
    public CapsuleCollider2D    hitbox;

    public void Initialize(HeroData data)
    {
        hero    .Initialize(data);
        context .Initialize();

        Services.Get<WeaponSystem>().AssignContext(context);
        Services.Get<WeaponSystem>().AssignWeaponSet(context.weaponSet);

        Services.Get<MovementEngine>().AssignHero(this);

    }

    public Hero Hero        => hero;
    public Context Context  => context;
}


public class Hero : Entity
{
    HeroStats stats;

    public float Health                 { get; set; }
    public float MaxHealth              => stats.MaxHealth;

    public float Mana                   { get; set; }
    public float MaxMana                => stats.MaxMana;

    public float Speed                  { get; } = 5;
    public float Damage                 { get; }

    public void Initialize(HeroData definition)
    {
        DataMapper.Map(definition, this);
        stats = new(definition);
        stats.Initialize();
    }

    public Stats Stats => stats;
}

public class HeroStats : Stats
{
    public HeroStats(HeroData hero)
    {
        stats.Add("MaxHealth"          , hero.MaxHealth);
        stats.Add("MaxMana"            , hero.MaxMana);
    }

    public float MaxHealth              => this["MaxHealth"];
    public float MaxMana                => this["MaxMana"];
}


public static class HeroFactory
{
    public static HeroController Create()
    {
        var data = new HeroData();

        var prefab = Registry.Prefabs.Get<GameObject>("Hero");
        if (data == null)
        {
            Debug.Log("Failed to load HeroPrefab");
            return null;
        }

        var obj = GameObject.Instantiate(prefab);

        var controller = obj.GetComponent<HeroController>();
        controller.Initialize(data);

        return controller;
    }
}

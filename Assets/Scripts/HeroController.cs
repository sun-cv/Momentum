using UnityEngine;



public class HeroController : Controller
{
    public Rigidbody2D          body;
    public CapsuleCollider2D    hitbox;
    public Animator             animator;
    public SpriteRenderer       renderer;
}


public class Hero
{
    HeroStats       stats;
    Context         context;
    HeroController  character;

    public float Health         { get; set; }
    public float MaxHealth      => stats.MaxHealth;
    public float Mana           { get; set; }
    public float MaxMana        => stats.MaxMana;
    public float Speed          => stats.Speed;

    public void Initialize(HeroController controller, HeroData data)
    {
        character   = controller;

        stats       = new();
        context     = new();

        stats   .Initialize(data);
        context .Initialize();

        Services.Get<CameraRig>()       .AssignHero(this);
        Services.Get<WeaponSystem>()    .AssignHero(this);
        Services.Get<MovementEngine>()  .AssignHero(this);

        Health  = MaxHealth;
        Mana    = MaxMana;
    }
    
    public Context Context          => context;
    public HeroController Character => character;
}


public class HeroStats : Stats
{
    public void Initialize(HeroData data)
    {
        stats.Add(nameof(MaxHealth), data.MaxHealth);
        stats.Add(nameof(MaxMana)  , data.MaxMana);
        stats.Add(nameof(Speed)    , data.Speed);
    }

    public float MaxHealth  => this[nameof(MaxHealth)];
    public float MaxMana    => this[nameof(MaxMana)];
    public float Speed      => this[nameof(Speed)];
}


public static class HeroFactory
{
    public static Hero Create()
    {
        var prefab      = Registry.Prefabs.Get<GameObject>("HeroController");
        var instance    = Object.Instantiate(prefab);
        var controller  = instance.GetComponent<HeroController>();

        var hero        = new Hero();
        var data        = new HeroData();
    
        hero.Initialize(controller, data);

        return hero;
    }
}

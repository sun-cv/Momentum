using UnityEngine;


public class HeroController : Controller
{
    Hero hero;

    public Rigidbody2D          body;
    public CapsuleCollider2D    hitbox;
    public Animator             animator;

    public void Initialize(HeroData data)
    {
        hero = new(data);
    }

    public Hero Hero => hero;
}

public class Hero : Entity, IHasHealth, IHasMana
{
    public string DefaultMainHand;
    public string DefaultOffHand;

    public int Health               { get; set; }
    public int MaxHealth            { get; set; }
    public int HealthRegen          { get; set; }
    public int BaseHealthRegen      { get; set; }
    public int HealthRegenCooldown  { get; set; }
    public int MaxHealthMultiplier  { get; set; }
    public int MinHealthTickRate    { get; set; }   

    public int Mana                 { get; set; }
    public int MaxMana              { get; set; }
    public int ManaRegen            { get; set; }
    public int BaseManaRegen        { get; set; }
    public int ManaRegenCooldown    { get; set; }
    public int MaxManaMultiplier    { get; set; }
    public int MinManaTickRate      { get; set; }

    public Hero(HeroData definition)
    {
        DataMapper.Map(definition, this);
    }
}

public static class HeroFactory
{
    public static HeroController Create()
    {
        var data = GameRegistry.GetEntity("Hero") as HeroData;
        if (data == null) return null;

        var prefab = GameRegistry.GetPrefab("Hero");
        if (prefab == null) return null;

        var obj = GameObject.Instantiate(prefab);

        var controller = obj.GetComponent<HeroController>();
        controller.Initialize(data);

        return controller;
    }
}


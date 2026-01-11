using UnityEngine;





public class HeroStats : Stats
{
    Hero owner;
    
    public HeroStats(Hero hero)
    {
        owner = hero;
        
        stats.Add(nameof(MaxHealth)         , hero.Definition.MaxHealth);
        stats.Add(nameof(MaxMana)           , hero.Definition.MaxMana);
        stats.Add(nameof(Speed)             , hero.Definition.Speed);
        stats.Add(nameof(SpeedMultiplier)   , hero.Definition.Speed);
        stats.Add(nameof(Attack)            , hero.Definition.Attack);
        stats.Add(nameof(AttackMultiplier)  , hero.Definition.Attack);
    }

    float health; 
    float mana;

    public float MaxHealth          => this[nameof(MaxHealth)]; 
    public float Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0, MaxHealth);
    }
    public float MaxMana            => this[nameof(MaxMana)];
    public float Mana
    {
        get => mana;
        set => mana = Mathf.Clamp(value, 0, MaxHealth);
    }
    public float Speed              => this[nameof(Speed)];
    public float SpeedMultiplier    => this[nameof(SpeedMultiplier)];
    public float Attack             => this[nameof(Attack)];
    public float AttackMultiplier   => this[nameof(AttackMultiplier)];
}


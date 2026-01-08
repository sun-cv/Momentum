using UnityEngine;





public class HeroStats : Stats
{
    public HeroStats(Hero hero)
    {
        stats.Add(nameof(MaxHealth), hero.Definition.MaxHealth);
        stats.Add(nameof(MaxMana)  , hero.Definition.MaxMana);
        stats.Add(nameof(Speed)    , hero.Definition.Speed);
    }

    float health; 
    float mana;

    public float MaxHealth  => this[nameof(MaxHealth)]; 
    public float Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0, MaxHealth);
    }
    public float MaxMana    => this[nameof(MaxMana)];
    public float Mana
    {
        get => mana;
        set => mana = Mathf.Clamp(value, 0, MaxHealth);
    }
    public float Speed      => this[nameof(Speed)];
}


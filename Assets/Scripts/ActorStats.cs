using System.Linq;
using System.Reflection;
using UnityEngine;





public class ActorStats : Stats
{
    Actor owner;
    
    public ActorStats(Actor actor)
    {
        if (actor is not IDefined instance)
            return;

        owner = actor;
        
        foreach (var stat in StatProperties)
        {
            var value = (float)stat.GetValue(instance.Definition.Stats);

            if (value < 0)
                continue;

            stats.Add(stat.Name, value);
        }
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

    static readonly PropertyInfo[] StatProperties = typeof(StatsDefinition).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.PropertyType == typeof(float)).ToArray();
}


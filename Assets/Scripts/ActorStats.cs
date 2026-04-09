using System.Linq;
using System.Reflection;



public class ActorStats : Stats
{
    // ===============================================================================

    public ActorStats(Actor actor) : base(actor)
    {
        foreach (var stat in StatProperties)
        {
            var value = (float)stat.GetValue(actor.Definition.Stats);

            if (value < 0)
                continue;

            stats.Add(stat.Name, value);
        }

        Enable();
    }

    // ===============================================================================
    // Accessors
    // ===============================================================================

    public float MaxHealth          => this[nameof(MaxHealth)];
    public float HealthRegen        => this[nameof(HealthRegen)];

    public float MaxArmor           => this[nameof(MaxArmor)];

    public float MaxShield          => this[nameof(MaxShield)];
    public float ShieldRegen        => this[nameof(ShieldRegen)];

    public float MaxEnergy          => this[nameof(MaxEnergy)];
    public float EnergyRegen        => this[nameof(EnergyRegen)];

    public float Strength           => this[nameof(Strength)];
    public float StrengthMultiplier => this[nameof(StrengthMultiplier)];
    
    public float Speed              => this[nameof(Speed)];
    public float SpeedMultiplier    => this[nameof(SpeedMultiplier)];

    public float Impact             => this[nameof(Impact)];

    // ===============================================================================

    // readonly Logger Log = Logging.For(LogSystem.Stats);

    static readonly PropertyInfo[] StatProperties =
        typeof(StatsDefinition)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(parameter => parameter.IsDefined(typeof(StatAttribute)))
            .ToArray();
}

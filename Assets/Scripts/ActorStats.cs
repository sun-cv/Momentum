using System.Linq;
using System.Reflection;



public class ActorStats : Stats
{
    readonly Actor owner;
    
    // ===============================================================================

    public ActorStats(Actor actor)
    {
        owner = actor;
        
        foreach (var stat in StatProperties)
        {
            var value = (float)stat.GetValue(actor.Definition.Stats);

            if (value < 0)
                continue;

            stats.Add(stat.Name, value);
        }

        owner.Emit.Link.LocalBinding<PresenceStateEvent>(HandlePresenceStateEvent);
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
    //  Events
    // ===============================================================================

    void HandlePresenceStateEvent(PresenceStateEvent message)
    {
        switch (message.State)
        {
            case Presence.State.Entering: Enable();  break;
            case Presence.State.Exiting:  Disable(); break;
            case Presence.State.Disposal: Dispose(); break;
        }
    }

    // ===============================================================================

    readonly Logger Log = Logging.For(LogSystem.Stats);

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    static readonly PropertyInfo[] StatProperties = typeof(StatsDefinition).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop => prop.PropertyType == typeof(float)).ToArray();
}

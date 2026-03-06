using System.Linq;
using System.Reflection;
using UnityEngine;



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

        owner.Emit.Link.LocalBinding<Message<Publish, PresenceStateEvent>>(HandlePresenceStateEvent);
    }

    // ===============================================================================
    // Accessors
    // ===============================================================================

    public float MaxHealth          => this[nameof(MaxHealth)];
    public float MaxArmor           => this[nameof(MaxArmor)];
    public float MaxShield          => this[nameof(MaxShield)];
    public float MaxMana            => this[nameof(MaxMana)];
    public float Strength           => this[nameof(Strength)];
    public float StrengthMultiplier => this[nameof(StrengthMultiplier)];
    public float Speed              => this[nameof(Speed)];
    public float SpeedMultiplier    => this[nameof(SpeedMultiplier)];


    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandlePresenceStateEvent(Message<Publish, PresenceStateEvent> message)
    {
        switch (message.Payload.State)
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
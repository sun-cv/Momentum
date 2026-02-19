using System.Linq;
using System.Reflection;
using UnityEngine;



public class ActorStats : Stats
{
    Actor owner;
    
        // -----------------------------------

    float health; 
    float mana;

    // ===============================================================================

    public ActorStats(Actor actor)
    {
        if (actor is not IDefined instance)
        {
        Log.Error($"Cannot initialize ActorStats for {actor.GetType().Name}. Actor does not implement IDefined interface");
            return;
        }

        owner = actor;
        
        foreach (var stat in StatProperties)
        {
            var value = (float)stat.GetValue(instance.Definition.Stats);

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
    public float Mass               => this[nameof(Mass)];

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandlePresenceStateEvent(Message<Publish, PresenceStateEvent> message)
    {
        switch(message.Payload.State)
        {
            case Presence.State.Entering:
                Enable();
            break;
            case Presence.State.Exiting:
                Disable();
            break;
            case Presence.State.Disposal:
                Dispose();
            break;
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
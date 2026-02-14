using System.Collections.Generic;





public class ActorDefinition : Definition
{
    public StatsDefinition Stats                        { get; init; }
    public PresenceDefinition Presence                  { get; init; }
    public LifecycleDefinition Lifecycle                { get; init; }
    public AnimationDefinition Animations               { get; init; }

};  




public class StatsDefinition : Definition
{   
    public float Health                                 { get; init; }
    public float MaxHealth                              { get; init; }

    public float Mana                                   { get; init; }
    public float MaxMana                                { get; init; }

    public float Attack                                 { get; init; }

    public float Speed                                  { get; init; }    
    public float Mass                                   { get; init; }
}

public class PresenceDefinition : Definition
{
    public bool EnableAbsentState                       { get; init; }

    public bool CanBeCameraTarget                       { get; init; }
}

public class LifecycleDefinition : Definition
{
    public bool EnableHealthThresholds                  { get; init; } = false;
    public bool AlertOnHealthChange                     { get; init; } = false;
    public bool AlertOnDeath                            { get; init; } = false;

    public CorpseBehavior Corpse                        { get; init; }
    public RespawnBehavior Respawn                      { get; init; }
    
    public List<HealthThreshold> HealthThresholds       { get; init; }
    public List<Effect> OnDeathEffects                  { get; init; }
}

public class SpawnBehavior : Definition
{
    
}

public class RespawnBehavior : Definition
{
    public bool Enabled                                 { get; init; }
    public float RespawnDelay                           { get; init; }

    public bool RestoreFullHealth                       { get; init; }
}

public class AnimationDefinition : Definition
{
    public AnimationSet Spawn                           { get; init; }
    public AnimationSet Death                           { get; init; }
}


public class AnimationSet : Definition
{
    public bool     Enabled                             { get; init; }
    public string   Default                             { get; init; }
    public string[] Random                              { get; init; }
    
    // public Dictionary<DamageType, string> ByDamageType  { get; init; }
}

public class CorpseBehavior : Definition
{
    public bool Persists                                { get; init; } = false;
    public float PersistDuration                        { get; init; } = -1;
}
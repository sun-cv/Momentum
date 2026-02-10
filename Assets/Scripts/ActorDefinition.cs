using System.Collections.Generic;





public class ActorDefinition : Definition
{
    public StatsDefinition Stats                        { get; init; }
    public LifecycleDefinition Lifecycle                { get; init; }
};  




public class StatsDefinition : Definition
{   
    public float Health                                 { get; init; } = -1;
    public float MaxHealth                              { get; init; } = -1;

    public float Mana                                   { get; init; } = -1;
    public float MaxMana                                { get; init; } = -1;

    public float Speed                                  { get; init; } = -1;

    public float Attack                                 { get; init; } = -1;

    public float Mass                                   { get; init; } = -1;
}

public class LifecycleDefinition : Definition
{
    public bool EnableDeathDetection                    { get; init; } = false;
    public bool EnableHealthThresholds                  { get; init; } = false;
    public bool AlertOnHealthChange                     { get; init; } = false;
    public bool AlertOnDeath                            { get; init; } = false;

    public List<HealthThreshold> HealthThresholds       { get; init; } = new();
    public DeathAnimationSet DeathAnimations            { get; init; }
    public List<Effect> OnDeathEffects                  { get; init; } = new();

    public RespawnBehavior RespawnBehavior              { get; init; }
}


public class RespawnBehavior
{
    public bool Enabled                                 { get; init; }
    public float RespawnDelay                           { get; init; }

    public bool RestoreFullHealth                       { get; init; }
}

public class DeathAnimationSet : Definition
{
    public string   Default                             { get; init; }
    public string[] Random                              { get; init; }
    
    // public Dictionary<DamageType, string> ByDamageType  { get; init; }
}
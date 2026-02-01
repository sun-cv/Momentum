

using System.Collections.Generic;

public class ActorDefinition : Definition
{

    public StatsDefinition Stats            { get; init; }
    public LifecycleDefinition Lifecycle    { get; init; }
};




public class StatsDefinition : Definition
{
    public float Health                     { get; init; } = -1;
    public float MaxHealth                  { get; init; } = -1;

    public float Mana                       { get; init; } = -1;
    public float MaxMana                    { get; init; } = -1;

    public float Speed                      { get; init; } = -1;

    public float Attack                     { get; init; } = -1;
}

public class LifecycleDefinition : Definition
{
    
}



public class DeathAnimationSet : Definition
{
    public string   Default                             { get; init; }
    public string[] Random                              { get; init; }
    
    public Dictionary<DamageType, string> ByDamageType  { get; init; }
}
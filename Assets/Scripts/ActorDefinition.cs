using System;
using System.Collections.Generic;
using System.Reflection;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                          Actor
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public abstract class ActorDefinition : Definition
{
    public StatsDefinition Stats                        { get; init; } = new();
    public PhysicsDefinition Physics                    { get; init; } = new();
    public LifecycleDefinition Lifecycle                { get; init; } = new();
    public AppearanceDefinition Appearance              { get; init; } = new();
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                          Stats
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class StatsDefinition
{
    public ResourceConfig Health                        { get; init; } = new();
    [Stat] public float MaxHealth                       { get; init; }
    [Stat] public float HealthRegen                     { get; init; }

    public ResourceConfig Armor                         { get; init; } = new();
    [Stat] public float MaxArmor                        { get; init; }

    public ResourceConfig Shield                        { get; init; } = new();
    [Stat] public float MaxShield                       { get; init; }
    [Stat] public float ShieldRegen                     { get; init; }

    public ResourceConfig Energy                        { get; init; } = new();
    [Stat] public float MaxEnergy                       { get; init; }
    [Stat] public float EnergyRegen                     { get; init; }

    public ResourceConfig Integrity                     { get; init; } = new();
    [Stat] public float MaxIntegrity                    { get; init; }

    [Stat] public float Strength                        { get; init; }
    [Stat] public float Speed                           { get; init; }
    [Stat] public float Impact                          { get; init; }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                              Resource Config
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ResourceConfig
{
    public bool AlertOnChange                           { get; init; }
    public List<ResourceThreshold> Thresholds           { get; init; } = new();
}

public class ResourceThreshold
{
    public string EventName                             { get; init; }
    public float Percentage                             { get; init; }
    public ThresholdTrigger Trigger                     { get; init; }
    public List<Effect> Effects                         { get; init; } = new();
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Physics
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class PhysicsDefinition
{
    /// <summary> Mass of the entity. Affects momentum transfer ratios and solver-based collision response. </summary>
    public float Mass                                   { get; init; }
    /// <summary> Reduces incoming transfer force from Actor contacts. 0 = full force received, 1 = immovable. </summary>
    public float PushResistance                         { get; init; }
    /// <summary> Minimum impact magnitude required before this entity responds to Actor contact forces. </summary>
    public float MomentumThreshold                      { get; init; }
    /// <summary> Minimum impact magnitude required before bleed force is applied back onto this entity. Prevents bounce on casual contact. </summary>
    public float BleedThreshold                         { get; init; }
    /// <summary> Fraction of non-transferred force that bleeds back onto this entity on impact. 0 = no bleed, 1 = full bleed. </summary>
    public float BleedRatio                             { get; init; }
    public float Friction                               { get; init; } = Settings.Physics.FRICTION;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Lifecycle
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class LifecycleDefinition
{
    public SpawnConfig Spawn                            { get; init; } = new();
    public RespawnConfig Respawn                        { get; init; } = new();
    public CorpseConfig Corpse                          { get; init; } = new();

    public List<Effect> OnDeathEffects                  { get; init; } = new();
    public bool AlertOnDeath                            { get; init; }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Spawn
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class SpawnConfig
{
    public string Corpse                                { get; init; }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Respawn
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class RespawnConfig
{
    public bool Enabled                                 { get; init; }
    public float RespawnDelay                           { get; init; }
    public bool RestoreFullHealth                       { get; init; }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Corpse
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class CorpseConfig
{
    public int MaxOccupancy                             { get; init; }
    public float FreshDuration                          { get; init; }
    public float DecayDuration                          { get; init; }
    public float ConsumeDuration                        { get; init; }
    public float RemainsDuration                        { get; init; }
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                       Appearance
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AppearanceDefinition
{
    public SortTier             DepthSortingTier        { get; init; }
    public bool                 CanBeCameraTarget       { get; init; }

    public AnimationDefinition  Animations              { get; init; } = new();
}



        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Animation
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class AnimationDefinition
{
    public AnimationSet         Spawn                   { get; init; } = new();
    public AnimationSet         Death                   { get; init; } = new();
}

public class AnimationSet
{
    public bool Enabled                                 { get; init; }
    public string Default                               { get; init; }
    public string[] Random                              { get; init; }

    public List<string> ByName                          { get; init; } = new();
    public Dictionary<string, string> ByDamage          { get; init; } = new();
    public Dictionary<string, string> ByLocation        { get; init; } = new();
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        ActorStats
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//
// public class ActorStats : Stats
// {
//     public ActorStats(Actor actor) : base(actor)
//     {
//         foreach (var stat in StatProperties)
//         {
//             var value = (float)stat.GetValue(actor.Definition.Stats);
//             if (value < 0)
//                 continue;
//             stats.Add(stat.Name, value);
//         }
//         Enable();
//     }
//
//     // ── Accessors ────────────────────────────────────────────────────────────────────────
//     public float MaxHealth              => this[nameof(MaxHealth)];
//     public float HealthRegen            => this[nameof(HealthRegen)];
//     public float MaxArmor               => this[nameof(MaxArmor)];
//     public float MaxShield              => this[nameof(MaxShield)];
//     public float ShieldRegen            => this[nameof(ShieldRegen)];
//     public float MaxEnergy              => this[nameof(MaxEnergy)];
//     public float EnergyRegen            => this[nameof(EnergyRegen)];
//     public float MaxIntegrity           => this[nameof(MaxIntegrity)];
//     public float Strength               => this[nameof(Strength)];
//     public float StrengthMultiplier     => this[nameof(StrengthMultiplier)];
//     public float Speed                  => this[nameof(Speed)];
//     public float SpeedMultiplier        => this[nameof(SpeedMultiplier)];
//     public float Impact                 => this[nameof(Impact)];
//
//     // ── Reflection scan ──────────────────────────────────────────────────────────────────
//     static readonly PropertyInfo[] StatProperties =
//         typeof(StatsDefinition)
//             .GetProperties(BindingFlags.Instance | BindingFlags.Public)
//             .Where(p => p.IsDefined(typeof(StatAttribute)))
//             .ToArray();
// }

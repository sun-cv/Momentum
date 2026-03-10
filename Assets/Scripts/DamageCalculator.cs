using System.Collections.Generic;



public class DamageSystem
{
    
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct Damage : IResourceAction
{
    public float Amount                     { get; init; }
    public DamageType Type                  { get; init; }

    public Damage(float amount, DamageType type)
    {
        Amount  = amount;
        Type    = type;
    }
}

public readonly struct DamageConfig
{
    public bool BypassArmor                 { get; init; }
    public bool BypassShield                { get; init; }
}

public readonly struct DamagePackageConfig
{
    public ParryConfig Parry                { get; init; }
    public BlockConfig Block                { get; init; }
}

public readonly struct ParryConfig
{
    public bool Enabled                     { get; init; }
    public float ParryWindow                { get; init; }
    public float PerfectParryWindow         { get; init; }

    public float ParryReward                { get; init; }
    public float PerfectParryReward         { get; init; }

    public float StaggerDuration            { get; init; }
}

public readonly struct BlockConfig
{
    public bool Enabled                     { get; init; }
}

public readonly struct DamageComponent
{
    public Damage Damage                    { get; init; }
    public DamageConfig Config              { get; init; }
    public List<Effect> Effects             { get; init; }

    public DamageComponent(Damage damage, DamageConfig config = default)
    {
        Damage      = damage;
        Config      = config;
        Effects     = new();
    }
}

public readonly struct DamagePackage
{
    public List<DamageComponent> Components { get; init; }
    public DamagePackageConfig Config       { get; init; }

    public DamagePackage(List<DamageComponent> components, DamagePackageConfig config = default)
    {
        Components = components;
        Config     = config;
    }
}

public class DamageContext
{
    public Actor Target                     { get; init; }
    public Actor Source                     { get; init; }
    public DamagePackage Package            { get; init; }
    public DamageResult Result              { get; init; }

    public DamageContext(Actor target, Actor source, DamagePackage package)
    {
        Target      = target;
        Source      = source;
        Package     = package;
        Result      = new();
    }
 }

public class DamageResult
{
    public float TotalDamage                { get; set; }
    public float ShieldDamage               { get; set; }
    public float ArmorDamage                { get; set; }
    public float HealthDamage               { get; set; }
    public bool ShieldBroken                { get; set; }
    public bool Parried                     { get; set; }
    public bool Blocked                     { get; set; }
    public bool Killed                      { get; set; }
    public List<Effect> AppliedEffects      { get; set; } = new();
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum DamageType
{
    Fire,
    Frost,
    Shock,
    Poison,
    Physical,
    Dynamic,
    Explosion,
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public readonly struct DamageEvent : IMessage
{
    public DamageContext Context            { get; init; }

    public DamageEvent(DamageContext context)
    {
        Context = context; 
    }
}



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Processor
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public interface IMitigationProcessor
{
    float Process(DamageContext context);
}


public class ArmorMitigation : IMitigationProcessor
{
    public float Process(DamageContext context)
    {
        return 0f;
    }
}

public class ResistanceMitigation : IMitigationProcessor
{
    public float Process(DamageContext context)
    {
        return 0f;
    }
}

public class KillingBlow {}
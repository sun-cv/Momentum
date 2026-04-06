using System.Collections.Generic;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 classes                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct Damage : IResourceAction
{
    public float Amount                         { get; init; }
    public DamageElement Element                { get; init; }

    public Damage(float amount, DamageElement element)
    {
        Amount          = amount;
        Element         = element;
    }
}

public readonly struct DamageComponent
{
    public Damage Damage                        { get; init; }
    public DamageMode Mode                      { get; init; }
    public List<StatusComponent> StatusEffects  { get; init; }

    public DamageComponent(Damage damage, DamageMode mode)
    {
        Damage          = damage;
        Mode            = mode;
        StatusEffects   = new();
    }
}

public readonly struct DamagePackage
{
    public List<DamageComponent> Components     { get; init; } 
    public DamagePackageConfig Config           { get; init; }
    public DamageResult Result                  { get; init; }

    public DamagePackage(List<DamageComponent> components, DamagePackageConfig config = default)
    {
        Components      = components;
        Config          = config;
        Result          = new(components);
    }
}

public class DamageContext
{
    public Actor Source                         { get; init; }
    public Actor Target                         { get; init; }
    public DamagePackage Package                { get; init; }

    public DamageContext(Actor source, Actor target, DamagePackage package)
    {
        Source          = source;
        Target          = target;
        Package         = package;
    }
 }

public class DamageResult
{
    public bool Parried                         { get; set; }
    public bool Blocked                         { get; set; }

    public Dictionary<DamageComponent, ComponentResult> Components  { get; set; } = new();

    public DamageResult(List<DamageComponent> components)
    {
        foreach (var component in components)
        {
            Components.Add(component, new() { Damage = component.Damage.Amount });
        }
    }
}

public class ComponentResult
{
    public float Damage                         { get; set; }
    public float Shield                         { get; set; }
    public float Armor                          { get; set; }
    public float Health                         { get; set; }
    public bool BrokeShield                     { get; set; }
    public bool BrokeArmor                      { get; set; }
    public bool BrokeHealth                     { get; set; }
}


        // ===================================
        //  Configs
        // ===================================


public readonly struct DamagePackageConfig
{
    public ParryConfig Parry                    { get; init; }
    public BlockConfig Block                    { get; init; }
}

public readonly struct ParryConfig
{
    public bool  Enabled                        { get; init; }
    public float ParryWindow                    { get; init; }
    public float PerfectParryWindow             { get; init; }

    public float ParryReward                    { get; init; }
    public float PerfectParryReward             { get; init; }

    public float StaggerDuration                { get; init; }
}

public readonly struct BlockConfig
{
    public bool  Enabled                        { get; init; }
    public bool  Unblockable                    { get; init; }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct KillingBlow : IMessage  
{
    public DamageContext Context                { get; init; }
    public DamageComponent Component            { get; init; }

    public KillingBlow(DamageContext context, DamageComponent component)
    {
        Context     = context;
        Component   = component;
    }
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum DamageElement
{
    Fire,       // Burns armor and health on dot, break effect DOT
    Frost,      // slows, break effect: longer recharge on shield.
    Shock,      // Increased damage to shield, break effect stun
    Physical,   // regular damage
    Explosion,  // break effect: knocback
}

public enum DamageMode
{
    DoT,
    Laser,
    Direct,
}


using System;
using System.Collections.Generic;
using UnityEngine;



public class Resources
{
    readonly Actor owner;

        // -----------------------------------

    readonly List<IResourceConsumer> distributed;
    readonly Dictionary<Type, IResourceConsumer> consumers;

        // -----------------------------------

    readonly List<ResourcePackage> queue;

    // ===============================================================================
    
    public Resources(Actor actor)
    {
        owner = actor;

        Link.Global<Message<Request, ResourcePackage>>(HandleResourceRequest);

        CreateConsumers();
    }

    void CreateConsumers()
    {
        if (owner.Definition.Resource.Shield.Enabled)   Register(new ShieldConsumer(this), distributer: true);
        if (owner.Definition.Resource.Armor.Enabled)    Register(new ArmorConsumer (this), distributer: true);
        if (owner.Definition.Resource.Health.Enabled)   Register(new HealthConsumer(this), distributer: true);
        if (owner.Definition.Resource.Mana.Enabled)     Register(new ManaConsumer  (this), distributer: false);
    }   

    // ===============================================================================

    public void Tick()
    {
        ProcessQueue();
    }

    // ===============================================================================

    void ProcessQueue()
    {
        foreach (var package in queue)
        {
            Process(package);
        }
    }

    void Process(ResourcePackage request)
    {
        switch (request.Route)
        {
            case Route.Direct:      ProcessDirect     (request); break;
            case Route.Broadcast:   ProcessBroadcast  (request); break;
            case Route.Distributed: ProcessDistributed(request); break;
        }
    }

    void ProcessDirect(ResourcePackage request)
    {
        consumers[request.Target]?.Consume(request.Component);
    }

    void ProcessBroadcast(ResourcePackage request)
    {
        foreach (var consumer in consumers.Values)
            consumer.Consume(request.Component);
    }

    void ProcessDistributed(ResourcePackage request)
    {
        foreach (var consumer in distributed)
        {
            if (consumer.CanConsume(request.Component))
                consumer.Consume(request.Component);

            if (request.Component.Consumed) break;
        }
    }

    T Get<T>() where T : IResourceConsumer 
    { 
        consumers.TryGetValue(typeof(T), out IResourceConsumer consumer); 
        return (T)consumer; 
    }

    void Register(IResourceConsumer consumer, bool distributer = false)
    {
        if (distributer)
            distributed.Add(consumer);

        consumers[consumer.GetType()] = consumer;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleResourceRequest(Message<Request, ResourcePackage> message)
    {
        queue.Add(message.Payload);
    }

    public float Shield => Get<ShieldConsumer>().Shield;
    public float Armor  => Get<ArmorConsumer>().Armor;
    public float Health => Get<HealthConsumer>().Health;
    public float Mana   => Get<ManaConsumer>  ().Mana;

    public Actor Owner => owner;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IResourceAction { public float Amount { get; init; } }


public interface IResourceConsumer : IConsumer<ResourceComponent>
{
    bool CanConsume(ResourceComponent request);
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Structs                                                   
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct Heal : IResourceAction
{
    public float Amount                     { get; init; }

    public Heal(float amount)
    {
        Amount = amount;
    }
}

public readonly struct Repair : IResourceAction
{
    public float Amount                     { get; init; }

    public Repair(float amount)
    {
        Amount = amount;
    }
}

public readonly struct Charge : IResourceAction
{
    public float Amount                     { get; init; }

    public Charge(float amount)
    {
        Amount = amount;
    }
}

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

public readonly struct Restore : IResourceAction
{
    public float Amount                     { get; init; }

    public Restore(float amount)
    {
        Amount = amount;
    }
}

public readonly struct Cast : IResourceAction
{
    public float Amount                     { get; init; }

    public Cast(float amount)
    {
        Amount = amount;
    }
}

public readonly struct HealthReset      : IResourceAction { public float Amount { get; init; } }
public readonly struct ArmorReset       : IResourceAction { public float Amount { get; init; } }
public readonly struct ShieldReset      : IResourceAction { public float Amount { get; init; } }
public readonly struct ManaReset        : IResourceAction { public float Amount { get; init; } }
public readonly struct ResourceReset    : IResourceAction { public float Amount { get; init; } }

public class ResourceComponent
{
    public IResourceAction  Action          { get; set; }
    public float            Remaining       { get; set; }
    public bool             Consumed        { get; set; }

    public ResourceComponent(IResourceAction action)
    {
        Action      = action;
        Remaining   = action.Amount;
        Consumed    = false;
    }
}

public class ResourcePackage
{
    public ResourceComponent Component          { get; init; }
    public Route             Route              { get; init; }
    public Type              Target             { get; init; }
    
    public ResourcePackage(ResourceComponent component, Route route, Type target = null)
    {
        Component   = component;
        Route       = route;
        Target      = target;
    }

    
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Handlers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Shield                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ShieldConsumer : IResourceConsumer
{
    readonly Actor actor;           
    readonly Resources resources;
    readonly ResourceMonitor monitor;

        // -----------------------------------

    float shield;

    // ===============================================================================

    public ShieldConsumer(Resources resources)
    {
        this.resources   = resources;
        this.actor       = resources.Owner;
        this.monitor     = new ResourceMonitor(actor, actor.Definition.Resource.Shield, OnShieldAlert);
    }

    // ===============================================================================

    public void Consume(ResourceComponent component)
    {
        switch(component.Action)
        {
            case Charge:         ProcessCharge(component); break;
            case Damage:         if (Damageable() && ApplicableDamage(component)) ProcessDamage(component); break;
            case ShieldReset:    ProcessReset ();        break;
            case ResourceReset:  ProcessReset ();        break;
        }

        monitor.Evaluate(shield, MaxShield);
    }

    // ===============================================================================

    void ProcessCharge(ResourceComponent component)
    {
        ApplyCharge((Charge)component.Action);
    }

    void ApplyCharge(Charge instance)
    {
        shield = Mathf.Min(shield + instance.Amount, MaxShield);
    }

    void ProcessDamage(ResourceComponent component)
    {
        float absorbed       = Mathf.Min(shield, component.Remaining);
        component.Remaining -= absorbed;
        component.Consumed   = component.Remaining <= 0;
        ApplyDamage(absorbed);
    }

    void ApplyDamage(float amount)
    {
        shield = Mathf.Max(0f, shield - amount);
    }

    void ProcessReset()
    {
        Reset();
    }

    void Reset()
    {
        shield = MaxShield;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void OnShieldAlert(float current, float max, float previous, float percent)
    {
        // actor.Emit.Local(Publish.Changed, new ShieldEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    public bool CanConsume(ResourceComponent component)
    {
        if (component.Consumed) 
            return false;

        return component.Action is Charge or Damage or ShieldReset or ResourceReset;
    }

    public bool Damageable()
    {
        return !Mortal.Invulnerable;
    }

    public bool ApplicableDamage(ResourceComponent component)
    {
        if (!applicableTypes.Contains(((Damage)component.Action).Type))
            return false;

        return true;
    }

    // ===============================================================================

    static readonly HashSet<DamageType> applicableTypes = new()
    {
        DamageType.Fire,
        DamageType.Frost,
        DamageType.Shock,
        DamageType.Explosion,
    };


    public float Shield             => shield;
    public float MaxShield          => Shielded.MaxShield;
    public IMortal Mortal           => actor as IMortal;
    public IShielded Shielded       => actor as IShielded;
    public float ShieldhPercent     => shield / MaxShield;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Armor                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ArmorConsumer : IResourceConsumer
{
    readonly Actor actor;           
    readonly Resources resources;
    readonly ResourceMonitor monitor;

        // -----------------------------------

    float armor;

    // ===============================================================================

    public ArmorConsumer(Resources resources)
    {
        this.resources   = resources;
        this.actor       = resources.Owner;
        this.monitor     = new ResourceMonitor(actor, actor.Definition.Resource.Armor, OnArmorAlert);
    }

    // ===============================================================================

    public void Consume(ResourceComponent component)
    {
        switch(component.Action)
        {
            case Repair:        ProcessRepair(component);   break;
            case Damage:        if (Damageable() && ApplicableDamage(component)) ProcessDamage(component); break;
            case ArmorReset:    ProcessReset ();            break;
            case ResourceReset: ProcessReset ();            break;
        }

        monitor.Evaluate(armor, MaxArmor);
    }

    // ===============================================================================

    void ProcessRepair(ResourceComponent component)
    {
        ApplyRepair((Repair)component.Action);
    }

    void ApplyRepair(Repair instance)
    {
        armor = Mathf.Min(armor + instance.Amount, MaxArmor);
    }

    void ProcessDamage(ResourceComponent component)
    {
        float absorbed       = Mathf.Min(armor, component.Remaining);
        component.Remaining -= absorbed;
        component.Consumed   = component.Remaining <= 0;
        ApplyDamage(absorbed);
    }

    void ApplyDamage(float amount)
    {
        armor = Mathf.Max(0f, armor - amount);
    }

    void ProcessReset()
    {
        Reset();
    }

    void Reset()
    {
        armor = MaxArmor;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void OnArmorAlert(float current, float max, float previous, float percent)
    {
        // actor.Emit.Local(Publish.Changed, new ShieldEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    public bool CanConsume(ResourceComponent component)
    {
        if (component.Consumed) 
            return false;

        return component.Action is Repair or Damage or ArmorReset or ResourceReset;
    }

    public bool Damageable()
    {
        return !Mortal.Invulnerable;
    }

    public bool ApplicableDamage(ResourceComponent component)
    {
        if (!applicableTypes.Contains(((Damage)component.Action).Type))
            return false;

        return true;
    }

    // ===============================================================================

    static readonly HashSet<DamageType> applicableTypes = new()
    {
        DamageType.Fire,
        DamageType.Frost,
        DamageType.Shock,
        DamageType.Explosion,
    };


    public float Armor              => armor;
    public float MaxArmor           => Armored.MaxArmor;
    public IMortal Mortal           => actor as IMortal;
    public IArmored Armored         => actor as IArmored;
    public float ShieldPercent      => armor / MaxArmor;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Health                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class HealthConsumer : IResourceConsumer
{
    readonly Actor actor;           
    readonly Resources resources;
    readonly ResourceMonitor monitor;

        // -----------------------------------

    float health;

    // ===============================================================================

    public HealthConsumer(Resources resources)
    {
        this.resources   = resources;
        this.actor       = resources.Owner;
        this.monitor     = new ResourceMonitor(actor, actor.Definition.Resource.Health, OnHealthAlert);
    }

    // ===============================================================================

    public void Consume(ResourceComponent request)
    {
        switch(request.Action)
        {
            case Heal:           ProcessHeal  (request); break;
            case Damage:         ProcessDamage(request); break;
            case HealthReset:    ProcessReset ();        break;
            case ResourceReset:  ProcessReset ();        break;
        }

        monitor.Evaluate(health, MaxHealth);
    }

    // ===============================================================================

    void ProcessHeal(ResourceComponent component)
    {
        ApplyHeal((Heal)component.Action);
    }

    void ApplyHeal(Heal instance)
    {
        health = Mathf.Min(health + instance.Amount, MaxHealth);
    }

    void ProcessDamage(ResourceComponent component)
    {
        if (IsDamageable())
            ApplyDamage((Damage)component.Action);
    }

    void ApplyDamage(Damage instance)
    {
        health = Mathf.Max(0f, health - instance.Amount);
    }

    void ProcessReset()
    {
        Reset();
    }

    void Reset()
    {
        health = MaxHealth;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void OnHealthAlert(float current, float max, float previous, float percent)
    {
        actor.Emit.Local(Publish.Changed, new HealthEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    public bool CanConsume(ResourceComponent request)
    {
        if (request.Consumed) 
            return false;

        return request.Action is Heal or Damage or HealthReset or ResourceReset;
    }

    public bool IsDamageable()
    {
        return !Damageable.Invulnerable;
    }

    // ===============================================================================

    public float Health             => health;
    public float MaxHealth          => Damageable.MaxHealth;
    public IMortal Damageable       => actor as IMortal;
    public float HealthPercent      => health / MaxHealth;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Mana                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ManaConsumer : IResourceConsumer
{
    readonly Actor actor;           
    readonly Resources resources;
    readonly ResourceMonitor monitor;

        // -----------------------------------

    float mana;

    // ===============================================================================

    public ManaConsumer(Resources resources)
    {
        this.resources   = resources;
        this.actor       = resources.Owner;
        this.monitor     = new ResourceMonitor(actor, actor.Definition.Resource.Mana, OnManaAlert);
    }

    // ===============================================================================

    public void Consume(ResourceComponent request)
    {
        switch(request.Action)
        {
            case Restore:       ProcessRestore  (request); break;
            case Cast:          ProcessCast     (request); break;
            case ManaReset:     ProcessReset    ();        break;
            case ResourceReset: ProcessReset    ();        break;
        }

        monitor.Evaluate(mana, MaxMana);
    }

    // ===============================================================================

    void ProcessRestore(ResourceComponent component)
    {
        ApplyRestore((Restore)component.Action);
    }

    void ApplyRestore(Restore instance)
    {
        mana = Mathf.Min(mana + instance.Amount, MaxMana);
    }

    void ProcessCast(ResourceComponent component)
    {

        ApplyCast((Cast)component.Action);
    }

    void ApplyCast(Cast instance)
    {
        mana = Mathf.Max(0f, mana - instance.Amount);
    }

    void ProcessReset()
    {
        Reset();
    }

    void Reset()
    {
        mana = MaxMana;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void OnManaAlert(float current, float max, float previous, float percent)
    {
        // actor.Emit.Local(Publish.Changed, new ManaEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    public bool CanConsume(ResourceComponent request)
    {
        if (request.Consumed) 
            return false;

        return request.Action is Restore or Cast or ManaReset or ResourceReset;
    }

    // ===============================================================================

    public float Mana               => mana;
    public float MaxMana            => Caster.MaxMana;
    public ICaster Caster           => actor as ICaster;
    public float ManaPercent        => mana / MaxMana;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                            Resource Monitor                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class ResourceMonitor
{
    readonly Actor                      actor;
    readonly ResourceConfig             config;
    readonly HashSet<ResourceThreshold> active  = new();

    Action<float, float, float, float>  onAlert;
    float                               lastPercent;

    // ===============================================================================

    public ResourceMonitor(Actor actor, ResourceConfig config, Action<float, float, float, float> onAlert = null)
    {
        this.actor   = actor;
        this.config  = config;
        this.onAlert = onAlert;
    }

    // ===============================================================================

    public void Evaluate(float current, float max)
    {
        float percent = current / max;

        if (config.EnableThresholds)
            EvaluateThresholds(percent);

        if (config.AlertOnChange)
            EvaluateAlert(current, max, percent);

        lastPercent = percent;
    }

    // ===============================================================================
    //  Thresholds
    // ===============================================================================

    void EvaluateThresholds(float current)
    {
        foreach (var threshold in config.Thresholds)
            EvaluateThreshold(threshold, current);
    }

    void EvaluateThreshold(ResourceThreshold threshold, float current)
    {
        bool wasActive = active.Contains(threshold);
        bool isActive  = current <= threshold.Percentage;

        if      (isActive  && !wasActive) EnterThreshold(threshold);
        else if (!isActive && wasActive)  ExitThreshold(threshold);
    }

    void EnterThreshold(ResourceThreshold threshold)
    {
        if (threshold.Trigger is ThresholdTrigger.OnEnter or ThresholdTrigger.OnCross)
            TriggerThreshold(threshold);
        active.Add(threshold);
    }

    void ExitThreshold(ResourceThreshold threshold)
    {
        if (threshold.Trigger is ThresholdTrigger.OnExit or ThresholdTrigger.OnCross)
            TriggerThreshold(threshold);
        active.Remove(threshold);
    }

    void TriggerThreshold(ResourceThreshold threshold)
    {
        foreach (var effect in threshold.Effects)
            actor.Emit.Local(Request.Create, new EffectDeclarationEvent(actor, effect));
    }

    // ===============================================================================
    //  Alerts
    // ===============================================================================

    void EvaluateAlert(float current, float max, float percent)
    {
        if (Mathf.Abs(percent - lastPercent) <= 0.01f) return;

        onAlert?.Invoke(current, max, lastPercent, percent);
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Factories
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public static class Apply
{
    public static ResourcePackage Heal(float amount)
        => new(new(new Heal(amount)), Route.Direct, typeof(HealthConsumer));

    public static ResourcePackage Repair(float amount)
        => new(new(new Repair(amount)), Route.Direct, typeof(ArmorConsumer));

    public static ResourcePackage Charge(float amount)
        => new(new(new Charge(amount)), Route.Direct, typeof(ShieldConsumer));

    public static ResourcePackage Damage(float amount, DamageType type)
        => new(new(new Damage(amount, type)), Route.Distributed);

    public static ResourcePackage Cast(float amount)
        => new(new(new Cast(amount)), Route.Direct, typeof(ManaConsumer));

    public static ResourcePackage Restore(float amount)
        => new(new(new Restore(amount)), Route.Direct, typeof(ManaConsumer));

    
    public static ResourcePackage HealthReset()
        => new(new(new HealthReset()), Route.Direct, typeof(HealthConsumer));

    public static ResourcePackage ArmorReset()
        => new(new(new ArmorReset()), Route.Direct, typeof(ArmorConsumer));

    public static ResourcePackage ShieldReset()
        => new(new(new ShieldReset()), Route.Direct, typeof(ShieldConsumer));

    public static ResourcePackage ManaReset()
        => new(new(new ManaReset()), Route.Direct, typeof(ManaConsumer));

    public static ResourcePackage ResourcesReset()
        => new(new(new ResourceReset()), Route.Broadcast);

}
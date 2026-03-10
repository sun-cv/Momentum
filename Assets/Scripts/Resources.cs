using System;
using System.Collections.Generic;
using UnityEngine;



public class Resources
{
    readonly Actor owner;

        // -----------------------------------

    readonly Dictionary<Type, IResourceHandler> handlers = new();

    // ===============================================================================
    
    public Resources(Actor actor)
    {
        owner = actor;
        CreateConsumers();
    }

    void CreateConsumers()
    {
        if (owner is IHealth)   Register(new HealthHandler(this));
        if (owner is IArmor)    Register(new ArmorHandler (this));
        if (owner is IShield)   Register(new ShieldHandler(this));
        if (owner is ICaster)   Register(new EnergyHandler(this));
    }   

    // ===============================================================================

    T Get<T>() where T : IResourceHandler 
    { 
        handlers.TryGetValue(typeof(T), out IResourceHandler consumer); 
        return (T)consumer; 
    }

    void Register(IResourceHandler handler)
    {
        handlers[handler.GetType()] = handler;
    }

    public float Health => Get<HealthHandler>().Health;
    public float Armor  => Get<ArmorHandler> ().Armor;
    public float Shield => Get<ShieldHandler>().Shield;
    public float Energy => Get<EnergyHandler>().Energy;

    public Actor Owner => owner;
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IResourceAction { public float Amount { get; init; } }

public interface IResourceHandler {}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct Heal : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Heal(float amount)
    {
        Amount = amount;
    }
}

public readonly struct Wound : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Wound(float amount)
    {
        Amount = amount;
    }
}


public readonly struct Repair : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Repair(float amount)
    {
        Amount = amount;
    }
}

public readonly struct Fracture : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Fracture(float amount)
    {
        Amount = amount;
    }
}


public readonly struct Restore : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Restore(float amount)
    {
        Amount = amount;
    }
}

public readonly struct Dissipate : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Dissipate(float amount)
    {
        Amount = amount;
    }
}


public readonly struct Expend : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Expend(float amount)
    {
        Amount = amount;
    }
}

public readonly struct Recharge : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Recharge(float amount)
    {
        Amount = amount;
    }
}


public readonly struct HealthReset      : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct ArmorReset       : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct ShieldReset      : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct EnergyReset      : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct ResourceReset    : IMessage, IResourceAction { public float Amount { get; init; } }

public readonly struct HealthEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly float Health            { get; init; }
    public readonly float MaxHealth         { get; init; }
    public readonly float CurrentPercent    { get; init; }
    public readonly float LastPercent       { get; init; }

    public HealthEvent(Actor owner, float health, float maxHealth, float current, float last)
    {
        Owner           = owner;
        Health          = health;
        MaxHealth       = maxHealth;
        CurrentPercent  = current;
        LastPercent     = last;
    }
}

public readonly struct ArmorEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly float Armor             { get; init; }
    public readonly float MaxArmor          { get; init; }
    public readonly float CurrentPercent    { get; init; }
    public readonly float LastPercent       { get; init; }

    public ArmorEvent(Actor owner, float armor, float maxArmor, float current, float last)
    {
        Owner           = owner;
        Armor           = armor;
        MaxArmor        = maxArmor;
        CurrentPercent  = current;
        LastPercent     = last;
    }
}

public readonly struct ShieldEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly float Shield            { get; init; }
    public readonly float MaxShield         { get; init; }
    public readonly float CurrentPercent    { get; init; }
    public readonly float LastPercent       { get; init; }

    public ShieldEvent(Actor owner, float shield, float maxShield, float current, float last)
    {
        Owner           = owner;
        Shield          = shield;
        MaxShield       = maxShield;
        CurrentPercent  = current;
        LastPercent     = last;
    }
}

public readonly struct EnergyEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly float Energy            { get; init; }
    public readonly float MaxEnergy         { get; init; }
    public readonly float CurrentPercent    { get; init; }
    public readonly float LastPercent       { get; init; }

    public EnergyEvent(Actor owner, float energy, float maxEnergy, float current, float last)
    {
        Owner           = owner;
        Energy          = energy;
        MaxEnergy       = maxEnergy;
        CurrentPercent  = current;
        LastPercent     = last;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Handlers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ResourceHandler : Service, IServiceLoop, IResourceHandler
{
    protected readonly Actor        actor;
    protected readonly Resources    resources;

        // -----------------------------------

    readonly ResourceMonitor        monitor;

        // -----------------------------------

    protected readonly List<IResourceAction> queue = new();

        // -----------------------------------

    protected Func<float>   max         = () => 0f;
    protected Func<float>   regen       = () => 0f;

    protected RegenHandler  regeneration;

        // -----------------------------------

    float value;

    // ===============================================================================

    public ResourceHandler(Resources resources, ResourceConfig config)
    {
        this.resources  = resources;
        this.actor      = resources.Owner;
        this.monitor    = new ResourceMonitor(actor, config, OnAlert);
    }

    // ===============================================================================

    public void Loop()
    {
        Regenerate();
        ProcessQueue();
        Monitor();
    }

    // ===============================================================================

    void Regenerate()
    {
        if (regeneration == null) 
            return;

        float amount = regeneration.Tick();

        if (amount > 0) 
            Increase(amount);
    }

    void ProcessQueue()
    {
        if (queue.Count == 0) 
            return;

        foreach (var action in queue)
        {
            ProcessAction(action);
        }

        queue.Clear();
    }

    void Monitor()
    {
        monitor.Evaluate(value, Max);
    }

    // ===============================================================================

    protected virtual void ProcessAction(IResourceAction action)                            { }
    protected virtual void OnAlert(float current, float max, float previous, float percent) { }

    // ===============================================================================

    protected void Increase(float amount)
    {
        value = Mathf.Min(value + amount, Max);
    }

    protected void Decrease(float amount)
    {
        value = Mathf.Max(0f, value - amount);
    }

    protected void Reset()               
    {
        value = Max;
    }

    protected void Queue<T>(T message) where T : IResourceAction
    {
        queue.Add(message);
    }
    

    // ===============================================================================

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority  => ServiceUpdatePriority.Resources;
    public float Value              => value;
    public float Max                => max();
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Health                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class HealthHandler : ResourceHandler
{
    readonly IMortal mortal;

    // ===============================================================================

    public HealthHandler(Resources resources) : base(resources, resources.Owner.Definition.Resource.Health)
    {
        mortal              = actor as IMortal;

        max                 = () => mortal.MaxHealth;

        if (actor is IHealthRegen instance)
        {
            regeneration    = new RegenHandler(() => instance.HealthRegen);
        }
        
        actor.Emit.Link.Local<Heal>         (Queue);
        actor.Emit.Link.Local<Wound>        (Queue);
        actor.Emit.Link.Local<HealthReset>  (Queue);
        actor.Emit.Link.Local<ResourceReset>(Queue);

        Reset();
    }

    // ===============================================================================

    protected override void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Heal        instance: Increase(instance.Amount); break;
            case Wound       instance: Decrease(instance.Amount); break;
            case HealthReset:          Reset();                   break;
            case ResourceReset:        Reset();                   break;
        }
    }

    // ===============================================================================

    protected override void OnAlert(float current, float max, float previous, float percent)
    {
        actor.Emit.Local(Publish.Changed, new HealthEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================

    public float Health => Value;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Armor                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ArmorHandler : ResourceHandler
{
    readonly IArmor armored;

    // ===============================================================================

    public ArmorHandler(Resources resources) : base(resources, resources.Owner.Definition.Resource.Armor)
    {
        armored = actor as IArmor;

        max     = () => armored.MaxArmor;

        actor.Emit.Link.Local<Repair>       (Queue);
        actor.Emit.Link.Local<Fracture>     (Queue);
        actor.Emit.Link.Local<ArmorReset>   (Queue);
        actor.Emit.Link.Local<ResourceReset>(Queue);

        Reset();
    }

    // ===============================================================================

    protected override void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Repair      instance: Increase(instance.Amount); break;
            case Fracture    instance: Decrease(instance.Amount); break;
            case ArmorReset:           Reset();                   break;
            case ResourceReset:        Reset();                   break;
        }
    }

    // ===============================================================================

    protected override void OnAlert(float current, float max, float previous, float percent)
    {
        actor.Emit.Local(Publish.Changed, new ArmorEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================

    public float Armor => Value;
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Shield                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ShieldHandler : ResourceHandler
{
    readonly IShield shielded;

    // ===============================================================================

    public ShieldHandler(Resources resources) : base(resources, resources.Owner.Definition.Resource.Shield)
    {
        shielded            = actor as IShield;

        max                 = () => shielded.MaxShield;

        if (actor is IShieldRegen instance)
        {
            regeneration    = new RegenHandler(() => instance.ShieldRegen);
        }

        actor.Emit.Link.Local<Restore>      (Queue);
        actor.Emit.Link.Local<Dissipate>    (Queue);
        actor.Emit.Link.Local<ShieldReset>  (Queue);
        actor.Emit.Link.Local<ResourceReset>(Queue);
        actor.Emit.Link.Local<ResourceReset>(Queue);

        Reset();
    }

    // ===============================================================================

    protected override void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Restore     instance: Increase(instance.Amount); break;
            case Dissipate   instance: Decrease(instance.Amount); break;
            case ShieldReset:          Reset();                   break;
            case ResourceReset:        Reset();                   break;
        }
    }

    // ===============================================================================

    protected override void OnAlert(float current, float max, float previous, float percent)
    {
        actor.Emit.Local(Publish.Changed, new ShieldEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================

    public float Shield => Value;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Energy                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class EnergyHandler : ResourceHandler
{
    readonly ICaster caster;

    // ===============================================================================

    public EnergyHandler(Resources resources) : base(resources, resources.Owner.Definition.Resource.Energy)
    {
        caster              = actor as ICaster;

        max                 = () => caster.MaxEnergy;

        if (actor is IEnergyRegen instance)
        {
            regeneration    = new RegenHandler(() => instance.EnergyRegen);
        }

        actor.Emit.Link.Local<Recharge>     (Queue);
        actor.Emit.Link.Local<Expend>       (Queue);
        actor.Emit.Link.Local<EnergyReset>  (Queue);
        actor.Emit.Link.Local<ResourceReset>(Queue);

        Reset();
    }

    // ===============================================================================

    protected override void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Recharge  instance: Increase(instance.Amount); break;
            case Expend    instance: Decrease(instance.Amount); break;
            case EnergyReset:        Reset();                   break;
            case ResourceReset:      Reset();                   break;
        }
    }

    // ===============================================================================

    protected override void OnAlert(float current, float max, float previous, float percent)
    {
        actor.Emit.Local(Publish.Changed, new EnergyEvent(actor, current, max, previous, percent));
    }

    // ===============================================================================

    public float Energy => Value;
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

        if (config.Thresholds.Count > 0)
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
            actor.Emit.Local(Request.Create, new EffectAPI(actor, effect));
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
//                                         Helpers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class RegenHandler
{
    readonly Func<float> regen;

    public RegenHandler(Func<float> regen)
    {
        this.regen      = regen;
    }

    public float Tick()
    {
        if (!Enabled) 
            return 0;

        return regen() * Clock.DeltaTime;
    }

    public bool Enabled => regen() > 0;
}
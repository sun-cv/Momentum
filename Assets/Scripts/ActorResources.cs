using System;
using System.Collections.Generic;
using UnityEngine;



public class Resources : ActorService
{

    readonly Dictionary<Type, IResourceHandler> handlers = new();

    // ===============================================================================
    
    public Resources(Actor actor) : base(actor)
    {
        RegisterHandlers();

        Enable();
    }

    // ===============================================================================

    T Get<T>() where T : IResourceHandler 
    { 
        handlers.TryGetValue(typeof(T), out IResourceHandler consumer); 
        return (T)consumer; 
    }

    void RegisterHandlers()
    {
        if (owner is IShield)   Register(new ShieldHandler   (owner));
        if (owner is IArmor )   Register(new ArmorHandler    (owner));
        if (owner is IHealth)   Register(new HealthHandler   (owner));
        if (owner is IEnergy)   Register(new EnergyHandler   (owner));
        if (owner is ICorpse)   Register(new IntegrityHandler(owner));
    }   

    void Register(IResourceHandler handler)
    {
        handlers[handler.GetType()] = handler;
    }

    // ===============================================================================

    // readonly Logger Log = Logging.For(LogSystem.Resources);

    // ===============================================================================

    public float Health     => Get<HealthHandler>   ().Health;
    public float Armor      => Get<ArmorHandler>    ().Armor;
    public float Shield     => Get<ShieldHandler>   ().Shield;
    public float Energy     => Get<EnergyHandler>   ().Energy;
    public float Integrity  => Get<IntegrityHandler>().Integrity;
}





// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                        Handlers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Shield                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ShieldHandler : ActorService, IServiceLoop, IResourceHandler
{

    readonly ResourceMonitor    monitor;
    readonly RegenHandler       regeneration;

        // -----------------------------------

    readonly List<IResourceAction> queue = new();

        // -----------------------------------

    float shield;

    // ===============================================================================

    public ShieldHandler(Actor actor) : base(actor)
    {
        this.monitor    = new ResourceMonitor(owner, owner.Definition.Stats.Shield, OnShieldAlert);

        if (owner is IShieldRegen instance)
        {
            regeneration = new RegenHandler(() => instance.ShieldRegen);
        }

        owner.Bus.Link.Local<Restore>      (Queue);
        owner.Bus.Link.Local<Dissipate>    (Queue);
        owner.Bus.Link.Local<ShieldReset>  (Queue);
        owner.Bus.Link.Local<ResourceReset>(Queue);

        Reset();
        Enable();
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
        if (regeneration == null) return;

        float amount = regeneration.Tick();
        
        if (amount > 0) 
            Restore(amount);
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
        monitor.Evaluate(shield, MaxShield);
    }

    // ===============================================================================

    void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Restore        instance:   Restore(instance.Amount);   break;
            case Dissipate      instance:   Dissipate(instance.Amount); break;
            case ShieldReset:               Reset();                    break;
            case ResourceReset:             Reset();                    break;
        }
    }

    // ===============================================================================

    void Restore(float amount)
    {
        shield = Mathf.Min(shield + amount, MaxShield);
    }

    void Dissipate(float amount)
    {
        shield = Mathf.Max(0f, shield - amount);
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
        owner.Bus.Emit.Local(Publish.Changed, new ShieldEvent(owner, current, max, previous, percent));
    }

    void Queue<T>(T message) where T : IResourceAction
    {
        queue.Add(message);
    }

    // ===============================================================================

    public float Shield             => shield;
    public float MaxShield          => Shielded.MaxShield;
    public float ShieldPercent      => shield / MaxShield;
    public IMortal Mortal           => owner as IMortal;
    public IShield Shielded         => owner as IShield;
    public IShieldEquipped Equipped => owner as IShieldEquipped;
    public UpdatePriority Priority  => ServiceUpdatePriority.Resources;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Armor                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class ArmorHandler : ActorService, IServiceLoop, IResourceHandler
{

    readonly ResourceMonitor    monitor;

        // -----------------------------------

    readonly List<IResourceAction> queue = new();

        // -----------------------------------

    float armor;

    // ===============================================================================

    public ArmorHandler(Actor actor) : base(actor)
    {
        this.monitor    = new ResourceMonitor(owner, owner.Definition.Stats.Armor, OnArmorAlert);

        owner.Bus.Link.Local<Repair>        (Queue);
        owner.Bus.Link.Local<Fracture>      (Queue);
        owner.Bus.Link.Local<ArmorReset>   (Queue);
        owner.Bus.Link.Local<ResourceReset> (Queue);

        Reset();
        Enable();
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessQueue();
        Monitor();
    }

    // ===============================================================================

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
        monitor.Evaluate(armor, MaxArmor);
    }

    // ===============================================================================

    void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Repair         instance: Repair (instance.Amount);     break;
            case Fracture       instance: Fracture(instance.Amount);    break;
            case ArmorReset:              Reset();                      break;
            case ResourceReset:           Reset();                      break;
        }
    }

    // ===============================================================================

    void Repair(float amount)
    {
        armor = Mathf.Min(armor + amount, MaxArmor);
    }

    void Fracture(float amount)
    {
        armor = Mathf.Max(0f, armor - amount);
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
        owner.Bus.Emit.Local(Publish.Changed, new ArmorEvent(owner, current, max, previous, percent));
    }

    void Queue<T>(T message) where T : IResourceAction
    {
        queue.Add(message);
    }

    // ===============================================================================

    public float Armor              => armor;
    public float MaxArmor           => Armored.MaxArmor;
    public float ArmorPercent       => armor / MaxArmor;
    public IMortal Mortal           => owner as IMortal;
    public IArmor Armored           => owner as IArmor;
    public UpdatePriority Priority  => ServiceUpdatePriority.Resources;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Health                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class HealthHandler : ActorService, IServiceLoop, IResourceHandler
{

    readonly ResourceMonitor    monitor;
    readonly RegenHandler       regeneration;

        // -----------------------------------

    readonly List<IResourceAction> queue = new();

        // -----------------------------------

    float health;

    // ===============================================================================

    public HealthHandler(Actor actor) : base(actor)
    {
        this.monitor    = new ResourceMonitor(owner, owner.Definition.Stats.Health, OnHealthAlert);

        if (owner is IHealthRegen instance)
        {
            regeneration = new RegenHandler(() => instance.HealthRegen);
        }

        owner.Bus.Link.Local<Heal>         (Queue);
        owner.Bus.Link.Local<Wound>        (Queue);
        owner.Bus.Link.Local<HealthReset>  (Queue);
        owner.Bus.Link.Local<ResourceReset>(Queue);

        Reset();
        Enable();
    }

    // ===============================================================================

    public void Loop()
    {
        Regenerate();
        ProcessQueue();
        Monitor();

        DebugLog();
    }

    // ===============================================================================

    void Regenerate()
    {
        if (regeneration == null) return;

        float amount = regeneration.Tick();
        
        if (amount > 0) 
            Heal(amount);
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
        monitor.Evaluate(health, MaxHealth);
    }

    // ===============================================================================

    void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Heal         instance: Heal (instance.Amount); break;
            case Wound        instance: Wound(instance.Amount); break;
            case HealthReset:           Reset();                break;
            case ResourceReset:         Reset();                break;
        }
    }

    // ===============================================================================

    void Heal(float amount)
    {
        health = Mathf.Min(health + amount, MaxHealth);
    }

    void Wound(float amount)
    {
        health = Mathf.Max(0f, health - amount);
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

    }

    void Queue<T>(T message) where T : IResourceAction
    {
        queue.Add(message);
    }

    // ===============================================================================

    void DebugLog()
    {
        Logging.For(LogSystem.Resources).Trace($"{owner.GetType().Name}.Health", () => health, clean: true);
    }

    public float Health             => health;
    public float MaxHealth          => Mortal.MaxHealth;
    public float HealthPercent      => health / MaxHealth;
    public IMortal Mortal           => owner as IMortal;
    public UpdatePriority Priority  => ServiceUpdatePriority.Resources;
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Energy                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class EnergyHandler : ActorService, IServiceLoop, IResourceHandler
{

    readonly ResourceMonitor    monitor;
    readonly RegenHandler       regeneration;

        // -----------------------------------

    readonly List<IResourceAction> queue = new();

        // -----------------------------------

    float energy;

    // ===============================================================================

    public EnergyHandler(Actor actor) : base(actor)
    {
        this.monitor    = new ResourceMonitor(owner, owner.Definition.Stats.Health, OnEnergyAlert);

        if (owner is IEnergyRegen instance)
        {
            regeneration = new RegenHandler(() => instance.EnergyRegen);
        }

        owner.Bus.Link.Local<Recharge>     (Queue);
        owner.Bus.Link.Local<Expend>       (Queue);
        owner.Bus.Link.Local<EnergyReset>  (Queue);
        owner.Bus.Link.Local<ResourceReset>(Queue);

        Reset();
        Enable();
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
        if (regeneration == null) return;

        float amount = regeneration.Tick();
        
        if (amount > 0) 
            Recharge(amount);
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
        monitor.Evaluate(energy, MaxEnergy);
    }

    // ===============================================================================

    void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Recharge       instance: Recharge (instance.Amount);   break;
            case Expend         instance: Expend(instance.Amount);      break;
            case EnergyReset:             Reset();                      break;
            case ResourceReset:           Reset();                      break;
        }
    }

    // ===============================================================================

    void Recharge(float amount)
    {
        energy = Mathf.Min(energy + amount, MaxEnergy);
    }

    void Expend(float amount)
    {
        energy = Mathf.Max(0f, energy - amount);
    } 

    void Reset()
    {
        energy = MaxEnergy;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void OnEnergyAlert(float current, float max, float previous, float percent)
    {
        owner.Bus.Emit.Local(Publish.Changed, new EnergyEvent(owner, current, max, previous, percent));
    }

    void Queue<T>(T message) where T : IResourceAction
    {
        queue.Add(message);
    }

    // ===============================================================================

    public float Energy             => energy;
    public float MaxEnergy          => Energized.MaxEnergy;
    public float EnergyPercent      => energy / MaxEnergy;
    public IMortal Mortal           => owner as IMortal;
    public IEnergy Energized        => owner as IEnergy;
    public UpdatePriority Priority  => ServiceUpdatePriority.Resources;
}


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                Integrity                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public class IntegrityHandler : ActorService, IServiceLoop, IResourceHandler
{

    readonly ResourceMonitor    monitor;

        // -----------------------------------

    readonly List<IResourceAction> queue = new();

        // -----------------------------------

    float integrity;

    // ===============================================================================

    public IntegrityHandler(Actor actor) : base(actor)
    {
        this.monitor    = new ResourceMonitor(owner, owner.Definition.Stats.Integrity, OnIntegrityAlert);

        owner.Bus.Link.Local<Deplete>       (Queue);
        owner.Bus.Link.Local<IntegrityReset>(Queue);
        owner.Bus.Link.Local<ResourceReset> (Queue);

        Reset();
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessQueue();
        Monitor();
    }

    // ===============================================================================

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
        monitor.Evaluate(integrity, MaxIntegrity);
    }

    // ===============================================================================

    void ProcessAction(IResourceAction action)
    {
        switch (action)
        {
            case Deplete        instance: Deplete(instance.Amount);     break;
            case IntegrityReset:          Reset();                      break;
            case ResourceReset:           Reset();                      break;
        }
    }

    // ===============================================================================

    void Deplete(float amount)
    {
        integrity = Mathf.Max(0f, integrity - amount);
    } 

    void Reset()
    {
        integrity = MaxIntegrity;
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void OnIntegrityAlert(float current, float max, float previous, float percent)
    {
        owner.Bus.Emit.Local(Publish.Changed, new IntegrityEvent(owner, current, max, previous, percent));
    }

    void Queue<T>(T message) where T : IResourceAction
    {
        queue.Add(message);
    }

    // ===============================================================================

    public float Integrity          => integrity;
    public float MaxIntegrity       => Corpse.MaxIntegrity;
    public float IntegrityPercent   => integrity / MaxIntegrity;
    public ICorpse Corpse           => owner as ICorpse;
    public UpdatePriority Priority  => ServiceUpdatePriority.Resources;
}
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IResourceAction { public float Amount { get; init; }}

public interface IResourceHandler { }

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                  Enums                                                 
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum ResourceType
{
    Health,
    Armor,
    Shield,
    Energy
}

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

public readonly struct Recharge : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Recharge(float amount)
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

public readonly struct Deplete : IMessage, IResourceAction
{
    public float Amount                     { get; init; }

    public Deplete(float amount)
    {
        Amount = amount;
    }
}

public readonly struct HealthReset      : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct ArmorReset       : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct ShieldReset      : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct EnergyReset      : IMessage, IResourceAction { public float Amount { get; init; } }
public readonly struct IntegrityReset   : IMessage, IResourceAction { public float Amount { get; init; } }
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


public readonly struct IntegrityEvent : IMessage
{
    public readonly Actor Owner             { get; init; }
    public readonly float Integrity         { get; init; }
    public readonly float MaxIntegrity      { get; init; }
    public readonly float CurrentPercent    { get; init; }
    public readonly float LastPercent       { get; init; }

    public IntegrityEvent(Actor owner, float integrity, float maxIntegrity, float current, float last)
    {
        Owner           = owner;
        Integrity       = integrity;
        MaxIntegrity    = maxIntegrity;
        CurrentPercent  = current;
        LastPercent     = last;
    }
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                            Resource Monitor                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


public class ResourceMonitor
{
    readonly Actor                      owner;
    readonly ResourceConfig             config;
    readonly HashSet<ResourceThreshold> active  = new();

    readonly Action<float, float, float, float>  onAlert;
    float                               lastPercent;

    // ===============================================================================

    public ResourceMonitor(Actor owner, ResourceConfig config, Action<float, float, float, float> onAlert = null)
    {
        this.owner   = owner;
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
        {
            EvaluateThreshold(threshold, current);
        }
    }

    void EvaluateThreshold(ResourceThreshold threshold, float current)
    {
        bool wasActive = active.Contains(threshold);
        bool isActive  = current <= threshold.Percentage;

        switch(isActive, wasActive)
        {
            case (true, false): EnterThreshold(threshold); break;
            case (false, true): ExitThreshold(threshold); break;
        }
    }

    void EnterThreshold(ResourceThreshold threshold)
    {
        if (threshold.Trigger is ThresholdTrigger.OnEnter or ThresholdTrigger.OnCross)
        {
            TriggerThreshold(threshold);
        }
            
        active.Add(threshold);
    }

    void ExitThreshold(ResourceThreshold threshold)
    {
        if (threshold.Trigger is ThresholdTrigger.OnExit or ThresholdTrigger.OnCross)
        {
            TriggerThreshold(threshold);
        }

        active.Remove(threshold);
    }

    void TriggerThreshold(ResourceThreshold threshold)
    {
        foreach (var effect in threshold.Effects)
        {
            var API = new EffectAPI(owner, effect)
            {
                Request = Request.Create
            };

            owner.Bus.Emit.Local(API.Request, API);
        }
    }

    // ===============================================================================
    //  Alerts
    // ===============================================================================

    void EvaluateAlert(float current, float max, float percent)
    {
        if (Mathf.Abs(percent - lastPercent) <= 0.01f) return;

        onAlert?.Invoke(current, max, lastPercent, percent);
    }

    // void EvaluateEmptyAlert(float current, float max, float percent)
    // {
    //     if (current >= 0f) return;
    //
    //     onAlert?.Invoke(current, max, lastPercent, percent);
    // }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Helpers
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


// Rework required
public class RegenHandler
{
    readonly Func<float> regen;

    float timer;

    public RegenHandler(Func<float> regen)
    {
        this.regen = regen;
    }

    public float Tick()
    {
        if (!Enabled) return 0;

        timer += Clock.DeltaTime;

        if (timer < 1f) return 0;

        timer -= 1f;
        return regen();
    }

    public bool Enabled => regen() > 0;
}


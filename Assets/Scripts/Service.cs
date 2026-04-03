using System;



// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum ServiceRate { Tick, Loop, Step, Util }

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                               Interfaces                                                      
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public interface IService       : IDisposable   { public bool IsEnabled     { get; }};
public interface IServicePriority               { UpdatePriority Priority   { get; }};

public interface IServiceRate   { }
public interface IServiceTick   : IService, IServiceRate, IServicePriority  { public void Tick();               };
public interface IServiceLoop   : IService, IServiceRate, IServicePriority  { public void Loop();               };
public interface IServiceStep   : IService, IServiceRate, IServicePriority  { public void Step();               };
public interface IServiceUtil   : IService, IServiceRate, IServicePriority  { public void Util();               };
public interface IServiceLate   : IService, IServiceRate, IServicePriority  { public void Late();    
};
public interface IPassiveRate   { }
public interface IPassiveTick   : IService, IPassiveRate, IServicePriority  { public void PassiveTick();        };
public interface IPassiveLoop   : IService, IPassiveRate, IServicePriority  { public void PassiveLoop();        };
public interface IPassiveStep   : IService, IPassiveRate, IServicePriority  { public void PassiveStep();        };
public interface IPassiveUtil   : IService, IPassiveRate, IServicePriority  { public void PassiveUtil();        };
public interface IPassiveLate   : IService, IPassiveRate, IServicePriority  { public void PassiveLate();        };


        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Classes                                                    
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

[Service]
public abstract class RegisteredService : Service {}

public abstract class Service : IService
{
    public Guid RuntimeID               { get; } = Guid.NewGuid();

        // -----------------------------------

    protected bool enabled  = false;
    protected bool active   = false;
    protected bool passive  = false;

    // ===============================================================================

    public void Restart()
    {
        OnRestart();
    }

    public void Enable()
    {
        enabled = true;
        
        Active();
        OnEnable();
    }

    public void Active() 
    {
        if (!enabled || active) return;

        active  = true;
        passive = false;

        Services.Lane.Deregister(this);
        Services.Lane.RegisterService(this);

        OnActive();
    }
    
    public void Passive() 
    {
        if (!enabled || !PassiveEnabled || passive) return;

        active  = false;
        passive = true;

        Services.Lane.Deregister(this);
        Services.Lane.RegisterPassive(this);

        OnPassive();
    }

    protected virtual void Absent()
    { 
        if (!enabled) return;
            
        Disable();
    }
        
    public void Disable()
    {
        if (!enabled) return;

        enabled = false;
        active  = false;
        passive = false;

        OnDisable();
    }

    // ===============================================================================

    protected virtual void OnEnable()   { }
    protected virtual void OnActive()   { }
    protected virtual void OnPassive()  { }
    protected virtual void OnAbsent()   { }
    protected virtual void OnRestart()  { }
    protected virtual void OnDisable()  { }
    protected virtual void OnDispose()  { }

    // ===============================================================================

    public virtual void Dispose()
    {
        Services.Lane.Deregister(this);
        OnDispose();
    }
    // ===============================================================================

    public bool PassiveEnabled  => this is IPassiveTick || this is IPassiveLoop || this is IPassiveStep || this is IPassiveUtil || this is IPassiveLate;
    public bool IsEnabled       => enabled;
    public bool IsActive        => active;
    public bool IsPassive       => passive;
    public bool IsDisabled      => !enabled;
}


public abstract class ActorService : Service
{
    protected readonly Actor           owner;
    protected readonly ActorDefinition definition;
    
    // ===============================================================================
    
    public ActorService(Actor owner)
    {
        this.owner      = owner;
        this.definition = owner.Definition;

        this.owner.Bus.Link.Local<PresenceStateEvent>(HandlePresenceStateEvent);

        Enable();
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    protected virtual void HandlePresenceStateEvent(PresenceStateEvent message)
    {
        switch (message.State)
        {
            case Presence.State.Resetting:  Restart();      break;
            case Presence.State.Entering:   Enable();       break;
            case Presence.State.Simulated:  Passive();      break;
            case Presence.State.Absent:     Absent();       break;
            case Presence.State.Disposal:   Dispose();      break;
        }
    }
}


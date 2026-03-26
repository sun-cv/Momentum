using System.Collections.Generic;



public class DamageResolver : RegisteredService, IServiceLoop
{
    
    readonly List<DamageContext> queue = new(); 

    // ===============================================================================

    public DamageResolver()
    {

        Link.Global<ResolveDamage>(HandleResolveDamageEvent);

        Services.Lane.Register(this);
    }

    // ===============================================================================

    public void Loop()
    {
        ProcessQueue();
    }

    // ===============================================================================

    void ProcessQueue()
    {
        foreach (var context in queue)
        {
            ProcessContext(context);
        }
    
        queue.Clear();
    }

    void ProcessContext(DamageContext context)
    {
        foreach (var component in context.Package.Components)
        {
            ProcessComponent(context, component);
        }
    }

    void ProcessComponent(DamageContext context, DamageComponent component)
    {
        var target = context.Target;
        var result = context.Package.Result.Components[component];

        SendResourceDamage(target, result);
        SendStatusEffects (target, component);
        
        if (WasKillingBlow(result))
            SendKillingBlow(target, context, component);
    
        DebugLog(target, context, result);
    }

    // ===============================================================================
    //  Events
    // ===============================================================================

    void HandleResolveDamageEvent(ResolveDamage message)
    {
        queue.Add(message.Context);
    }

    void SendResourceDamage(Actor target, ComponentResult result)
    {
        if (result.Shield > 0) target.Bus.Emit.Local(new Dissipate(result.Shield));
        if (result.Armor  > 0) target.Bus.Emit.Local(new Fracture (result.Armor ));
        if (result.Health > 0) target.Bus.Emit.Local(new Wound    (result.Health));
    }

    void SendStatusEffects(Actor target, DamageComponent component)
    {
        foreach (var status in component.StatusEffects)
        {
            // Send target and status to Status effect system > dot system
        }
    }

    void SendKillingBlow(Actor target, DamageContext context, DamageComponent component)
    {
        target.Bus.Emit.Local(new KillingBlow(context, component));
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================
  
    bool WasKillingBlow(ComponentResult result)
    {
        return result.BrokeHealth;
    }

    // ===============================================================================

    void DebugLog(Actor actor, DamageContext context, ComponentResult result)
    {

      Log.Trace("Resolver.Target",          () => actor.GetType().Name);
      Log.Trace("Resolver.Target.Shield",   () => actor is IShield instance ? instance.Shield : "");
      Log.Trace("Resolver.Target.Armor",    () => actor is IArmor  instance ? instance.Armor  : "");
      Log.Trace("Resolver.Target.Health",   () => actor is IMortal instance ? instance.Health : "");
      
      Log.Trace("Resolver.Damage.Shield",   () => result.Shield);
      Log.Trace("Resolver.Damage.Armor",    () => result.Armor);
      Log.Trace("Resolver.Damage.Health",   () => result.Health);
    }

    readonly Logger Log = Logging.For(LogSystem.Combat);

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public UpdatePriority Priority => ServiceUpdatePriority.DamageProcessor;

}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                         Events
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public readonly struct ResolveDamage    : IMessage
{
    public DamageContext Context            { get; init; }

    public ResolveDamage(DamageContext context)
    {
        Context = context; 
    }
}






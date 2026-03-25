// using System.Collections.Generic;



// public class DepDamageSystem : RegisteredService, IServiceStep
// {

//     readonly List<IMitigationProcessor> mitigations = new();

//         // -----------------------------------

//     readonly List<DamageContext> queue              = new();

//     // ===============================================================================

//     public DepDamageSystem()
//     {
//         Link.Global<Message<Request, DamageEvent>>(HandleDamageEvent);
//     }

//     // ===============================================================================
    
//     public void Step()
//     {
//         ProcessDamageQueue();
//     }

//     // ===============================================================================

//     void ProcessDamageQueue()
//     {
//         foreach (var context in queue)
//         {
//             ProcessDamageComponents(context, context.Package.Components);
//         }
//     }


//     void ProcessDamageComponents(DamageContext context, List<DamageComponent> components)
//     {
//         foreach (var component in components)
//         {
//             ProcessDamage(context, component);
//         }
//     }


//     void ProcessDamage(DamageContext context, DamageComponent component)
//     {
//         if (!IsDamageable(context.Target, out var mortal))
//             return;

//         CalculateMitigation(context);
//         NotifyActor(context);
//     }

//     void CalculateMitigation(DamageContext context)
//     {
//         foreach (var handler in mitigations)
//         {
//             handler.Process(context);
//         }
//     }

//     // ===============================================================================
//     //  Events
//     // ===============================================================================

//     void HandleDamageEvent(Message<Request, DamageEvent> message)
//     {
//         queue.Add(message.Payload.Context);
//     }

//     void NotifyActor(DamageContext context)
//     {
//         context.Target.Emit.Local(new DamageEvent(context));
//     }


//     // ===============================================================================
//     //  Predicates
//     // ===============================================================================

//     bool IsDamageable(Actor target, out IMortal actor)
//     {
//         if (target is IMortal mortal && !mortal.Invulnerable && !mortal.Impervious)
//         {
//             actor = mortal;
//             return true;
//         }

//         actor = null;
//         return false;
//     }

//     // ===============================================================================

//     Logger Log = new(LogSystem.Damage, LogLevel.Debug);

//     public override void Dispose()
//     {
//         Services.Lane.Deregister(this);
//     }

//     public UpdatePriority Priority => ServiceUpdatePriority.Damage;
// }

// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                                      Declarations
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//         //                                  Enums                                                 
//         // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// public enum DamageElement
// {
//     Fire,
//     Frost,
//     Shock,
//     Poison,
//     Physical,
//     Dynamic,
//     Explosion,
// }


// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                                         Events
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// public readonly struct Damage : IResourceAction
// {
//     public float Amount                     { get; init; }
//     public DamageElement Type                  { get; init; }

//     public Damage(float amount, DamageElement type)
//     {
//         Amount  = amount;
//         Type    = type;
//     }
// }


// public readonly struct DamageComponent
// {
//     public Damage Damage                    { get; init; }
//     public List<Effect> Effects             { get; init; }

//     public DamageComponent(Damage damage)
//     {
//         Damage          = damage;
//         Effects         = new();
//     }
// }

// public readonly struct DamagePackage
// {
//     public List<DamageComponent> Components { get; init; }

//     public DamagePackage(List<DamageComponent> components)
//     {
//         Components = components;
//     }
// }

// public class DamageContext
// {
//     public Actor Target                     { get; init; }
//     public Actor Source                     { get; init; }
//     public DamagePackage Package            { get; init; }

//     public DamageContext(Actor target, Actor source, DamagePackage package)
//     {
//         Target      = target;
//         Source      = source;
//         Package     = package;
//     }
//  }

// public readonly struct DamageEvent
// {
//     public DamageContext Context            { get; init; }

//     public DamageEvent(DamageContext context)
//     {
//         Context = context; 
//     }
// }



// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                                        Processor
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬


// public interface IMitigationProcessor
// {
//     float Process(DamageContext context);
// }


// public class ArmorMitigation : IMitigationProcessor
// {
//     public float Process(DamageContext context)
//     {
//         return 0f;
//     }
// }

// public class ResistanceMitigation : IMitigationProcessor
// {
//     public float Process(DamageContext context)
//     {
//         return 0f;
//     }
// }

// public class KillingBlow {}
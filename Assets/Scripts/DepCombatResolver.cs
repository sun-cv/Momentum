// using System.Collections.Generic;
// using UnityEngine;



// public class DepCombatResolver : RegisteredService, IServiceStep, IInitialize
// {
//     readonly List<DamageContext> queue = new();

//     // ===============================================================================

//     public void Initialize()
//     {
//         Link.Global<Message<Request, CombatEvent>>(HandleCombatEvent);
//     }
    
//     // ===============================================================================
    
//     public void Step()
//     {
//         ProcessQueuedEvents();
//     }

//     void ProcessQueuedEvents()
//     {
//         foreach (var context in queue)
//         {
//             ProcessCombatEvent(context);
//         }

//         queue.Clear();
//     }

//     void ProcessCombatEvent(DamageContext context)
//     {
//         var source      = context.Source;
//         var target      = context.Target;
//         var components  = context.Package.Components;

//         ProcessComponents(source, target, components);
//     }

//     // ===============================================================================

//     void ProcessComponents(Actor source, Actor target, List<DamageComponent> components)
//     {
//         foreach (var component in components)
//         {            
//             ProcessComponent(source, target, component);
//         }
//     }


//         // Rework required - Self targetting buffs on hit?
//     void ProcessComponent(Actor source, Actor target, DamageComponent component)
//     {
//         ApplyEffects(target, component.Effects);
//     }

//     // ===============================================================================
//     //  Effect Management
//     // ===============================================================================

//     public void ApplyEffects(Actor actor, List<Effect> effects)
//     {
//         foreach (var effect in effects)
//         {
//             actor.Emit.Local(Request.Create, effect);
//         }
//     }

//     // ===============================================================================
//     //  Events
//     // ===============================================================================
    
//     public void HandleCombatEvent(Message<Request, CombatEvent> message)
//     {
//         queue.Add(message.Payload.Context);
//     }

//     public void SendDamageEvent()
//     {
//         Emit.Global(Request.Create, new DamageEvent());
//     }

//     // ===============================================================================

//     readonly Logger Log = Logging.For(LogSystem.Combat);

//     public override void Dispose()
//     {   
//         Services.Lane.Deregister(this);
//     }

//     public UpdatePriority Priority => ServiceUpdatePriority.Combat;
// }


// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
// //                                         Events
// // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

// public readonly struct CombatEvent
// {
//     public DamageContext Context            { get; init; }

//     public CombatEvent(DamageContext context)
//     {
//         Context = context;
//     }
// }




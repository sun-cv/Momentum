// using System;
// using System.Collections.Generic;
// using UnityEngine;



// public class HitboxController : Controller
// {
//     public int              hitboxId;
//     public HitboxManager    manager;

//     private void OnTriggerEnter(Collider trigger)
//     {
//         if (!trigger.TryGetComponent<Character>(out var character))
//             return;

//         manager.DetectHit(new(){ HitboxId = hitboxId,  Target = character.owner });
//     }
// }


// public enum HitboxBehavior
// {
//     Attached,
//     Stationary,
//     Projectile
// }

// public class HitboxDefinition : Definition
// {
//     public string Prefab                { get; init; }
//     public Vector3 Offset               { get; init; }
//     public Quaternion Quaternion        { get; init; }
//     public int FrameStart               { get; init; }
//     public int FrameEnd                 { get; init; }
//     public bool AllowMultiHit           { get; init; }

//     public HitboxBehavior Behavior      { get; init; }

//     public float ProjectileSpeed        { get; init; }
//     public Vector3 ProjectileDirection  { get; init; }
// };

// public struct HitEvent
// {
//     public int HitboxId             { get; init; }
//     public Entity Owner             { get; init; }
//     public Entity Target            { get; init; }
// };


// public class HitboxInstance : Instance
// {
//     public int                  hitboxId;
//     public Entity               owner;
//     public WeaponAction         weapon;
//     public HitboxDefinition     definition;

//     public int                  currentFrame    = 0;
//     public GameObject           hitbox;
// }






// public class HitboxManager : RegisteredService
// {

//     readonly Dictionary<int, HitboxInstance> activeHitboxes;

//     int hitboxIds;

//     public override void Initialize()
//     {
//         EventBus<HitboxRequest>.Subscribe(HandleHitboxRequest);
//     }

//     HitboxInstance CreateInstance(Entity owner, HitboxDefinition definition, WeaponAction weapon = null)
//     {
//         HitboxInstance instance = new()
//         {
//             hitboxId    = hitboxIds++,
//             owner       = owner,
//             definition  = definition
//         };

//         if (weapon != null)
//             instance.weapon = weapon;

//         return instance;
//     }

//     void CreateHitbox(HitboxInstance instance)
//     {
//         // var prefab = Registry.Prefabs.Get(instance.definition.Prefab);

//         // Vector3 ownerPos = instance.owner.GetPosition();
//         // Quaternion ownerRot = instance.owner.GetRotation();

//         // Vector3 worldOffset = ownerRot * instance.definition.Offset;
//         // Vector3 spawnPos = ownerPos + worldOffset;
//         // Quaternion spawnRot = ownerRot * instance.definition.Quaternion;

//         // var hitbox = UnityEngine.Object.Instantiate(prefab, spawnPos, spawnRot);
//         // var controller = hitbox.GetComponent<HitboxController>();

//         // instance.hitbox = hitbox;
//         // controller.hitboxId = instance.hitboxId;
//         // controller.manager = this;

//         // // Handle behavior
//         // switch (instance.definition.Behavior)
//         // {
//         //     case HitboxBehavior.Attached:
//         //         // Parent to owner's transform
//         //         if (instance.owner is Hero hero)
//         //             hitbox.transform.SetParent(hero.Character.transform);
//         //         else if (instance.owner is Enemy enemy)
//         //             hitbox.transform.SetParent(enemy.Character.transform);
//         //         break;

//         //     case HitboxBehavior.Stationary:
//         //         // Do nothing - stays where spawned
//         //         break;

//         //     case HitboxBehavior.Projectile:
//         //         // Add projectile movement component
//         //         var projectile = hitbox.AddComponent<ProjectileMovement>();
//         //         projectile.Initialize(
//         //             instance.definition.ProjectileSpeed,
//         //             ownerRot * instance.definition.ProjectileDirection
//         //         );
//         //         break;
//         // }
//     }

//     public void SpawnHitbox()
//     {
        

//     }

//     public void DetectHit(HitEvent evt)
//     {
        
//     }

//     void HandleHitboxRequest(HitboxRequest evt)
//     {
//         var owner       = evt.Payload.Owner;
//         var definition  = evt.Payload.Definition;
//         var weapon      = evt.Payload.Weapon;

//         var instance    = CreateInstance(owner, definition, weapon);
        
//         CreateHitbox(instance);

//         activeHitboxes.Add(instance.hitboxId, instance);
//     }

// }




// public readonly struct HitboxRequestPayload
// {
//     public readonly Entity Owner                { get; init; }
//     public readonly HitboxDefinition Definition { get; init; }
//     public readonly WeaponAction Weapon         { get; init; }
// }

// public readonly struct HitboxRequest : IEvent
// {
//     public Guid Id                      { get; }
//     public Publish Action               { get; }
//     public HitboxRequestPayload Payload { get; }

//     public HitboxRequest(Guid id, Publish action, HitboxRequestPayload payload)
//     {
//         Id      = id;
//         Action  = action;
//         Payload = payload;
//     }
// }

// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
//
//
// public class FacingIntentDep : ActorService, IServiceTick
// {
//
//     readonly IntentSystem intent;
//
//         // -----------------------------------
//
//     readonly List<FacingAPI> queue                  = new();
//
//         // -----------------------------------
//
//     readonly List<Priority> tiers                   = new();
//     readonly Dictionary<Priority, FacingAPI> claims = new();
//
//         // -----------------------------------
//
//     readonly EffectCache locks;
//
//         // -----------------------------------
//
//     Direction facing                = new(Vector2.down);
//
//     DirectionMode mode              = DirectionMode.Live;
//     DirectionSource source          = DirectionSource.Direction;
//     DirectionConstraint constraint  = DirectionConstraint.Free;
//
//     IntentSnapshot snapshot;
//
//     bool hasClaim;
//
//    // ===============================================================================
//
//     public FacingIntentDep(IntentSystem intent) : base (intent.Owner)
//     {
//         this.intent  = intent;
//         locks   = new(owner.Bus,(effect) => effect is IDisableRotate);
//
//         CreateTiers();
//     }
//
//     // ===============================================================================
//
//     public void Tick()
//     {
//         ProcessClaims();
//         ResolveFacingSource();
//         ResolveFacingUpdate();
//     }
//
//     void ProcessClaims()
//     {
//         ProcessQueue();
//     }
//
//     void ProcessQueue()
//     {
//         foreach (var request in queue)
//         {
//             Process(request); 
//         }
//     }
//
//     void Process(FacingAPI message)
//     {
//         switch(message.Request)
//         {
//             case Request.Claim:     ClaimFacing(message);   break;
//             case Request.Release:   ReleaseFacing(message); break;
//         }
//     }
//
//     void ResolveFacingSource()
//     {
//         if (ResolveClaimedFacing())
//             return;
//
//         SetDefaultFacing();
//     }
//     
//     bool ResolveClaimedFacing()
//     {
//         if (claims.Count == 0)
//             return false;
//
//         foreach (var tier in tiers)
//         {
//             if (claims.TryGetValue(tier, out var claim) && claim.Constraint != DirectionConstraint.Locked)
//             {
//                 hasClaim    = true;
//
//                 source      = claim.Source;
//                 constraint  = claim.Constraint;
//                 snapshot    = claim.Snapshot;
//                 
//                 return true;
//             }
//         }
//         return false;
//     }
//
//     void SetDefaultFacing()
//     {
//         hasClaim    = false;
//         source      = DirectionSource.Direction;
//         constraint  = DirectionConstraint.Free;
//         snapshot    = default;
//     }
//
//     void ResolveFacingUpdate()
//     {
//         if (hasClaim)
//         {
//             SetFacing();
//             return;
//         }
//
//         if (FacingLocked())
//             return;
//         
//         SetFacing();
//         UpdateDefaultFacing();
//     }
//
//     void SetFacing()
//     {
//         switch(constraint)
//         {
//             case DirectionConstraint.Free: SelectFacingMode();  break;
//             case DirectionConstraint.Locked:                    break;
//         }
//     }
//
//     void SelectFacingMode()
//     {
//         switch(mode)
//         {
//             case DirectionMode.Live: SetIntentFacing();         break;
//             case DirectionMode.Snapshot: SetSnapshotFacing();   break;
//         }
//     }
//
//     void SetIntentFacing()
//     {
//         switch(source)
//         {
//             case DirectionSource.Aim:           facing = intent.Aiming.Aim;                 break;
//             case DirectionSource.Direction:     facing = intent.Direction.Direction;        break;
//             case DirectionSource.LastDirection: facing = intent.Direction.LastDirection;    break;
//         }
//     }
//
//     void SetSnapshotFacing()
//     {
//         switch(source)
//         {
//             case DirectionSource.Aim:           facing = snapshot.Aim;              break;
//             case DirectionSource.Direction:     facing = snapshot.Direction;        break;
//             case DirectionSource.LastDirection: facing = snapshot.LastDirection;    break;
//         }
//     }
//
//     bool FacingLocked()
//     {
//         if (locks.Count > 0)
//             return true;
//
//         if (constraint is DirectionConstraint.Locked)
//             return true;
//
//         return false;
//     }
//
//     void UpdateDefaultFacing()
//     {
//         if (facing.IsZero)
//             return;
//
//         if (!facing.IsCardinal)
//             return;
//
//         bool facingHorizontal = Mathf.Abs(facing.X) > Mathf.Abs(facing.Y);
//
//         if (facingHorizontal)
//         {
//             facing = new Vector2(facing.X > 0 ? 1 : -1, 0);
//             return;
//         }
//
//         if (!facingHorizontal)
//         {
//             if (intent.Direction.DiagonalTravel.Duration >= Orientation.GetTurnDelay(facing, intent.Direction))
//                 facing = new Vector2(facing.X > 0 ? 1 : -1, 0);
//         }
//     }
//
//     // ===============================================================================
//     //  Helpers
//     // ===============================================================================
//
//
//     void ClaimFacing(FacingAPI request)
//     {
//         claims[request.Priority] = request;
//     }
//
//     void ReleaseFacing(FacingAPI request)
//     {
//         if (!claims.TryGetValue(request.Priority, out var claim))
//             return;
//
//         if (request.Claimant != claim.Claimant)
//             return;
//
//         claims.Remove(request.Priority);
//     }
//
//     void CreateTiers()
//     {
//         foreach (Priority tier in Enum.GetValues(typeof(Priority)))
//             tiers.Add(tier);
//
//         tiers.Reverse();
//     }
//
//     void ClaimLockConstraint(FacingAPI request)
//     {
//         foreach (var tier in tiers)
//         {
//             if (claims.TryGetValue(tier, out var api) && api == request)
//             {
//                 SetFacing();
//                 return;
//             }
//         }
//     }
//
//     // ===============================================================================
//
//     public Direction Facing         => facing;
//     public UpdatePriority Priority  => ServiceUpdatePriority.FacingHandler;
// }
//
//
//
// public class FacingAPIDep : API
// {
//     public Guid Claimant                    { get; init; }
//     public Priority Priority                { get; init; }
//     public DirectionSource Source           { get; init; }
//     public DirectionConstraint Constraint   { get; init; }
//     public IntentSnapshot Snapshot          { get; init; }
//
//     public FacingAPI(Guid claimant, Priority priority, DirectionSource source, DirectionConstraint constraint, IntentSnapshot snapshot = new())
//     {
//         Claimant    = claimant;
//         Priority    = priority;
//         Source      = source;
//         Constraint  = constraint;
//         Snapshot    = snapshot;
//     }
// }
//
//
//

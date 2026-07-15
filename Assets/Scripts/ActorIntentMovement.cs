//
//
//
// using UnityEngine;
//
// public class MovementIntent : ActorService, IServiceLoop
// {
//     readonly IntentSystem intent;
//
//         // -----------------------------------
//
//     Direction facing                = new(Vector2.down);
//
//    // ===============================================================================
//
//     public MovementIntent(IntentSystem intent) : base(intent.Owner)
//     {
//         this.intent = intent;
//     }
//
//    // ===============================================================================
//
//     public void Loop()
//     {
//         UpdateDefaultFacing();
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
//             if (intent.Direction.DiagonalTravel.Duration >= Orientation.GetTurnDelay(facing, intent.Direction.Direction))
//                 facing = new Vector2(facing.X > 0 ? 1 : -1, 0);
//         }
//     }
//
//     public UpdatePriority Priority      => ServiceUpdatePriority.IntentSystem;
//
// }

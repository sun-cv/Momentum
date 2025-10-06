// using UnityEngine;

// namespace Momentum {
//     public class Move : State {
//         readonly PlayerContext ctx;

//         public Move(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
//             this.ctx = ctx;
//         }

//         protected override State GetTransition() {
//             if (!ctx.grounded) return ((PlayerRoot)Parent).Airborne;
            
//             return Mathf.Abs(ctx.move.x) <= 0.01f ? ((Grounded)Parent).Idle : null;
//         }

//         protected override void OnUpdate(float deltaTime) {
//             var target = ctx.move.x * ctx.moveSpeed;
//             ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, target, ctx.accel * deltaTime);
//         }
//     }
// }
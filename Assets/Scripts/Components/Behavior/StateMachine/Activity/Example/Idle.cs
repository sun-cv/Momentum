// using UnityEngine;

// namespace Momentum {
//     public class Idle : State {
//         readonly PlayerContext ctx;

//         public Idle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
//             this.ctx = ctx;
//         }

//         protected override State GetTransition() {
//             return Mathf.Abs(ctx.move.x) > 0.01f ? ((Grounded)Parent).Move : null;
//         }

//         protected override void OnEnter() {
//             ctx.velocity.x = 0f;
//         }
//     }
// }
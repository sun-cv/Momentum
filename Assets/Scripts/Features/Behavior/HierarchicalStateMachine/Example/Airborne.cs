// using UnityEngine;

// namespace Momentum {
//     public class Airborne : State {
//         readonly PlayerContext ctx;

//         public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
//             this.ctx = ctx;
//             Add(new ColorPhaseActivity(ctx.renderer){
//                 enterColor = Color.red, // runs while Airborne is activating
//             });
//         }
        
//         protected override State GetTransition() => ctx.grounded ? ((PlayerRoot)Parent).Grounded : null;

//         protected override void OnEnter() {
//             // TODO: Update Animator through ctx.anim
//         }
//     }
// }
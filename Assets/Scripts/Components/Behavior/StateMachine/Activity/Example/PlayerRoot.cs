// namespace Momentum {
//     public class PlayerRoot : State {
//         public readonly Grounded Grounded;
//         public readonly Airborne Airborne;
//         readonly PlayerContext ctx;

//         public PlayerRoot(StateMachine m, PlayerContext ctx) : base(m, null) {
//             this.ctx = ctx;
//             Grounded = new Grounded(m, this, ctx);
//             Airborne = new Airborne(m, this, ctx);
//         }
        
//         protected override State GetInitialState() => Grounded;
//         protected override State GetTransition() => ctx.grounded ? null : Airborne;
//     }
// }


namespace Momentum
{


    public class SprintState : HeroState, IAutomatic, IInterruptible
    {
        public SprintState(Hero hero) : base(hero) {}

        public override void Enter()
        {
            movement.mode   = MovementMode.Dynamic;
            movement.intent = MovementIntent.Move;

            animator.Play(HeroAnimation.Locomotion);
        }

        public override void Tick()
        {
            // noop
        }

        public override void Exit()
        {
            // noop
        }


    }
}
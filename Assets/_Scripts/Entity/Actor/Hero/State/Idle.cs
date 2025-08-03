
namespace Momentum
{

    public class IdleState : HeroState, IAutomatic, IInterruptible, ICancellable
    {
        public IdleState(Hero hero) : base(hero) {}

        public override void Enter()
        {
            movement.mode   = MovementMode.Dynamic;
            movement.intent = MovementIntent.Idle;

            movement.IdleTimer.Reset();
            movement.IdleTimer.Start();

            Animation();
        }

        public override void Animation()
        {
            animator.Play(HeroAnimation.Idle);
        }

        public override void Tick()
        {
            // noop
        }

        public override void Exit()
        {
            movement.IdleTimer.Stop();
        }

    }
}
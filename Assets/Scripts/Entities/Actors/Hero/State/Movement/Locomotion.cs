

namespace Momentum.HSM.Hero.Movement
{

    public class Locomotion : State 
    {

        public readonly Idle Idle;
        public readonly Sprint Sprint;

        public Locomotion(State state) : base(state)
        {
            Idle    = new(this);
            Sprint  = new(this);
        }

        protected override State GetInitialState()
        {
            return Idle;
        }

        protected override State GetTransition()
        {
            // Movement Vector = 0 > Idle
            // Movement Vector != 0 > Sprint;

            return null;
        }

        protected override void OnEnter()
        {

        }

        protected override void OnUpdate(float deltaTime)
        {

        }

        protected override void OnExit()
        {
        
        }


    }


}
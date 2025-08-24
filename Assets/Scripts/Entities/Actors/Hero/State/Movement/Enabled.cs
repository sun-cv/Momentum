

namespace Momentum.HSM.Hero.Movement
{

    public class Enabled : State 
    {

        public readonly Locomotion Locomotion;
        public readonly Dash Dash;

        public Enabled(State state) : base(state)
        {
            Locomotion  = new(this);
            Dash        = new(this);

        }

        protected override State GetInitialState()
        {
            return Locomotion;
        }

        protected override State GetTransition()
        {

            // Dash Command > Dash;

            return Locomotion;
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
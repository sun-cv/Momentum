


namespace Momentum.HSM.Hero.Movement
{

    public class Dash : State 
    {

        public Dash(State state) : base(state)
        {

        }


        protected override State GetTransition()
        {
            return ((Enabled)Parent).Locomotion;
        }

        protected override void OnEnter()
        {
            // Add movement activity?
            // Add();
        }

        protected override void OnUpdate(float deltaTime)
        {

        }

        protected override void OnExit()
        {
        
        }


    }


}
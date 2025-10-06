


namespace Momentum.HSM.Hero.Movement
{

    public class Dash : State 
    {

        public Dash(State state, Context context) : base(state, context) {}



        protected override State GetTransition()
        {
            return ((Enabled)Parent).Locomotion;
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
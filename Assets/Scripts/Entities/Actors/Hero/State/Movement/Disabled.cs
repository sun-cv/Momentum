

namespace Momentum.HSM.Hero.Movement
{

    public class Disabled : State 
    {

        public Disabled(State state, Context context) : base(state, context) {}


        protected override State GetInitialState()
        {
           return null;
        }

        protected override State GetTransition()
        {
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
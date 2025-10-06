

namespace Momentum.HSM.Hero.Movement
{

    public class Enabled : State 
    {

        public readonly Locomotion Locomotion;

        public Enabled(State state, Context context) : base(state, context)
        {
            Locomotion  = new(this, context);
        }

        protected override State GetInitialState()
        {
            return Locomotion;
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
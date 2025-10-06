

namespace Momentum.HSM.Hero.Behavior
{

    public class Disabled : State 
    {

        public StatusEffect StatusEffect;

        public Disabled(State state, Context context) : base(state, context) 
        {
            StatusEffect = new(this, context);
        }


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
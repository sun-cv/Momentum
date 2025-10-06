

namespace Momentum.HSM.Hero.Movement
{

    public class RootMovement : State 
    {
        
        public readonly Enabled Enabled;
        public readonly Disabled Disabled;


        public RootMovement(Context context) : base(null, context)
        {
            Enabled  = new(this, context);
            Disabled = new(this, context);
        }


        protected override State GetInitialState()
        {
            return Enabled;
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
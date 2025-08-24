

using System.Buffers.Text;

namespace Momentum.HSM.Hero.Movement
{

    public class RootMovement : State 
    {
        
        public readonly Enabled enabled;
        public readonly Disabled disabled;


        public RootMovement() : base()
        {
            enabled  = new(this);
            disabled = new(this);
        }


        protected override State GetInitialState()
        {
            return enabled;
        }

        protected override State GetTransition()
        {
            // Status == disabled ?? Disabled : Enabled;
            // REWORK REQUIRED implement status component and read context?
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



namespace Momentum.HSM.Hero.Combat
{

    public class RootCombat : State 
    {
        
        public readonly Enabled enabled;
        public readonly Disabled disabled;


        public RootCombat() : base()
        {
            enabled = new(this);
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
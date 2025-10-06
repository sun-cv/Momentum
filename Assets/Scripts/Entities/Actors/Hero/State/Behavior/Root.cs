


using UnityEngine;

namespace Momentum.HSM.Hero.Behavior
{

    public class RootBehavior : State 
    {
        public Active Active;
        public Inactive Inactive;

        public bool active = true;

        public RootBehavior(Context context) : base(null, context)
        {
            Active   = new(this, context);
            Inactive = new(this, context);
        }

        protected override State GetInitialState()
        {
            return Active;
        }

        protected override State GetTransition()
        {
            return active ? null : Inactive;
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
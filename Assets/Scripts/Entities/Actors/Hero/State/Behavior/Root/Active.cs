using Unity.VisualScripting;
using UnityEngine;

namespace Momentum.HSM.Hero.Behavior
{

    public class Active : State 
    {
        public Alive Alive;
        public Dead  Dead;

        public bool alive = true;

        public Active(State state, Context context) : base(state, context) 
        {
            Alive = new(this, context);
            Dead  = new(this, context);
        }

        protected override State GetInitialState()
        {
            return Alive;
        }

        protected override State GetTransition()
        {
            return alive ? null : Dead;
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
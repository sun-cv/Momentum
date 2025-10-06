


using UnityEngine;

namespace Momentum.HSM.Hero.Movement
{

    public class Idle : State 
    {

        public Idle(State state, Context context) : base(state, context) {}

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
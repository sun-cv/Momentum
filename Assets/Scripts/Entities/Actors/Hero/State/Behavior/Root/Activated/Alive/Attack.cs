


using Unity.VisualScripting;
using UnityEngine;

namespace Momentum.HSM.Hero.Behavior
{

    public class Attack : State 
    {

        public Attack(State state, Context context) : base(state, context) 
        {
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
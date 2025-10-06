


using Unity.VisualScripting;
using UnityEngine;

namespace Momentum.HSM.Hero.Behavior
{

    public class Idle : State 
    {

        public Idle(State state, Context context) : base(state, context) 
        {
        }

        protected override State GetTransition()
        {
            return null;
        }

        protected override void OnEnter()
        {
            // Debug.Log("Enter Idle");
        }

        protected override void OnUpdate(float deltaTime)
        {
        }

        protected override void OnExit()
        {
        }


    }


}
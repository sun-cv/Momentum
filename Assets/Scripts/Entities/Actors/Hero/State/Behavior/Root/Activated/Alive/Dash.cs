


using Unity.VisualScripting;
using UnityEngine;

namespace Momentum.HSM.Hero.Behavior
{

    public class Dash : State 
    {

        // public IAbilitySystem ability;


        public Dash(State state, Context context) : base(state, context) 
        {
            // ability     = component.ability.System;
        }

        protected override State GetTransition()
        {
            return null;
        }

        protected override void OnEnter()
        {
            // ability.Cast(AbilityState.Dash);
        }

        protected override void OnUpdate(float deltaTime)
        {
        }

        protected override void OnExit()
        {
        }


    }


}
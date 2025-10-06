using Unity.VisualScripting;
using UnityEngine;

namespace Momentum.HSM.Hero.Behavior
{

    public class Alive : State 
    {
        public IAbilitySystem ability;

        public Disabled Disabled;
        public Idle Idle;
        public Cast Cast;
        public Dash Dash;
        public Attack Attack;
        public ShieldHeld ShieldHeld;

        public bool alive = true;
        public bool disabled = false;

        public Alive(State state, Context context) : base(state, context) 
        {
            ability     = component.ability.System;
            Disabled    = new(this, context);   
            Idle        = new(this, context);
            Cast        = new(this, context);
            Dash        = new(this, context);
            Attack      = new(this, context);
            ShieldHeld  = new(this, context);
        }

        protected override State GetTransition()
        {
            if (disabled)
                return Disabled;

            if (ability.HasEngaged(AbilityState.Dash)) 
                return Dash;

            if (ability.HasEngaged(AbilityState.ShieldHeld)) 
                return ShieldHeld;

            if (ability.HasEngaged(AbilityState.Attack)) 
                return Attack;

            if (ability.CastRequested)
                return Cast;
            
            if (ActiveChild != Idle)
                return Idle;

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
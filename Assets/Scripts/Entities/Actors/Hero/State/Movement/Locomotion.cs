

using UnityEngine;

namespace Momentum.HSM.Hero.Movement
{

    public class Locomotion : State 
    {

        public readonly Idle Idle;
        public readonly Sprint Sprint;

        public Locomotion(State state, Context context) : base(state, context)
        {
            Idle    = new(this, context);
            Sprint  = new(this, context);
        }

        protected override State GetInitialState()
        {
            return Idle;
        }

        protected override State GetTransition()
        {
            if (entity.movement.idle)
                return Idle;
            
            if(entity.movement.sprint)
                return Sprint;

            return Idle;
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
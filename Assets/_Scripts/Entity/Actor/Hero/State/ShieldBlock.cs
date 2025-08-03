using System;

namespace Momentum
{


    public class ShieldBlockState : HeroState, IManual
    {
        public HeroContext.Action.ShieldBlock   action;
        public float duration;

        public ShieldBlockState(Hero hero) : base(hero) 
        {
            action = hero.context.action.shieldBlock;
        }

        public override void Enter()
        {
            
        }

        public override void Tick()
        {

        }


        public override void Exit()
        {
        }
    }
}
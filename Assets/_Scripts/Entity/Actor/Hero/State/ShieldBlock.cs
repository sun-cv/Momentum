using System;
using Momentum.State;
using Momentum.Timers;
using UnityEngine;

namespace Momentum.Actor.Hero
{


    public class ShieldBlockState : BaseState, ICommandState
    {
        public HeroContext.Action.ShieldBlock   action;
        public float duration;

        public ShieldBlockState(Hero hero) : base(hero) 
        {
            action = hero.context.action.shieldBlock;
        }

        public virtual void SetCallback(Action callback)
        {
            commandCallback = callback;
        }


        public override void Enter()
        {
        }

        public override void TickFixed()
        {

        }

        public void OnComplete()
        {
            commandCallback.Invoke();
        }

        public override void Exit()
        {
        }
    }
}
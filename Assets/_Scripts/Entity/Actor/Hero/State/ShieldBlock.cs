using System;
using Momentum.State;
using Momentum.Timers;
using UnityEngine;

namespace Momentum.Actor.Hero
{


    public class ShieldBlockState : BaseState, IStateCommand
    {
        public HeroContext.Action.ShieldBlock   action;
        public float duration;

        public ShieldBlockState(Hero hero) : base(hero) 
        {
            action = hero.context.action.shieldBlock;
        }

        public virtual void SetCallback(Action callback)
        {
            OnComplete = callback;
        }


        public override void Enter()
        {
        }

        public override void TickFixed()
        {

        }

        public override void SignalComplete()
        {
            OnComplete.Invoke();
        }

        public override void Exit()
        {
        }
    }
}
using System;
using Momentum.State;
using Mono.Cecil.Cil;
using UnityEngine;


namespace Momentum.Actor.Hero
{

    public class IdleState : BaseState, IStateAutomatic, IInterruptible
    {
        public IdleState(Hero hero) : base(hero) {}
        
        public override void Enter()
        {
            movement.IdleTimer.Reset();
            movement.IdleTimer.Start();
            
            animator.Play(HeroAnimation.Idle);
        }


        public override void Exit()
        {
            movement.IdleTimer.Stop();
        }

    }
}
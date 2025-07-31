using System;
using Momentum.State;
using Mono.Cecil.Cil;
using UnityEngine;


namespace Momentum.Actor.Hero
{

    public class IdleState : BaseState, ILocomotionState
    {
        public IdleState(Hero hero) : base(hero) {}
        
        public override void Enter()
        {
            movement.IdleTimer.Start();
            animator.Play(HeroAnimation.Idle);

            Debug.Log($"Auto behavior test: {context.state.idle.Value}");
        }


        public override void Exit()
        {
            movement.IdleTimer.Stop();
            movement.IdleTimer.Reset();
        }

    }
}
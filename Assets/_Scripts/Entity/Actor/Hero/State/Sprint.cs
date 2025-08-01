using System;
using Momentum.State;
using UnityEngine;

namespace Momentum.Actor.Hero
{


    public class SprintState : BaseState, IStateAutomatic, IInterruptible
    {
        public SprintState(Hero hero) : base(hero) {}

        public override void Enter()
        {
            animator.Play(HeroAnimation.Locomotion, out  var duration);
        }

        public override void Exit()
        {
            // noop
        }


    }
}
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Momentum
{


    public class BasicAttackState : HeroState, IManual, IInterruptible, ICancellable
    {
        public HeroContext.Action.BasicAttack action;

        public BasicAttackState(Hero hero) : base(hero) 
        {
            action = hero.context.action.basicAttack;


        }

        public override void BindResult(Action<Result, TransitionMode> report)
        {
            result = report;
        }

        public override void Enter()
        {
            movement.mode   = MovementMode.Impulse;
            movement.intent = MovementIntent.Attack;

            progress = new Progress(attribute.attack.duration);

            action.attackCount += 1;

            movement.impulseDirection   = movement.lastDirection == Vector2.zero ? movement.defaultDirection : movement.lastDirection;
            movement.force              = attribute.attack.force;

            movement.impulseRequest.Set();
            // Animation();     
            progress.Start();       
        }

        public override void Animation()
        {
            animator.Play(HeroAnimation.BasicAttack);
        }

        public override void Tick()
        {
            UpdateContext();
            if (ExitConditionMet()) Exit();
        }

        public override void UpdateContext()
        {
            movement.progress = progress.Percent;
        }

        public override bool ExitConditionMet()
        {
            return progress.IsFinished;
        }

        public override void Exit()
        {
            movement.mode = MovementMode.Dynamic;

            CooldownService.Add(Cooldown.Create<AttackIntervalCooldown>(context));

            ReportResult(Result.Success, TransitionMode.Automatic);
        }

        public override void Cancel()
        {
            movement.mode = MovementMode.Dynamic;
            
            CooldownService.Add(Cooldown.Create<AttackIntervalCooldown>(context));

            ReportResult(Result.Cancelled, TransitionMode.Cancelled);
        }
    }
}
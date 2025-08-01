using System;
using Momentum.State;
using Momentum.Timers;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

namespace Momentum.Actor.Hero
{


    public class BasicAttackState : BaseState, IStateCommand, IInterruptible
    {
        public HeroContext.Action.BasicAttack action;
        public float duration;

        public BasicAttackState(Hero hero) : base(hero) 
        {
            action = hero.context.action.basicAttack;
        }

        public override void SetOnComplete(Action callback)
        {
            OnComplete = callback;
        }


        public override void Enter()
        {
            state.basicAttack.Set();
            
            action.direction        = movement.principal;  
            action.distance         = attribute.attack.force * attribute.attack.duration;
        
            action.startPosition    = context.transform.position;
            action.targetPosition   = action.startPosition + action.direction * action.distance;

            action.attackIntervalTimer              = new Stopwatch();
            action.attackCooldownTimer              = new Countdown(attribute.attack.attackCooldown);
            action.attackComboCooldownTimer         = new Countdown(attribute.attack.attackComboCooldown);

            action.attackIntervalTimer.OnTimerStart         += () => action.velocity = attribute.attack.force;
            action.attackIntervalTimer.OnTimerStop          += () => action.attackCooldownTimer.Start();

            action.attackCooldownTimer.OnTimerStart         += () => action.attackCooldown.Set();
            action.attackCooldownTimer.OnTimerStop          += () => action.attackCooldown.Clear();

            action.attackComboCooldownTimer.OnTimerStart    += () => { action.attackComboCooldown.Set();};
            action.attackComboCooldownTimer.OnTimerStop     += () => { action.attackComboCooldown.Clear(); action.attackCount = 0; };

            action.attackIntervalTimer.Start();
            
            action.attackCount += 1;

            if (action.attackCount == attribute.attack.attackCount)
            {
                action.attackComboCooldownTimer     = new Countdown(attribute.attack.attackComboCooldown);
                action.attackComboCooldownTimer.Start();
            }

            animator.Play(HeroAnimation.BasicAttack, out duration);

            Debug.Log("Calling enter");
        }

        public override void TickFixed()
        {
            float attackProgress    = Mathf.Clamp01(action.attackIntervalTimer.CurrentTime / duration);
            Vector2 newPos          = Vector2.Lerp(action.startPosition, action.targetPosition, attackProgress);

            context.transform.position = newPos;

            if (attackProgress >= 1f)
            {
                SignalComplete();
            }

        }

        public override void SignalComplete()
        {
            OnComplete.Invoke();
        }

        public override void Exit()
        {
            action.attackIntervalTimer.Stop();
            state.basicAttack.Clear();
        }
    }
}
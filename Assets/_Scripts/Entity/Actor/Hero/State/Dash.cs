using System;
using Momentum.Interface;
using Momentum.State;
using Momentum.Timers;
using UnityEngine;


namespace Momentum.Actor.Hero
{


    public class DashState : BaseState, IStateCommand
    {
        public HeroContext.Action.Dash  action;

        public DashState(Hero hero) : base(hero) 
        {
            action = hero.context.action.dash;
        }

        public override void SetOnComplete(Action callback)
        {
            OnComplete = callback;
        }

        public override void Enter()
        {
            action.direction          = movement.direction.normalized;

            action.distance           = attribute.dash.force * attribute.dash.duration;
            action.startPosition      = context.transform.position;
            action.targetPosition     = action.startPosition + action.direction * action.distance;

            action.dashTimer                      = new Stopwatch();
            action.dashCooldownTimer              = new Countdown(attribute.dash.dashCooldown);

            action.dashTimer.OnTimerStart         += () => action.velocity = attribute.dash.force;
            action.dashTimer.OnTimerStop          += () => action.dashCooldownTimer.Start();

            action.dashCooldownTimer.OnTimerStart += () => action.dashCooldown.Set();
            action.dashCooldownTimer.OnTimerStop  += () => action.dashCooldown.Clear();

            action.dashTimer.Start();
        }

        public override void TickFixed()
        {

            float dashProgress  = Mathf.Clamp01(action.dashTimer.CurrentTime / attribute.dash.duration);
            Vector2 newPos      = Vector2.Lerp(action.startPosition, action.targetPosition, dashProgress);
    
            context.transform.position = newPos;

            if (dashProgress >= 1f)
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
            action.dashTimer.Stop();

            action.direction = Vector2.zero;
            action.velocity  = action.defaultVelocity;

        }

    }
}


// namespace Momentum.Actor.Player.State.Movement
// {


//     public class DashMovementState1 : BaseMovementState
//     {
//         public DashMovementState1(Hero controller) : base(controller) {}

//     float dashDistance;
//     Vector2 dashStartPos;
//     Vector2 dashTargetPos;
//     float dashElapsed;

//     public override void Enter()
//     {
//         dash.active.Set();

//         dash.direction = movement.direction.normalized;

//         dashDistance = stats.dash.force * stats.dash.duration;
//         dashStartPos = context.transform.position;
//         dashTargetPos = dashStartPos + dash.direction * dashDistance;
//         dashElapsed = 0f;
//     }

//     public override void TickFixed()
//     {
//         dashElapsed += Time.fixedDeltaTime;
//         float t = Mathf.Clamp01(dashElapsed / stats.dash.duration);
//         Vector2 newPos = Vector2.Lerp(dashStartPos, dashTargetPos, t);
//         context.transform.position = newPos;

//         if (t >= 1f)
//         {
//             dash.active.Clear();
//             // Dash finished
//         }
//     }

//     public override void Exit()
//     {
//         dash.direction = Vector2.zero;
//     }

//     }
// }
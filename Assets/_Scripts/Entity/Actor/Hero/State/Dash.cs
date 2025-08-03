using System;
using UnityEngine;


namespace Momentum
{


    public class DashState : HeroState, IManual
    {
        public HeroContext.Action.Dash  action;

        public DashState(Hero hero) : base(hero) 
        {
            action              = hero.context.action.dash;
        }

        public override void BindResult(Action<Result, TransitionMode> report)
        {
            result = report;
        }

        public override void Enter()
        {
            movement.mode       = MovementMode.Kinematic;
            movement.intent     = MovementIntent.Dash;

            progress            = new Progress(attribute.dash.duration);

            movement.lockedDirection    = movement.lastDirection;

            movement.distance           = attribute.dash.force * attribute.dash.duration;
            movement.startPosition      = context.transform.position;
            movement.targetPosition     = movement.startPosition + movement.lockedDirection * movement.distance;

            progress.Start();
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

            CooldownService.Add(Cooldown.Create<DashCooldown>(context));

            ReportResult(Result.Success, TransitionMode.Automatic);
        }

    }
}

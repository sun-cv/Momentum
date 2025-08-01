using System;
using UnityEngine;
using Momentum.Interface;
using Momentum.State;


namespace Momentum.Actor.Hero
{
    public abstract class HeroCommand : ICommand
    {
        
        public abstract void Execute(IStateMachineController stateMachine, Action onComplete);
        public abstract float RequestedTime { get; }
        public abstract CommandType Type    { get; }

        public static T Create<T>() where T : HeroCommand
        {
            return (T)Activator.CreateInstance(typeof(T));
        }
    }

    public class DashCommand : HeroCommand, ICommandBufferable
    {

        public override float RequestedTime { get; }
        public override CommandType Type => CommandType.Dash;

        public DashCommand()
        {
            RequestedTime = Time.time;
        }

        public override void Execute(IStateMachineController stateMachine, Action onComplete)
        {
            stateMachine.CommandState<DashState>(onComplete);
        }

    }

    public class BasicAttackCommand : HeroCommand, ICommandBufferable
    {

        public override float RequestedTime { get; }
        public override CommandType Type    { get; }

        public BasicAttackCommand()
        {
            RequestedTime = Time.time;
        }

        public override void Execute(IStateMachineController stateMachine, Action onComplete)
        {
            stateMachine.CommandState<BasicAttackState>(onComplete);
        }
    }


}
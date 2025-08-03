using System;
using UnityEngine;

namespace Momentum
{

    

    public class Command : ICommand
    {
        protected IStateMachineController stateMachine;

        public virtual CommandType Type     { get; }
        public virtual Priority Priority    { get; }
        public virtual Result Result        { get; }

        public virtual float TimeRequested  { get; }
        public virtual float TimeExecuted   { get; set; }

        public virtual float ExpirePeriod   { get; }
        public virtual float BufferPeriod   { get; }
        public virtual float TimeBuffered   { get; set; }

        public virtual bool CanCancel()     { return false; }
        public virtual void RequestCancel() {}
        public virtual void Buffer()        {}
        public virtual void Initialize(IStateMachineController stateMachine, Action<Result> report)
        {
            this.reportResult = report;
            this.stateMachine = stateMachine;
        }

        public Action<Result> reportResult;

        public virtual void Execute() {}

        public static T Create<T>() where T : Command
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

    }

    public class DashCommand : Command, IBufferable
    {
        public override CommandType Type        { get; } = CommandType.Dash;
        public override Priority Priority       { get; } = Priority.High;

        public override float TimeRequested     { get; } = Time.time;
        public override float TimeExecuted      { get; set; }

        public override float ExpirePeriod      { get;} = .5f;
        public override float BufferPeriod      { get;} = .3f;
        public override float TimeBuffered      { get; set; }

        public override Result Result           { get; }

        public override void Execute()
        {
            stateMachine.CommandState<DashState>(reportResult);
        }
        
        public override void Buffer()
        {
            TimeBuffered = Time.time;
        }
    }

    public class BasicAttackCommand : Command, IBufferable, ICancellable
    {
        public override CommandType Type        { get; } = CommandType.BasicAttack;
        public override Priority Priority       { get; } = Priority.Low;

        public float          TimeMinimum       { get; } = 1f;

        public override float TimeRequested     { get; } = Time.time;
        public override float TimeExecuted      { get; set;}

        public override float ExpirePeriod      { get;} = .5f;
        public override float BufferPeriod      { get;} = .3f;

        public override Result Result           { get; }

        public override void Execute()
        {
            stateMachine.CommandState<BasicAttackState>(reportResult);
        }

        public override bool CanCancel()
        {
            return Time.time >= TimeExecuted + TimeMinimum; 
        }

        public override void RequestCancel()
        {
            stateMachine.CancelState();
        }

        public override void Buffer()
        {
            TimeBuffered = Time.time;
        }
    }


}
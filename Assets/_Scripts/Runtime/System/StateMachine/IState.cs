using System;
using NUnit.Framework.Internal;


namespace Momentum
{

    public interface IState {}

    public abstract class State : IState
    {
        public abstract void Enter();    
        public abstract void Exit(); 
        public abstract void Tick();

        public virtual  void Cancel() {}
        public virtual  void Interrupt() {}
        public virtual  void TickFixed() {}
        public virtual  void Condition() {}

        public virtual  void Audio() {}
        public virtual  void Animation() {}
        public virtual  void BindResult(Action<Result, TransitionMode> action) {}
        public virtual  void ReportResult(Result result, TransitionMode mode) {}
        public virtual  void UpdateContext() {}
        public virtual  void UpdateCondition() {}
        public virtual  bool ExitConditionMet() { return false;}

    }


}
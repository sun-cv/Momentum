using System;


namespace Momentum
{

    public interface IStateMachineController
    {
        public void CommandState<T>(Action<Result> callback) where T : State;
        public void InterruptState<T>() where T : IDisruption; 
        public void CancelState();
    }
}
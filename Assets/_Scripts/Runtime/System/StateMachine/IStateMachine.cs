using System;
using Momentum.Interface;


namespace Momentum.State
{

    public interface IStateMachineController
    {
        public void CommandState<T>(Action callback) where T : IState;
    }

}
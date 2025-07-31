using System;
using Momentum.Interface;


namespace Momentum.State
{

    public interface IStateMachineController
    {
        public void ChangeStateCommand<T>(Action callback) where T : IState;
    }

}
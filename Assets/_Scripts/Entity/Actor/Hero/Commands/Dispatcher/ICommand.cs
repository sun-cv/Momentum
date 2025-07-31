using System;
using System.Threading.Tasks;
using Momentum.Actor.Hero;
using Momentum.State;


namespace Momentum.Interface
{

    public interface ICommand
    {
        CommandType Type    { get; }
        float RequestedTime { get; }
        
        void Execute(IStateMachineController stateMachine, Action value);
    }

    public interface ICommandQueue 
    {
        public void Enqueue(ICommand command); 
    }
    
    public interface ICommandBufferable {}

    public interface ICommandDispatcher
    {
        void Enqueue (ICommand command);
    }

    public interface ICommandValidator
    {
        bool CanExecute(HeroContext context);
    }

}
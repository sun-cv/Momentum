using System;
using Unity.VisualScripting;


namespace Momentum
{

    public enum CommandType
    {
        Dash,
        BasicAttack,
    }

    public interface ICommand
    {
        void Execute();
    }

    public interface ICommandQueue 
    {
        public void Enqueue(Command command); 
    }

    public interface ICommandDispatcher
    {
        void Enqueue (Command command);
    }

    public interface ICommandValidator
    {
        bool CanExecute(IValidatorService service);
    }

    public interface ICommandCooldownValidator
    {
        bool CanExecute(ICooldownHandler handler);
    }

}
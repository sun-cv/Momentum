namespace Momentum.State
{
    public interface ITransition
    {
        IState To               { get; }
        IPredicate Condition    { get; }
    }

}
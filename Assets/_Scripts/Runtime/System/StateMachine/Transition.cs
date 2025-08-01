using System;


namespace Momentum.State
{

public abstract class Transition 
{
    public IState To { get; protected set; }
    public abstract bool Evaluate();
}

public class Transition<T> : Transition 
{
    public readonly T condition;

    public Transition(IState to, T condition) 
    {
        To = to;
        this.condition = condition;
    }
    public override bool Evaluate() 
    {
        var result = (condition as Func<bool>)?.Invoke();

        if (result.HasValue) 
        {
            return result.Value;
        }
        
        result = (condition as ActionPredicate)?.Evaluate();

        if (result.HasValue) 
        {
            return result.Value;
        }
        
        result = (condition as IPredicate)?.Evaluate();
        
        if (result.HasValue) 
        {
            return result.Value;
        }
        return false;
    }
}

}


using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Momentum.State
{
    public interface IPredicate
    {
        bool Evaluate();
    }

    public class FunctionPredicate : IPredicate
    {
        readonly Func<bool> function;

        FunctionPredicate(Func<bool> function)
        {
            this.function = function;
        }
        
        public bool Evaluate() => function.Invoke();
    }


    public class ActionPredicate : IPredicate 
    {
        public bool flag;
        public ActionPredicate(ref Action eventReaction)
        {
            eventReaction += () => { flag = true; };
        }

        public bool Evaluate() 
        {
            bool result = flag;
            flag        = false;

            return result;
        }
    }

    
    public class And : IPredicate 
    {
        [SerializeField] List<IPredicate> rules = new List<IPredicate>();
        public bool Evaluate() => rules.All(r => r.Evaluate());
    }

    public class Or : IPredicate 
    {
        [SerializeField] List<IPredicate> rules = new List<IPredicate>();
        public bool Evaluate() => rules.Any(r => r.Evaluate());
    }

    public class Not : IPredicate 
    {
        [SerializeField] IPredicate rule;
        public bool Evaluate() => !rule.Evaluate();
    }

}
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Momentum
{
    public interface IPredicate
    {
        bool Evaluate();
    }

    public class And : IPredicate 
    {
        private readonly List<IPredicate> rules;

        public And(params IPredicate[] rules)
        {
            this.rules = new List<IPredicate>(rules);
        }

        public bool Evaluate() => rules.All(rule => rule.Evaluate());
    }

    public class Or : IPredicate 
    {
        private readonly List<IPredicate> rules;

        public Or(params IPredicate[] rules)
        {
            this.rules = new List<IPredicate>(rules);
        }

        public bool Evaluate() => rules.Any(r => r.Evaluate());
    }

    public class Not : IPredicate 
    {
        private readonly IPredicate rule;

        public Not(IPredicate rule)
        {
            this.rule = rule;
        }

        public bool Evaluate() => !rule.Evaluate();
    }


    public abstract class BasePredicate : IPredicate
    {
        public T Access<T>() => Predicate.Resolve<T>();
    
        public bool Evaluate() => EvaluateInternal();
    
        protected abstract bool EvaluateInternal();
    }
    
    public abstract class CachedPredicate : IPredicate
    {
        private bool? cachedResult;
        private int lastFrameEvaluated;

        public T Access<T>() { return Predicate.Resolve<T>(); }

        public bool Evaluate()
        {
            int currentFrame = Time.frameCount;

            if (lastFrameEvaluated != currentFrame)
            {
                cachedResult        = EvaluateInternal();
                lastFrameEvaluated  = currentFrame;
            }

            return cachedResult.Value;
        }

        protected abstract bool EvaluateInternal();
    }
    
    

    public class FunctionPredicate : BasePredicate
    {
        private readonly Func<bool> function;

        public FunctionPredicate(Func<bool> function)
        {
            this.function = function;
        }

        protected override bool EvaluateInternal() => function.Invoke();
    }


    public class ActionPredicate : BasePredicate
    {
        private bool flag;

        public ActionPredicate(ref Action eventReaction)
        {
            eventReaction += () => flag = true;
        }

        protected override bool EvaluateInternal()
        {
            bool result = flag;
            flag        = false;
            return result;
        }
    }


    // public class CooldownPredicate : BasePredicate
    // {
    //     private readonly Func<ICooldownSystem, bool> onCooldown;

    //     public CooldownPredicate(Func<ICooldownSystem, bool> onCooldown) => this.onCooldown = onCooldown;

    //     protected override bool EvaluateInternal()
    //     {
    //         return onCooldown(Access<ICooldownSystem>());
    //     }
    // }


    // public class CooldownComboPredicate : BasePredicate
    // {
    //     private readonly Func<ICooldownSystem, bool> onCooldown;

    //     public CooldownComboPredicate(Func<ICooldownSystem, bool> onCooldown) 
    //         => this.onCooldown = onCooldown;

    //     protected override bool EvaluateInternal()
    //     {
    //         return onCooldown(Access<ICooldownSystem>());
    //     }
    // }


    // public class ResourcePredicate : BasePredicate
    // {
    //     private readonly Func<Context, float> resourceAvailable;
    //     private readonly float required;

    //     public ResourcePredicate(Func<Context, float> resourceAvailable, float required)
    //     {
    //         this.resourceAvailable = resourceAvailable;
    //         this.required = required;
    //     }

    //     protected override bool EvaluateInternal()
    //     {
    //         return resourceAvailable(Access<Context>()) >= required;
    //     }
    // }

}
using System;

namespace Momentum
{
    [Serializable]
    public abstract class AbilityPredicate
    {
        public abstract bool IsMet(Context context);
    }



    // [Serializable]
    // public class StateTransitionRequirement : AbilityPredicate
    // {
    //     [Header("")]


    // }

}
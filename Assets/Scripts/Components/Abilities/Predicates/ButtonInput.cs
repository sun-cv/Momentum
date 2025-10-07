using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Momentum
{

    [Serializable]
    public struct ButtonPredicate
    {
        public ButtonInput input;
        public ButtonCondition condition;
    }

    [Serializable]
    public class ButtonInputRequirement : AbilityPredicate
    {
        [Header("Single button requirement")]
        public ButtonPredicate predicate;

        [Header("Multi button requirement")]
        public bool enableMultiButton;
        public List<ButtonPredicate>  multiButtonPredicates;

        public override bool IsMet(Context context)
        {
            if (context.entity is not HeroContext hero)
            {
                throw new InvalidOperationException("Button Input requirement only available for Hero.");
            }

            bool validInput = false;

            if (!enableMultiButton)
                validInput = hero.Intent.input.activeButtons.Any((activeButton) => activeButton.Input == predicate.input && activeButton.Condition == predicate.condition);
            else
                validInput = multiButtonPredicates.All((requiredButton) => hero.Intent.input.activeButtons.Any((activeButton) => activeButton.Input == requiredButton.input && activeButton.Condition == requiredButton.condition));

            return validInput;
        }
    }


}
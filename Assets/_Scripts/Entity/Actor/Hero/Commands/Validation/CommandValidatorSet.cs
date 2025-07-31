

using System;
using System.Collections.Generic;
using System.Linq;
using Momentum.Interface;

namespace Momentum.Actor.Hero
{


    public class CommandValidatorSet
    {
        private readonly List<ICommandValidator> validators;

        public CommandValidatorSet(params ICommandValidator[] validators)
        {
            this.validators = validators.ToList();
        }

        public static CommandValidatorSet Compose(CooldownValidator cooldownValidator, params CommandValidatorSet[] sets)
        {
            return new CommandValidatorSet(sets.SelectMany(set => set.validators).ToArray());
        }

        public CommandValidatorSet With(params ICommandValidator[] more)
        {
            return new CommandValidatorSet(validators.Concat(more).ToArray());
        }

        public bool Validate(HeroContext hero)
        {
            return validators.All(validator => validator.CanExecute(hero));
        }
    }

    public static class CommonValidatorSets
    {
        public static readonly CommandValidatorSet MustBeMobile = new(
            new NotDisabledValidator(),
            new MovementIntentValidator()
        );


    }


}
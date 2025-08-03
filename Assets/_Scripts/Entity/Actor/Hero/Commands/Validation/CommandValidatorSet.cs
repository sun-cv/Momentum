using System.Collections.Generic;
using System.Linq;

namespace Momentum
{


    public class CommandValidatorSet
    {
        private readonly List<ICommandValidator> validators;
        private IValidatorService service;

        public CommandValidatorSet(params ICommandValidator[] validators)
        {
            this.validators = validators.ToList();
            service = ValidatorService.Get();
        }

        public static CommandValidatorSet Compose(CooldownValidator cooldownValidator, params CommandValidatorSet[] sets)
        {
            return new CommandValidatorSet(sets.SelectMany(set => set.validators).ToArray());
        }

        public CommandValidatorSet With(params ICommandValidator[] more)
        {
            return new CommandValidatorSet(validators.Concat(more).ToArray());
        }

        public bool Validate()
        {
            // Debug();
            return validators.All(validator => validator.CanExecute(service));
        }

        public bool Debug()
        {
            foreach (var validator in validators)
            {
                bool result = validator.CanExecute(service);
                if (!result)
                {
                    UnityEngine.Debug.Log($"Validator failed: {validator.GetType().Name}");
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<ICommandValidator> GetAll() => validators;
    }

    public static class CommonValidatorSets
    {
        public static readonly CommandValidatorSet MustBeMobile = new(
            new NotDisabledValidator(),
            new MovementIntentValidator()
        );


    }


}
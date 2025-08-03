using System;
using UnityEngine;

namespace Momentum
{


    public class MovementIntentValidator : ICommandValidator
    {
        public bool CanExecute(IValidatorService service)
        {
            
            return service.Resolve<HeroContext>().movement.locomotion;
        }
    }

    public class IdleIntentValidator : ICommandValidator
    {
        public bool CanExecute(IValidatorService service)
        {
            return service.Resolve<HeroContext>().movement.idle;
        }
    }

    public class NotDisabledValidator : ICommandValidator
    {
        public bool CanExecute(IValidatorService service)
        {
            return !service.Resolve<HeroContext>().condition.disabled;
        }
    }

    public class NotStunnedValidator : ICommandValidator
    {
        public bool CanExecute(IValidatorService service)
        {
            return !service.Resolve<HeroContext>().condition.stunned;
        }
    }

    public class NotKnockedBackValidator : ICommandValidator
    {
        public bool CanExecute(IValidatorService service)
        {
            return !service.Resolve<HeroContext>().condition.knockedBack;
        }
    }
    
    public class NotSlowedValidator : ICommandValidator
    {
        public bool CanExecute(IValidatorService service)
        {
            return !service.Resolve<HeroContext>().condition.slowed;
        }
    }

    public class CooldownValidator : ICommandValidator
    {
        private readonly Func<ICooldownHandler, bool> onCooldown;

        public CooldownValidator(Func<ICooldownHandler, bool> onCooldown)
        {
            this.onCooldown = onCooldown;
        }

        public bool CanExecute(IValidatorService service)
        {
            return onCooldown(service.Resolve<ICooldownHandler>());
        }

    }

    public class CooldownValidatorcombo : ICommandValidator
    {
        private readonly Func<ICooldownHandler, bool> onCooldown;

        public CooldownValidatorcombo(Func<ICooldownHandler, bool> onCooldown)
        {
            this.onCooldown = onCooldown;
        }

        public bool CanExecute(IValidatorService service)
        {
            return onCooldown(service.Resolve<ICooldownHandler>());
        }

    }


    public class ResourceValidator : ICommandValidator
    {
        private readonly Func<HeroContext, float> resourceAvailable;
        private readonly float required;

        public ResourceValidator(Func<HeroContext, float> resourceAvailable, float required )
        {
            this.resourceAvailable  = resourceAvailable;
            this.required           = required;
        }

        public bool CanExecute(IValidatorService service)
        {
            return resourceAvailable(service.Resolve<HeroContext>()) >= required;
        } 
    }



}



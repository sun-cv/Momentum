using System;
using UnityEngine;
using Momentum.Interface;

namespace Momentum.Actor.Hero
{

    public class MovementIntentValidator : ICommandValidator
    {
        public bool CanExecute(HeroContext context)
        {
            return context.movement.locomotion;
        }
    }

    public class IdleIntentValidator : ICommandValidator
    {
        public bool CanExecute(HeroContext context)
        {
            return context.movement.idle;
        }
    }

    public class NotDisabledValidator : ICommandValidator
    {
        public bool CanExecute(HeroContext context)
        {
            return !context.condition.disabled;
        }
    }

    public class NotStunnedValidator : ICommandValidator
    {
        public bool CanExecute(HeroContext context)
        {
            return !context.condition.stunned;
        }
    }

    public class NotKnockedBackValidator : ICommandValidator
    {
        public bool CanExecute(HeroContext context)
        {
            return !context.condition.knockedBack;
        }
    }
    
    public class NotSlowedValidator : ICommandValidator
    {
        public bool CanExecute(HeroContext context)
        {
            return !context.condition.slowed;
        }
    }
    

    public class CooldownValidator : ICommandValidator
    {
        private readonly Func<HeroContext, bool> onCooldown;

        public CooldownValidator(Func<HeroContext, bool> onCooldown)
        {
            this.onCooldown = onCooldown;
        }

        public bool CanExecute(HeroContext context)
        {
            return onCooldown(context);
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

        public bool CanExecute(HeroContext context)
        {
            return resourceAvailable(context) >= required;
        } 
    }



}



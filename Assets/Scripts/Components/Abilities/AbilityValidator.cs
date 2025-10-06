using System.Linq;
using UnityEngine;


namespace Momentum
{

    public class AbilityValidator
    {
        AbilityCooldowns cooldown;
        Context context;

        public AbilityValidator(Context context, AbilityCooldowns cooldown)
        {
            this.context  = context;
            this.cooldown = cooldown;
        }

        public bool IsValid(AbilityRequest request)             => !IsOnCooldown(request) && MeetsRequirements(request);
        public bool IsOnCooldown(AbilityRequest request)        => cooldown.IsActive(request.ability) || cooldown.GlobalActive(request.ability);
        public bool MeetsRequirements(AbilityRequest request)   => request.ability.predicates.All((rule) => rule.IsMet(context));
    
        public bool IsBufferable(AbilityRequest request)        => request.ability.queueing.bufferable; 
        public bool IsExpired(AbilityRequest request)           => Time.time - request.Meta.TimeCreated  > request.Expiration;
        public bool IsExpiredBuffer(AbilityRequest request)     => Time.time - request.Meta.TimeBuffered > request.InputBuffer;
        public bool IsExpiredValid(AbilityRequest request)      => Time.time - request.Meta.TimeValidated > request.ValidBuffer;

    }

}
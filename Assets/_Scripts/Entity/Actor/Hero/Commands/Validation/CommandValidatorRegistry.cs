using System;
using System.Collections.Generic;
using UnityEngine;


namespace Momentum
{

    public static class CommandValidatorRegistry
    {
        private static readonly Dictionary<Type, CommandValidatorSet> map = new()
        {
            { typeof(DashCommand), CommonValidatorSets.MustBeMobile.With(
                new CooldownValidator((handler) => !handler.IsActive<DashCooldown>())) 
            },

            { typeof(BasicAttackCommand), new CommandValidatorSet(
                new NotDisabledValidator(),
                new CooldownValidator((handler) => !handler.IsActive<AttackIntervalCooldown>()),
                new CooldownValidatorcombo((handler) => !handler.IsActive<AttackComboIntervalCooldown>()))
            } 
        };


        public static CommandValidatorSet Get(Type commandType)
        {
            if (map.TryGetValue(commandType, out var validators))
            {
                return validators;
            }

            return new CommandValidatorSet();
        }
    }













}
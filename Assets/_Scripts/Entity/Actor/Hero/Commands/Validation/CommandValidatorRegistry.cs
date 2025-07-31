using System;
using System.Collections.Generic;



namespace Momentum.Actor.Hero
{

    public static class CommandValidatorRegistry
    {
        private static readonly Dictionary<Type, CommandValidatorSet> map = new()
        {
            { typeof(DashCommand), CommonValidatorSets.MustBeMobile.With(
                new CooldownValidator((context) => !context.action.dash.dashCooldown)) 
            },

            { typeof(BasicAttackCommand), new CommandValidatorSet(
                new NotDisabledValidator(),
                new CooldownValidator((context) => !context.action.basicAttack.attackCooldown),
                new CooldownValidator((context) => !context.action.basicAttack.attackComboCooldown))
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
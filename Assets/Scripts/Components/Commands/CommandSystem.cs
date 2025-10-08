using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Momentum.Abilities;

namespace Momentum
{


    public interface ICommandSystem 
    {
        public void Initialize(InputIntent intent);
        public void Tick();
        public void AssignAbilitySystem(IAbilityEngine system);
        public void AssignAbilityMap(List<Ability> abilities);
    }



    public class CommandSystem : ICommandSystem
    {

        InputIntent intent;

        CommandRegistry  registry = new();
        CommandResolver  resolver = new();

        IAbilityEngine   ability;

        public void Initialize(InputIntent intent)
        {
            this.intent = intent;
        }

        public void Tick()
        {
            // ability.Request(resolver.InputIntentToAbilities(registry, intent.activeButtons));
        }

        public void AssignAbilitySystem(IAbilityEngine system)
        {
            this.ability = system;
        }
        public void AssignAbilityMap(List<Ability> abilities)
        {
            registry.GenerateMap(abilities);
        }
    }


    public class CommandResolver
    {
        public List<Ability> InputIntentToAbilities(CommandRegistry registry, HashSet<Button> buttons)
        {
            List<Ability> resolvedAbilities = new();

            foreach (var button in buttons)
            {
                if (registry.Map.TryGetValue(button.predicate, out var abilities))
                {
                    resolvedAbilities.AddRange(abilities);
                }
            }
            return resolvedAbilities;
        }   
    }

    public class CommandRegistry
    {
        private Dictionary<ButtonPredicate, List<Ability>> map = new();

        public void GenerateMap(List<Ability> abilities)
        {   
            map = new();

            foreach (var ability in abilities)
            {
                foreach ( var requirement in ability.casting.predicates)
                {
                    if (requirement is not ButtonInputRequirement buttonInputRequirement)
                        continue;

                    ButtonPredicate button = new();

                    if (buttonInputRequirement.enableMultiButton)
                        button = buttonInputRequirement.multiButtonPredicates.First();
    
                    else
                        button = buttonInputRequirement.predicate;

                    if (!map.TryGetValue(button, out var abilityList))
                    {
                        abilityList = new List<Ability>();
                        map.Add(button, abilityList);
                    }

                    abilityList.Add(ability);
                }
            }
        }
    
        public Dictionary<ButtonPredicate, List<Ability>> Map => map;

    }


}
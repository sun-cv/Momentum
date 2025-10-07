using System.Collections.Generic;
using Momentum.Abilities;


namespace Momentum
{

    public interface IAbilityEngine
    {

    }

    public class AbilityEngine : IAbilityEngine
    {
        Router router;
        Factory factory;
        Processor processor;


        public void Cast(List<Ability> abilities)   {}
        public void Cast(Ability ability)           {}



    }

}
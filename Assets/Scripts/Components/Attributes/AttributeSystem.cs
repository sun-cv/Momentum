using System;
using System.Collections.Generic;
using UnityEngine;


namespace Momentum
{


    public interface IAttributeSystem
    {
        public void  Register(Attribute attribute);
        public T     Get<T>() where T : IRuntimeAttribute;
        public float Resolve(string stat, float baseValue);
    }

    public class AttributeSystem : IAttributeSystem
    {
        private readonly Dictionary<Type, IRuntimeAttribute> attributes = new();

        readonly StatsMediator mediator = new();
        public   StatsMediator Mediator => mediator;

        public void Register(Attribute definition)
        {
            var attribute = definition.CreateRuntime();
            foreach (var Interface in attribute.GetType().GetInterfaces())
            {
                if (typeof(IRuntimeAttribute).IsAssignableFrom(Interface) && Interface != typeof(IRuntimeAttribute))
                {
                    attributes[Interface] = attribute;
                }
            }        
        }

        public T Get<T>() where T : IRuntimeAttribute => (T)attributes[typeof(T)];

        public float Resolve(string stat, float baseValue)
        {
            var query = new Query(stat, baseValue);

            Mediator.PerformQuery(this, query);

            foreach (var attribute in attributes.Values)
                attribute.Mediator.PerformQuery(this, query);

            return query.value;
        }

        public void AddModifier(StatModifier modifier)
        {
            Mediator.AddModifier(modifier);
        }

    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Momentum
{


    public interface IPredicateRegistry
    {
        public T Get<T>()   where T : IPredicate;
        public bool Is<T>() where T : IPredicate;
        T Resolve<T>();
    }

    public class PredicateRegistry : IRegistry, IPredicateRegistry
    {
        private readonly Dictionary<Type, IPredicate> predicates = new();
        private readonly Dictionary<Type, object>       services = new();

        public PredicateRegistry()
        {
            DiscoverPredicates();
        }

        private void DiscoverPredicates()
        {
            var predicateTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => !type.IsAbstract && typeof(IPredicate).IsAssignableFrom(type));
    
            foreach (var type in predicateTypes)
            {
                ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                
                if (ctor == null) 
                {
                    continue;
                }
    
                predicates[type] = (IPredicate)Activator.CreateInstance(type);
            }
        }


        public T Get<T>() where T : IPredicate
        {
            return (T)predicates[typeof(T)];
        }

        public bool Is<T>() where T : IPredicate
        {
            return Get<T>().Evaluate();
        }

        public IEnumerable<(string Name, bool Value)> GetAllCurrentValues()
        {
            foreach (var kv in predicates)
            {
                yield return (kv.Key.Name, kv.Value.Evaluate());
            }
        }

        public void Register<T>(T instance)
        {
            services[typeof(T)] = instance;

            Predicate.Resolve<ConfigRegistry, Attribute>(); 
        }

        public T Resolve<T>()
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new Exception($"Validation context missing service type {typeof(T)}");
        }
    }
}


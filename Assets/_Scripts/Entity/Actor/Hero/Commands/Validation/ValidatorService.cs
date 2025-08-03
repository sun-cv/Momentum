using System;
using System.Collections.Generic;

namespace Momentum
{

        //Rework required? Same static access pattern as ICooldownHandler?


    public interface IValidatorService
    {
        T Resolve<T>();
    }


    public class ValidatorService : IValidatorService
    {
        private readonly Dictionary<Type, object> services = new();


        public void Register<T>(T instance)
        {
            services[typeof(T)] = instance;
        }


        public T Resolve<T>()
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new Exception($"Validation context missing service type {typeof(T)}");
        }


        public static IValidatorService Get()
        {
            return Registry.Get<IValidatorService>();
        }

    }


}
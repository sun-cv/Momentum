using System;
using UnityEngine;


namespace Momentum
{
    public class Provider : MonoBehaviour, IDependencyProvider
    {
        // [Provide]
        public ServiceA ProvideServiceA()
        {
            return new ServiceA();
        }

        // [Provide]
        public ServiceB ProvideServiceB()
        {
            return new ServiceB();
        }

        // [Provide]
        public FactoryA ProvideFactoryA()
        {
            return new FactoryA();
        }
    }


    public class ServiceA
    {
        public void Initialize(String string1)
        {
        }

    }

    public class ServiceB 
    {
        public void Initialize(String string1)
        {
        }

    }

    


    public class FactoryA
    {
        ServiceA cachedServiceA;

        public ServiceA CreateServiceA()
        {
            cachedServiceA ??= new ServiceA();
            return cachedServiceA;
        }
    }

    

}
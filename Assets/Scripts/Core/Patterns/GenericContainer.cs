using System;
using System.Collections.Generic;


namespace Momentum
{

    public class Container
    {
        public Dictionary<string, object> values = new();

        public void Set(string key, object value)
        {
            if(values.ContainsKey(key))
            {
                throw new InvalidOperationException($"Container : {key} is already registered.");
            };
        }            

        public object Get(string key)
        {
            if(!values.TryGetValue(key, out object value))
            {
                throw new KeyNotFoundException($"Container: Service of type {key} is not registered.");
            }
            return value;
        }
    }

    public class Container<S>
    {
    
        public Dictionary<Type, S> values = new();

        public void Set<T>(T value) where T : S
        {
            var type = typeof(T);

            if(values.ContainsKey(type))
            {
                throw new InvalidOperationException($"Generic container {typeof(S)}: Type{type} is already registered.");
            };
        }            

        public T Get<T>() where T : S
        {
            if(!values.TryGetValue(typeof(T), out S value))
            {
                throw new KeyNotFoundException($"Generic container {typeof(S)}: Service of type {typeof(T)} is not registered.");
            }
            return (T)value;
        }
    }

}
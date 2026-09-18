using System;
using System.Collections.Generic;



namespace Game.Common
{

    public sealed class Registry
    {
        private readonly Dictionary<Type, object> stores = new();

        public void Register<T>(string id, T value)
        {
            if (!Store<T>().TryAdd(id, value))
                throw new InvalidOperationException($"Registry already holds <{typeof(T).Name}> with id: {id}");
        }

        public void Deregister<T>(string id)
        {
            if (!Store<T>().Remove(id))
                throw new InvalidOperationException($"Registry holds no <{typeof(T).Name}> to remove with id: {id}");
        }

        public T Get<T>(string id)
        {
            if (!Store<T>().TryGetValue(id, out var value))
                throw new KeyNotFoundException($"Registry holds no <{typeof(T).Name}> with id: {id}");

            return value;
        }

        private Dictionary<string, T> Store<T>()
        {
            if (!stores.TryGetValue(typeof(T), out var store))
            {
                store = new Dictionary<string, T>();
                stores[typeof(T)] = store;
            }

            return (Dictionary<string, T>)store;
        }
    }
}

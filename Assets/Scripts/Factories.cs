using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;



public interface IFactory
{
    
}

public interface IActorFactory : IFactory
{
    Actor Spawn(Vector3 position);
}

public interface ICorpseFactory : IActorFactory
{
    Actor SpawnCorpse(Actor owner, Vector3 position);
}

public interface IRespawnFactory : IActorFactory
{
    
}


[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FactoryAttribute : Attribute 
{
    public string Name { get; }
    
    public FactoryAttribute(string actorName)
    {
        Name = actorName;
    }
}

public static class Factories
{
    // ===============================================================================
    //  Public API
    // ===============================================================================

    public static T Get<T>()
    {
        return Registry.Get<T>();
    }


    public static T Get<T>(string name)
    {
        return Registry.Get<T>(name);
    }

    public static IFactory Get(string name)
    {
        return Registry.Get(name);
    }

    // ===============================================================================

    public static void Start()
    {
        Setup.Start();
    }

    // ===============================================================================

    private static class Setup
    {
        public static void Start()
        {
            AutoRegisterFactories();
        }

        public static void AutoRegisterFactories()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract)
                    continue;

                var attribute = type.GetCustomAttribute<FactoryAttribute>();

                if (attribute == null)
                    continue;

                var constructor = type.GetConstructor(Type.EmptyTypes);

                if (constructor == null)
                {
                    Log.Error($"[Factory] class {type.Name} has no no public empty constructor.");
                    continue;
                }
                var factory = Activator.CreateInstance(type);

                Registry.RegisterFactory(attribute.Name, type, (IFactory)factory);
            }
        }
    }

    private static class Registry
    {
        private static readonly Dictionary<Type, IFactory>   byType = new();
        private static readonly Dictionary<string, IFactory> byName = new();


        // ===============================================================================

        public static void RegisterFactory(string name, Type type, IFactory factory)
        {
            if (byType.ContainsKey(type))
                throw new InvalidOperationException($"Factory {type.Name} is already registered.");

            byType[type] = factory;
            byName[name] = factory;
        }

        public static void DeregisterFactory(string name)
        {
            if (!byName.TryGetValue(name, out var factory))
                throw new InvalidOperationException($"Factory for '{name}' is not registered.");

            byName.Remove(name);

            var type = factory.GetType();
            byType.Remove(type);
        }

        // ===============================================================================

        public static IFactory Get(string name)
        {
            if (!byName.TryGetValue(name, out var factory))
                throw new KeyNotFoundException($"No factory registered for: '{name}'");

            return factory;
        }

        public static T Get<T>()
        {
            if (!byType.TryGetValue(typeof(T), out var factory))
                throw new KeyNotFoundException($"No factory registered for: '{typeof(T).Name}'");

            return (T)factory;
        }


        public static T Get<T>(string name)
        {
            if (!byName.TryGetValue(name, out var factory))
                throw new KeyNotFoundException($"No factory registered for: '{name}'");

            var type = factory.GetType();

            if (factory is not T typed)
                throw new KeyNotFoundException($"Factory registered for '{name}' is not assignable to '{typeof(T).Name}'");

            return (T)factory;
        }

        public static bool HasFactory(string name)
        {
            return byName.ContainsKey(name);
        }

        // ===============================================================================

        public static void Clear()
        {
            byType.Clear();
            byName.Clear();
        }

        public static void Dispose() => Clear();

        public static List<string> RegisteredFactories => byName.Keys.ToList();
    }

    // ===============================================================================

    readonly static Logger Log = Logging.For(LogSystem.Factories);

    public static void Dispose()
    {
        Registry.Dispose();
    }
}
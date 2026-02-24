using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;



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

    public static Actor CreateActor(string actorName, Vector3 position)
    {
        return Registry.CreateActor(actorName, position);
    }

    public static bool CanCreate(string actorName)
    {
        return Registry.HasFactory(actorName);
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
            
                Debug.Log("Should see1");


            foreach (var type in assembly.GetTypes())
            {
                Debug.Log("Should see2");


                if (!type.IsClass || type.IsAbstract && !type.IsSealed)
                    continue;

                Debug.Log("Should see3");


                var attribute = type.GetCustomAttribute<FactoryAttribute>();
                if (attribute == null)
                    continue;

                Debug.Log("Should see4");

                // Look for a static Create method with signature: Actor Create(Vector3)
                var createMethod = type.GetMethod(
                    "Create",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(Vector3) },
                    null
                );

                if (createMethod == null)
                {
                    Log.Error($"[ActorFactory] class {type.Name} has no public static Create(Vector3) method.");
                    continue;
                }

                if (!typeof(Actor).IsAssignableFrom(createMethod.ReturnType))
                {
                    Log.Error($"[ActorFactory] class {type.Name}.Create must return Actor or subclass.");
                    continue;
                }

                var factory = (Func<Vector3, Actor>)Delegate.CreateDelegate(
                    typeof(Func<Vector3, Actor>), 
                    createMethod
                );

                Debug.Log(factory.GetType().Name);

                Registry.RegisterFactory(attribute.Name, factory);
                
                Log.Debug($"Registered factory: {attribute.Name} -> {type.Name}");
            }
        }
    }

    private static class Registry
    {
        private static readonly Dictionary<string, Func<Vector3, Actor>> factories = new();

        // ===============================================================================

        public static void RegisterFactory(string actorName, Func<Vector3, Actor> factory)
        {
            if (factories.ContainsKey(actorName))
                throw new InvalidOperationException($"Factory for '{actorName}' is already registered.");

            factories[actorName] = factory;
        }

        public static void DeregisterFactory(string actorName)
        {
            if (!factories.ContainsKey(actorName))
                throw new InvalidOperationException($"Factory for '{actorName}' is not registered.");

            factories.Remove(actorName);
        }

        // ===============================================================================

        public static Actor CreateActor(string actorName, Vector3 position)
        {
            if (!factories.TryGetValue(actorName, out var factory))
                throw new KeyNotFoundException($"No factory registered for actor: '{actorName}'");

            return factory(position);
        }

        public static bool HasFactory(string actorName)
        {
            return factories.ContainsKey(actorName);
        }

        // ===============================================================================

        public static void Clear()
        {
            factories.Clear();
        }

        public static void Dispose() => Clear();

        public static List<string> RegisteredFactories => factories.Keys.ToList();
    }

    // ===============================================================================

    readonly static Logger Log = Logging.For(LogSystem.Factories);

    public static void Dispose()
    {
        Registry.Dispose();
    }
}
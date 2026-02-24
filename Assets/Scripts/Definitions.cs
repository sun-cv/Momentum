using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;



[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class DefinitionAttribute : Attribute {  }


public static class Definitions
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

    public static bool TryGet<T>(out T instance)
    {
        return Registry.TryGet(out instance);
    }

    public static bool TryGet<T>(string name, out T instance)
    {
        return Registry.TryGet(name, out instance);
    }

    public static void Register<T>(T definition)
    {
        Registry.RegisterDefinition(definition);
    }

    public static void Deregister<T>()
    {
        Registry.DeregisterDefinition<T>();
    }

    public static void Deregister(Type type)
    {
        Registry.DeregisterDefinition(type);
    }

    public static void Deregister(string name)
    {
        Registry.DeregisterDefinition(name);
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
            AutoRegister();
        }

        public static void AutoRegister()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract) 
                    continue;

                if (type.GetCustomAttribute<DefinitionAttribute>() == null) 
                    continue;
            
        
                var constructor = type.GetConstructor(Type.EmptyTypes);
                
                if (constructor == null)
                    throw new InvalidOperationException($"[Definition] class {type.Name} has no public empty constructor.");
                
                var definition = (Definition)Activator.CreateInstance(type);

                Registry.RegisterDefinition(type, definition);
            }
        }
    }

private static class Registry
{
    private static readonly Dictionary<Type,   object> byType = new();
    private static readonly Dictionary<string, object> byName = new();

    // ===============================================================================

    public static void RegisterDefinition<T>(T definition)
    {
        if (byType.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"Definition {typeof(T).Name} is already registered.");

        byType[typeof(T)] = definition;

        if (definition is Definition instance && instance.Name != null)
            byName[instance.Name] = definition;
    }

    public static void RegisterDefinition(Type type, object definition)
    {
        if (byType.ContainsKey(type))
            throw new InvalidOperationException($"Definition {type.Name} is already registered.");

        byType[type] = definition;

        if (definition is Definition instance && instance.Name != null)
            byName[instance.Name] = definition;
    }

    public static void DeregisterDefinition<T>()
    {
        if (byType.TryGetValue(typeof(T), out var definition))
        {
            byType.Remove(typeof(T));

            if (definition is Definition instance && instance.Name != null)
                byName.Remove(instance.Name);
        }
    }

    public static void DeregisterDefinition(Type type)
    {
        if (!byType.TryGetValue(type, out var definition))
            throw new InvalidOperationException($"Definition {type.Name} is not registered.");

        byType.Remove(type);

        if (definition is Definition instance && instance.Name != null)
            byName.Remove(instance.Name);
    }

    public static void DeregisterDefinition(string name)
    {
        if (!byName.TryGetValue(name, out var definition))
            throw new InvalidOperationException($"Definition with name '{name}' is not registered.");

        byName.Remove(name);

        var type = definition.GetType();
        byType.Remove(type);
    }

    // ===============================================================================

    public static T Get<T>()
    {
        if (!byType.ContainsKey(typeof(T)))
            throw new KeyNotFoundException($"Definition of type {typeof(T).Name} is not registered.");

        return (T)byType[typeof(T)];
    }

    public static T Get<T>(string name)
    {
        if (!byName.ContainsKey(name))
            throw new KeyNotFoundException($"Definition with name '{name}' is not registered.");

        return (T)byName[name];
    }

    public static bool TryGet<T>(out T instance)
    {
        if (byType.TryGetValue(typeof(T), out var definition))
        {
            instance = (T)definition;
            return true;
        }
        instance = default;
        return false;
    }

    public static bool TryGet<T>(string name, out T instance)
    {
        if (byName.TryGetValue(name, out var definition))
        {
            instance = (T)definition;
            return true;
        }
        instance = default;
        return false;
    }

    // ===============================================================================

    public static void Clear()
    {
        byType.Clear();
        byName.Clear();
    }

    public static void Dispose()                                            => Clear();

    public static List<object> RegisteredDefinitionsList                    => byType.Values.ToList();
    public static Dictionary<Type, object> RegisteredDefinitionsByType      => byType;
    public static Dictionary<string, object> RegisteredDefinitionsByName    => byName;
}


    // ===============================================================================

    readonly static Logger Log = Logging.For(LogSystem.Definitions);

    public static void Dispose()
    {
        Registry.Dispose();
    }

}

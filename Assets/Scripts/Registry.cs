using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;


[AttributeUsage(AttributeTargets.Method)]
public class FunctionAttribute : Attribute {};


[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ServiceAttribute : Attribute {  };

public delegate void RegisteredFunction(object args);
 

public static class Function
{
    public static void Call(string name, object args) => Registry.Functions.Call(name, args);
}


public static class Registry
{

    public static void Initialize()
    {
        Registry.Prefabs.Initialize();
        Registry.Services.Initialize();
        Registry.Functions.Initialize();
    }


    public static class Prefabs
    {

        private static readonly Dictionary<string, GameObject> dictionary = new();
    
        public static void Initialize() => Reload();
    
        public static void Reload()
        {
            Load();
            LoadPrefab();   
        }
    
        private static void Load()
        {
        }
    
        private static void LoadPrefab()
        {
            foreach (var data in Resources.LoadAll<GameObject>("Prefab/Entity"))
                dictionary[data.name] = data;
        }
    
        public static T Get<T>(string name) where T : class
        {
            if (dictionary.TryGetValue(name, out var value))
                return value as T;
            return null;
        }
    }

    public static class Services
    {
        private static readonly Dictionary<Type, object> dictionary = new();

        public static void Initialize()
        {
            RegisterServices();
        }

        public static void Register<T>(T service)
        {
            if (dictionary.ContainsKey(typeof(T)))
                throw new InvalidOperationException($"Type {typeof(T)} is already registered.");

            dictionary[typeof(T)] = service;
        }

        public static void Deregister<T>(T service)
        {
            dictionary.Remove(typeof(T));
        }

        public static T Get<T>()
        {
            if (!dictionary.ContainsKey(typeof(T)))
                throw new KeyNotFoundException($"Service of type {typeof(T)} is not registered.");

            return (T)dictionary[typeof(T)];
        }

        public static bool TryGet<T>(out T instance)
        {
            if (dictionary.TryGetValue(typeof(T), out var service))
            {
                instance = (T)service;
                return true;
            }
            instance = default;
            return false;
        }

        public static void Clear()
        {
            dictionary.Clear();
        }

        public static void RegisterServices()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
    
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract) continue;

                if (type.GetCustomAttribute<ServiceAttribute>() == null) continue;

                var constructor = type.GetConstructor(Type.EmptyTypes);

                if (constructor == null)
                    throw new InvalidOperationException($"[Service] class {type.Name} has no public empty constructor.");

                if (!dictionary.ContainsKey(type))
                {
                    var service = Activator.CreateInstance(type);

                    if (service is IService typed)
                    {
                        GameTick.Register(typed);
                    }

                    dictionary[type] = service;
                    Debug.Log($"Registered {type.Name}");
                }
            }
        }

        public static Dictionary<Type, object> RegisteredServices => dictionary;
    }



    public static class Functions
    {
    
        private static readonly Dictionary<string, RegisteredFunction> function = new();
    
    
        public static void Initialize() => RegisterFunctions();
    
        public static void Register(string name, RegisteredFunction func)
        {
            if (!function.ContainsKey(name))
                function[name] = func;
        }
    
        public static void Call(string name, object args)
        {
            if (function.TryGetValue(name, out var func))
                func(args);
            else
                Console.WriteLine($"Function '{name}' not registered.");
        }
    
        public static void RegisterFunctions()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
    
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (method.GetCustomAttribute<FunctionAttribute>() != null)
                    {
                        function[method.Name] = CreateInvoker(method);
                        Debug.Log($"Registered {method.Name}");
                    }
                }
            }
        }
    
        private static RegisteredFunction CreateInvoker(MethodInfo method)
        {
            return args =>
            {
                var paramInfos = method.GetParameters();
                var paramValues = new object[paramInfos.Length];
    
                foreach (var (paramInfo, i) in paramInfos.Select((p, idx) => (p, idx)))
                {
                    var prop = args.GetType().GetProperty(paramInfo.Name) ?? throw new Exception($"Missing argument: {paramInfo.Name}");
                    paramValues[i] = prop.GetValue(args);
                }
    
                method.Invoke(null, paramValues);
            };
        }
    }



}
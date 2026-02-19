using System;
using System.Linq;
using System.Collections.Generic;



public static class Actors
{
    // ===============================================================================
    //  Public API
    // ===============================================================================

    public static void Register(Bridge bridge)
    {
        Registry.Register(bridge);
    }

    public static void Deregister(Bridge bridge)
    {
        Registry.Deregister(bridge);
    }

    public static Bridge GetID(Guid guid)
    {
        return Registry.GetID(guid);
    }

    public static IEnumerable<Bridge> GetActors()
    {
        return Registry.Actors;
    }

    public static IEnumerable<Bridge> GetInterface<T>() where T : class
    {
        if (Registry.Interfaces.TryGetValue(typeof(T), out var list))
            return list;

        return Enumerable.Empty<Bridge>();
    }

    // ===============================================================================
    //  Registry
    // ===============================================================================

    private static class Registry
    {
        static readonly List<Bridge> actors                         = new();
        static readonly Dictionary<Type, List<Bridge>> interfaces   = new();
        
        static readonly HashSet<Type> indexedInterfaces             = new()
        {
            typeof(IDepthColliding),
            typeof(IDepthSorted),
        };

        // ===================================
        //  Queries
        // ===================================

        public static Bridge GetID(Guid guid)
        {
            return actors.FirstOrDefault(bridge => bridge.Owner.RuntimeID == guid);
        }

        public static List<Bridge> Actors => actors;
        public static Dictionary<Type, List<Bridge>> Interfaces => interfaces;

        // ===================================
        // Mutations
        // ===================================

        public static void Register(Bridge bridge)
        {
            actors.Add(bridge);

            foreach (var interfaceType in indexedInterfaces)
            {
                if (interfaceType.IsAssignableFrom(bridge.Owner.GetType()))
                {
                    if (!interfaces.ContainsKey(interfaceType))
                    {
                        interfaces[interfaceType] = new();
                    }
                    interfaces[interfaceType].Add(bridge);
                }
            }
        }

        public static void Deregister(Bridge bridge)
        {
            actors.Remove(bridge);

            foreach (var list in interfaces.Values)
            {
                list.Remove(bridge);
            }
        }

        public static void Clear()
        {
            actors.Clear();

            foreach (var list in interfaces.Values)
            {
                list.Clear();
            }
        }
    }
}
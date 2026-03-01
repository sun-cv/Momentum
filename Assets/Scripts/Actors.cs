using System;
using System.Linq;
using System.Collections.Generic;



public static class Actors
{
    // ===============================================================================
    //  Public API
    // ===============================================================================

    public static void Register(Actor actor)
    {
        Registry.Register(actor);
    }

    public static void Deregister(Actor actor)
    {
        Registry.Deregister(actor);
    }

    public static Actor GetID(Guid guid)
    {
        return Registry.GetID(guid);
    }

    public static IEnumerable<Actor> GetActors()
    {
        return Registry.Actors;
    }

    public static IEnumerable<Actor> GetInterface<T>() where T : class
    {
        if (Registry.Interfaces.TryGetValue(typeof(T), out var list))
            return list;

        return Enumerable.Empty<Actor>();
    }

    // ===============================================================================
    //  Registry
    // ===============================================================================

    private static class Registry
    {
        static readonly List<Actor> actors                         = new();
        static readonly Dictionary<Type, List<Actor>> interfaces   = new();
        
        static readonly HashSet<Type> indexedInterfaces            = new()
        {
            typeof(IDepthColliding),
            typeof(IDepthSorted),
        };

        // ===================================
        //  Queries
        // ===================================

        public static Actor GetID(Guid guid)
        {
            return actors.FirstOrDefault(Actor => Actor.RuntimeID == guid);
        }

        public static List<Actor> Actors => actors;
        public static Dictionary<Type, List<Actor>> Interfaces => interfaces;

        // ===================================
        // Mutations
        // ===================================

        public static void Register(Actor actor)
        {
            actors.Add(actor);

            foreach (var interfaceType in indexedInterfaces)
            {
                if (interfaceType.IsAssignableFrom(actor.GetType()))
                {
                    if (!interfaces.ContainsKey(interfaceType))
                    {
                        interfaces[interfaceType] = new();
                    }
                    interfaces[interfaceType].Add(actor);
                }
            }
        }

        public static void Deregister(Actor actor)
        {
            actors.Remove(actor);

            foreach (var list in interfaces.Values)
            {
                list.Remove(actor);
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
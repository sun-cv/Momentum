using System;
using System.Collections.Generic;



namespace Game.Common
{

    public readonly struct Entity
    {
        public int Index        { get; init; }
        public int Generation   { get; init; } 

        public Entity(int index, int generation)
        {
            Index       = index;
            Generation  = generation;
        }
    }

    public class EntityIdentityComparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y)
        {
            return x.Index == y.Index && x.Generation == y.Generation;
        }

        public int GetHashCode(Entity instance)
        {
            return HashCode.Combine(instance.Index, instance.Generation);
        }
    }

}

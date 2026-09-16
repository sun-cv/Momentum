using System;
using System.Collections.Generic;

namespace Game.Common
{

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

    public class LabelSetComparer : IEqualityComparer<HashSet<string>>
    {
        public bool Equals(HashSet<string> x, HashSet<string> y)
        {
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<string> instance)
        {
            int hash = 0;

            foreach (var label in instance)
                hash ^= label.GetHashCode();

            return hash;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

using Game.Common;



namespace Game.Realm
{
    public class Bodies
    {
        private readonly Dictionary<Rigidbody2D, Entity> owners;

        internal Bodies()
        {
            owners = new();
        }

        public bool Owner(Rigidbody2D body, out Entity entity)
        {
            return owners.TryGetValue(body, out entity);
        }

        internal void Add(Rigidbody2D body, Entity entity)
        {
            owners.Add(body, entity);
        }

        internal void Remove(Rigidbody2D body)
        {
            if (!owners.Remove(body))
                throw new Exception($"[Bodies] Remove for a body with no owner: {body.name}");
        }
    }
}

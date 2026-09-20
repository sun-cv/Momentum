using System.Collections.Generic;
using Game.Common;



namespace Game.Realm
{

    public class World
    {
        private readonly Entities entities;

        public World()
        {
            entities    = new();
        }

        public void Shutdown()
        {
            
        }

        public IReadOnlyCollection<Entity> Query<TDomain>(Mask<TDomain> mask)
        {
            return Entity.Mask.Query<TDomain>(mask);
        }

        public Entities Entity => entities;
    }


    public class Location
    {
    }

    public class Teleporter
    {
    }
}



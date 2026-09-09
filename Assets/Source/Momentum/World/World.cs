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

        public IReadOnlyCollection<Entity> Query(Mask mask)
        {
            return entities.Component.Query(mask);
        }

        public Entities Entity => entities;
    }



    public class Location
    {
    }


    public class Spawner
    {

    }

    public class Teleporter
    {

    }
}



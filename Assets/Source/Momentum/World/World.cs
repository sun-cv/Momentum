using Game.Common;



namespace Game.Realm
{

    public class World
    {
        private readonly Entities entities;

        public World()
        {
            entities = new();
        }

        public void Shutdown()
        {

        }

        public Entities Entity => entities;
    }
    

    public class Entities
    {
        private readonly EntityPool pool;
        private readonly Components components;
        private readonly ComponentModifier modifier;

        public Entities()
        {
            pool        = new();
            components  = new();
            modifier    = new(components);
        }
        
        public Entity Create()
        {
            return pool.Create();
        }

        public void Release(Entity entity)
        {
            pool.Release(entity);
        }

        public Health Health(Entity entity) => components.Store<Health>().View(entity);

        public ComponentModifier Modify => modifier;
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



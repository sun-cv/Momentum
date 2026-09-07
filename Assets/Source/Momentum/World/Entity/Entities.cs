using Game.Common;



namespace Game.Realm
{
    public partial class Entities
    {
        private readonly EntityPool pool;
        private readonly Components component;
        private readonly ComponentModifierRegistry modifier;

        public Entities()
        {
            pool        = new();
            component   = new();
            modifier    = new(component);
        }
        
        public Entity Create()
        {
            return pool.Create();
        }

        public void Release(Entity entity)
        {
            pool.Release(entity);
            component.Clear(entity);
        }

        public Components Component             => component;
        public ComponentModifierRegistry Modify => modifier;
    }

    public partial class Entities
    {
        public Health Health(Entity entity) => component.View<Health>(entity);
        public Energy Energy(Entity entity) => component.View<Energy>(entity);
    }
}




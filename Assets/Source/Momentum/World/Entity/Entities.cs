using System;
using Game.Common;



namespace Game.Realm
{
    public partial class Entities
    {
        private readonly Pool pool;
        private readonly Factory factory;
        private readonly Components component;

        public Entities()
        {
            pool        = new();
            component   = new();
            factory     = new(this);
        }
        
        internal Entity Allocate()
        {
            return pool.Allocate();
        }

        public void Release(Entity entity)
        {
            pool.Release(entity);
            component.Clear(entity);
        }

        
        public Factory Create                   => factory;
        public Components Component             => component;
        public Components.Modifier Modify       => component.Modify;
    }

    public partial class Entities
    {
        public Health Health(Entity entity) => component.View<Health>(entity);
        public Energy Energy(Entity entity) => component.View<Energy>(entity);
    }
}




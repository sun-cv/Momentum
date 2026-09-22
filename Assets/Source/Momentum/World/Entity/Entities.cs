using System.Collections.Generic;

using Game.Common;
using Game.Diagnostic;



namespace Game.Realm
{
    public partial class Entities
    {
        private readonly Pool pool;
        private readonly Masks masks;
        private readonly Assembler assembler;
        private readonly Components component;
        private readonly Capabilities capabilities;

        internal Entities()
        {
            pool            = new();
            masks           = new(pool);

            masks.Register<Innate>();
            masks.Register<Capability>();
            masks.Register<Components>();

            capabilities    = new(masks);
            component       = new(masks, pool);

            assembler       = new(masks, pool, component);
        }
        
        public Entity Create(Definition definition, Entity parent)
        {
            return assembler.Assemble(definition, parent);
        }

        public Entity Create(Blueprint blueprint, ConstructionParameter parameter)
        {
            return assembler.Assemble(blueprint, parameter);
        }

        public void Release(Entity entity)
        {
            assembler.Release(entity);
        }

        public bool Alive(Entity entity)
        {
            return pool.Alive(entity);
        }

        public bool Has<TComponent>(Entity entity) where TComponent : IComponent
        {
            return component.Has<TComponent>(entity);
        }       

        internal IReadOnlyCollection<Entity> Query<TDomain>(Mask<TDomain> mask)
        {
            return masks.Query<TDomain>(mask);
        }

        public Components Component             => component;
        public Capabilities Capability          => capabilities;
        public Components.Modifier Modify       => component.Modify;

        static Entities() => Log<Entities>.Level(Diagnostic.Log.Level.Debug);
    }
}



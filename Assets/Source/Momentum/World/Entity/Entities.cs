using System;
using Game.Common;
using Game.Diagnostic;
using UnityEngine;



namespace Game.Realm
{
    public partial class Entities
    {
        private readonly Pool pool;
        private readonly Masks masks;
        private readonly Assembler assembler;
        private readonly Components component;
        private readonly Capabilities capabilities;

        public Entities()
        {
            pool            = new();
            masks           = new(pool);

            masks.Register<Innate>();
            masks.Register<Capability>();
            masks.Register<Components>();

            capabilities    = new(masks);
            component       = new(masks, pool);

            assembler       = new(this);
        }
        
        internal Entity Allocate()
        {
            return pool.Allocate();
        }

        public Entity Create(Blueprint blueprint, Vector3 position)
        {
            return assembler.Assemble(blueprint, position);
        }

        public Entity Create(Blueprint blueprint, Vector3 position, Entity parent)
        {
            return assembler.Assemble(blueprint, position, parent);
        }        

        public bool Alive(Entity entity)
        {
            return pool.Alive(entity);
        }

        public void Release(Entity entity)
        {
            if (!pool.Alive(entity))
                throw new Exception($"Attemped to release dead entity");

            assembler.Dismantle(entity);
            masks.Release(entity);
            pool.Release(entity);
        }
        
        public Masks Mask                       => masks;
        public Components Component             => component;
        public Capabilities Capability          => capabilities;
        public Components.Modifier Modify       => component.Modify;

        static Entities() => Log<Entities>.Level(Diagnostic.Log.Level.Debug);
    }
}



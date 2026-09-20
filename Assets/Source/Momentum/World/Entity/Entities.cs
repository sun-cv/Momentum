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

        public Entities()
        {
            pool        = new();
            masks       = new(pool);

            masks.Register<Components>();
            masks.Register<Capabilities>();

            component   = new(pool, masks.Get<Components>());
            // capabilities    = new(masks.Get<Capabilities>());

            assembler   = new(this);
        }
        
        internal Entity Allocate()
        {
            return pool.Allocate();
        }

        public Entity Create(Blueprint blueprint, Vector3 position)
        {
            return assembler.Assemble(blueprint, position);
        }

        public void Release(Entity entity)
        {
            if (!pool.Alive(entity))
                throw new Exception($"Attemped to release dead entity");

            assembler.Dismantle(entity);
            pool.Release(entity);
        }
        
        public Masks Mask                       => masks;
        public Components Component             => component;
        public Components.Modifier Modify       => component.Modify;

        static Entities() => Log<Entities>.Level(Diagnostic.Log.Level.Debug);
    }


    public partial class Capabilities
    {

        private int capacity = Config.World.Capability.Capacity;

        public Capabilities()
        {

        }



        static Capabilities() => Log<Capabilities>.Level(Diagnostic.Log.Level.Debug);
        
    }

    public partial class Capabilities
    {
        
    }
}



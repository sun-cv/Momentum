using System;
using Game.Common;
using UnityEngine;



namespace Game.Realm
{
    public partial class Entities
    {
        private readonly Pool pool;
        private readonly Assembler assembler;
        private readonly Components component;

        public Entities()
        {
            pool        = new();
            component   = new();
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
                throw new Exception($"Attemped to release dead entity")

            assembler.Dismantle(entity);
            pool.Release(entity);
        }
        
        public Components Component             => component;
        public Components.Modifier Modify       => component.Modify;
    }

}




using System;
using System.Collections.Generic;
using Game.Common;
using Game.Diagnostic;
using Game.Data;


namespace Game.Realm
{
    public class EntityPool
    {
        private int increment;
        private int renewed; 
        private int capacity        = Config.World.Entity.PoolCapacity;

        private int [] free;
        private bool[] alive;
        private int [] generations;

        public EntityPool()
        {
            free        = new int [capacity];
            alive       = new bool[capacity];
            generations = new int [capacity];
        }

        public Entity Allocate()
        {   
            int index       = renewed > 0 ? free[--renewed] : increment++;
            
            EnsureCapacity(index);

            alive[index]    = true;
            int generation  = generations[index]; 

            return new Entity(index, generation);
        }

        public void Release(Entity entity)
        {
            if (!IsAlive(entity))
                return;

            alive[entity.Index] = false;
            generations[entity.Index]++;

            if (renewed == free.Length)
                Array.Resize(ref free, renewed * 2);

            free[renewed++] = entity.Index; 
        }

        private void EnsureCapacity(int index)
        {
            if (index < capacity)
                return;

            capacity = Math.Max(capacity * 2, index + 1);

            Array.Resize(ref alive,       capacity);
            Array.Resize(ref generations, capacity);
        }

        public IEnumerable<Entity> Enumerate()
        {
            for (int index = 0; index < alive.Length; index++)
                if (alive[index])
                    yield return new Entity { Index = index, Generation = generations[index] };
        }

        public bool IsAlive(Entity entity)
        {
            return entity.Index < alive.Length && alive[entity.Index] && entity.Generation == generations[entity.Index];
        }

        static EntityPool() => Log<EntityPool>.Level(Diagnostic.Log.Level.Debug);
    }
}




using System;
using Game.Common;
using Game.Diagnostic;



namespace Game.Realm
{
    public class EntityPool
    {
        private int increment;
        private int renewed; 
        private int capacity        = 10;

        private int [] free;
        private bool[] alive;
        private int [] generations;

        public EntityPool()
        {
            free        = new int [capacity];
            alive       = new bool[capacity];
            generations = new int [capacity];
        }

        public Entity Create()
        {   
            int index = renewed > 0 ? free[--renewed] : increment++;

            EnsureCapacity(index);

            alive[index] = true;

            return new Entity { Index = index, Generation = generations[index] };
        }

        public void Release(Entity entity)
        {
            if (!IsAlive(entity))
                return;

            alive[entity.Index] = false;
            generations[entity.Index]++;

            Log<EntityPool>.Debug("increment:", () => increment);
            Log<EntityPool>.Debug("renewed:", () => renewed);
            Log<EntityPool>.Debug("alive:", () => alive.Length);
            Log<EntityPool>.Debug("free:", () => free.Length);

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

        public bool IsAlive(Entity entity)
        {
            return entity.Index < alive.Length && alive[entity.Index] && entity.Generation == generations[entity.Index];
        }

        static EntityPool() => Log<EntityPool>.Level(Diagnostic.Log.Level.Debug);
    }
}




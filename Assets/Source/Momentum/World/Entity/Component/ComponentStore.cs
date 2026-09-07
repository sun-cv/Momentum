

using System;
using Game.Common;



namespace Game.Realm
{

    public class ComponentStore<TComponent> where TComponent : IComponent
    {
        bool[]          entity;
        TComponent[]    component;

        public ComponentStore(int capacity = 10)
        {
            entity      = new bool      [capacity];
            component   = new TComponent[capacity];
        }

        public void Add(Entity entity, TComponent component)
        {
            EnsureCapacity(entity.Index);

            this.entity   [entity.Index] = true;
            this.component[entity.Index] = component;
        }

        public TComponent View(Entity entity)
        {
            return component[entity.Index];
        }
        
        public ref TComponent Modify(Entity entity)
        {
            return ref component[entity.Index];
        }

        public bool Has(Entity entity)
        {
            return entity.Index < this.entity.Length && this.entity[entity.Index];
        }

        public void Remove(Entity entity)
        {
            if (entity.Index < this.entity.Length) this.entity[entity.Index] = false;
        }

        void EnsureCapacity(int index)
        {
            if (index < component.Length) 
                return;

            int newSize = Math.Max(component.Length * 2, index + 1);

            Array.Resize(ref entity,    newSize);
            Array.Resize(ref component, newSize);
        }
    }    
}

using System;
using Game.Common;



namespace Game.Realm
{

    public class Store<TComponent> where TComponent : IComponent
    {
        TComponent[]    component;

        public Store(int capacity = 10)
        {
            component   = new TComponent[capacity];
        }

        public void Add(Entity entity, TComponent component)
        {
            EnsureCapacity(entity.Index);

            this.component[entity.Index] = component;
        }

        public TComponent View(Entity entity)
        {
            return component[entity.Index];
        }
        
        public ref TComponent Reference(Entity entity)
        {
            return ref component[entity.Index];
        }

        public void Remove(Entity entity)
        {
            component[entity.Index] = default;
        }

        void EnsureCapacity(int index)
        {
            if (index < component.Length) 
                return;

            int newSize = Math.Max(component.Length * 2, index + 1);

            Array.Resize(ref component, newSize);
        }
    }    
}

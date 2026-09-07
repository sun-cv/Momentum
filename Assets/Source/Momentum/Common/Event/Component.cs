


namespace Game.Common.Events
{

    public readonly struct ComponentAdded<TComponent> : IEvent where TComponent : IComponent
    {
        public Entity Entity { get; init; }

        public ComponentAdded(Entity entity) 
        { 
            Entity = entity; 
        }
    }

    public readonly struct ComponentRemoved<TComponent> : IEvent where TComponent : IComponent
    {
        public Entity Entity { get; init; }

        public ComponentRemoved(Entity entity) 
        { 
            Entity = entity; 
        }
    }
}

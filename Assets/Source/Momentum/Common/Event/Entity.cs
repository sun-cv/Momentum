


namespace Game.Common.Events
{

    public readonly struct EntityCreated : IEvent
    {
        public Entity Entity { get; init; }

        public EntityCreated(Entity entity)
        {
            Entity = entity;
        }
    }

    public readonly struct EntityReleased : IEvent
    {
        public Entity Entity { get; init; }

        public EntityReleased(Entity entity)
        {
            Entity = entity;
        }
    }

}




namespace Game.Common.Events
{
    public readonly struct RegisterTimer    : IEvent
    {
        public Timer Timer                      { get; init; }
    }
    
    public readonly struct DeregisterTimer  : IEvent
    {
        public Timer Timer                      { get; init; }
    }

    public readonly struct ClearTimers      : IEvent {}

    public readonly struct CreateHitbox     : IEvent
    {
        public Entity Parent                    { get; init; }
        public string Definition                { get; init; }
    }

    public readonly struct HitboxStruck     : IEvent
    {
        public Entity Hitbox                    { get; init; }
        public Entity Target                    { get; init; }
    }
}

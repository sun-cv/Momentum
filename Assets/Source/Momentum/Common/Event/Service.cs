


namespace Game.Common.Events
{
    public readonly struct RegisterTimer    : IEvent
    {
        public Timer Timer { get; init; }
    }
    
    public readonly struct DeregisterTimer  : IEvent
    {
        public Timer Timer { get; init; }
    }
}

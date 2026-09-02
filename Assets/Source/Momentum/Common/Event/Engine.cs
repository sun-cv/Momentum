

namespace Game.Common.Events
{
    
    public readonly struct RegisterService : IEvent
    {
        public ServiceEntry ServiceEntry { get; init; }

        public RegisterService(ServiceEntry entry)
        {
            ServiceEntry = entry;
        }
    }
}


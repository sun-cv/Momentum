


namespace Game.Common.Events
{
    
    public readonly struct RegisterService : IEvent
    {

        public IService Service         { get; init; }
        public ServiceSchedule Schedule { get; init; }

        public RegisterService(IService service, ServiceSchedule schedule)
        {
            Service     = service;
            Schedule    = schedule;
        }
    }

    public readonly struct ServiceScanCompleted : IEvent {}
}

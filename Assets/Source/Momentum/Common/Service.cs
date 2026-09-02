using System;



namespace Game.Common
{

    public interface IService : IDisposable   {};

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ServiceAttribute : Attribute {  }

    [Service]
    public abstract class RegisteredService : IService
    {
        public virtual void OnDispose() {} 

        public void Dispose()
        {
            //REWORK REQUIRED DISPOSE REGISTER AND CLEAR TICK
        }
    }

    public readonly struct ServiceEntry       : IComparable<ServiceEntry>
    {
        public IService Service         { get; init; }
        public ServiceSchedule Schedule { get; init; }
        
        public int CompareTo(ServiceEntry other)
        {
            return Schedule.Phase.CompareTo(other.Schedule.Phase) != 0 ? Schedule.Phase.CompareTo(other.Schedule.Phase) : Schedule.Priority.CompareTo(other.Schedule.Priority);
        }
    }

    public readonly struct ServiceSchedule    : IComparable<ServiceSchedule>
    {
        public TickPhase Phase          { get; init; }
        public int Priority             { get; init; }

        public int CompareTo(ServiceSchedule other)
        {
            return Phase.CompareTo(other.Phase) != 0 ? Phase.CompareTo(other.Phase) : Priority.CompareTo(other.Priority);
        }
    }
}

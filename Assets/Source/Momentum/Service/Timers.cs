using System.Collections.Generic;
using Game.Common;



namespace Game.Service
{
    public class Timers : RegisteredService, IRealBase, IGameBase
    {
        private readonly List<ITimer> tickTimers  = new();
        private readonly List<ITimer> timeTimers  = new();

        void IRealBase.Tick()
        {
            foreach (var timer in new List<ITimer>(timeTimers))
            {
                timer.Tick();
            }
        }
        
        void IGameBase.Tick()
        {
            foreach (var timer in new List<ITimer>(tickTimers))
            {
                timer.Tick();
            }
        }

        public void RegisterTimer(ITimer timer)
        {
            if (timer is TimeKeeper time)
                timeTimers.Add(time);

            if (timer is TickKeeper tick)
                tickTimers.Add(tick);
        }

        public void DeregisterTimer(ITimer timer)
        {
            if (timer is TimeKeeper time)
                timeTimers.Remove(time);

            if (timer is TickKeeper tick)
                tickTimers.Remove(tick);
        }

        public void Clear()
        {
            timeTimers.Clear();
            tickTimers.Clear();
        }
    }


    public readonly struct RegisterTimer    : IEvent
    {
        public Timer Timer { get; init; }
    }
    
    public readonly struct DeregisterTimer  : IEvent
    {
        public Timer Timer { get; init; }
    }
}




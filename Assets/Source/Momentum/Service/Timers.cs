using System.Collections.Generic;
using Game.Common;
using Game.Common.Events;



namespace Game.Service
{
    public class Timers : RegisteredService, IRealBase, IGameBase
    {
        private readonly List<ITimer> realTimers  = new();
        private readonly List<ITimer> gameTimers  = new();

        public Timers()
        {
            Event.Register<Timers, ClearTimers>();
            Event.Register<Timers, RegisterTimer>();
            Event.Register<Timers, DeregisterTimer>();
        }

        void IRealBase.Tick()
        {
            ProcessRequests();

            foreach (var timer in new List<ITimer>(realTimers))
            {
                timer.Tick();
            }
        }
        
        void IGameBase.Tick()
        {
            ProcessRequests();

            foreach (var timer in new List<ITimer>(gameTimers))
            {
                timer.Tick();
            }
        }

        private void ProcessRequests()
        {
            ClearTimers();
            RegisterTimers();
            DeregisterTimers();
        }
        
        private void ClearTimers()
        {
            foreach(var _ in Event.Read<Timers, ClearTimers>())
            {
                Clear();
            }
        }

        private void RegisterTimers()
        {
            foreach(var message in Event.Read<Timers, RegisterTimer>())
            {
                Register(message.Timer);
            }
        }

        private void DeregisterTimers()
        {
            foreach(var message in Event.Read<Timers, DeregisterTimer>())
            {
                Deregister(message.Timer);
            }
        }

        private void Register(ITimer timer)
        {
            if (timer.Mode == TickMode.Real)
                realTimers.Add(timer);

            if (timer.Mode == TickMode.Game)
                gameTimers.Add(timer);
        }

        private void Deregister(ITimer timer)
        {
            if (timer.Mode == TickMode.Real)
                realTimers.Remove(timer);

            if (timer.Mode == TickMode.Game)
                gameTimers.Remove(timer);
        }

        private void Clear()
        {
            realTimers.Clear();
            gameTimers.Clear();
        }
    }
}




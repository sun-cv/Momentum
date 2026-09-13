using System;
using Game.Common.Events;



namespace Game.Common
{

    public interface ITimer 
    {
        public void Tick();
    }

    public enum TimerMode { Up, Down }

    public abstract class Timer : ITimer 
    {
        public Action OnTimerStart = delegate { };
        public Action OnTimerStop  = delegate { };

        public bool  HasRun         { get; protected set; }
        public bool  IsRunning      { get; protected set; }
        public bool  Disposed       { get; protected set; }

        public virtual void OnStart()   {}
        public virtual void OnStop()    {}
        public virtual void OnPause()   {}
        public virtual void OnCancel()  {}
        public virtual void OnResume()  {}
        public virtual void OnReset()   {}
        public virtual void OnRestart() {}

        public abstract void Tick();
        public abstract bool IsFinished { get; }

        public void Start()
        {
            OnStart();

            if (!IsRunning)
            {
                IsRunning   = true;
                HasRun      = true;
                Event.Push<RegisterTimer>(new(){ Timer = this });
                OnTimerStart.Invoke();
            }
        }

        public virtual Timer Stop()
        {
            OnStop();

            if (IsRunning)
            {
                IsRunning = false;
                Event.Push<DeregisterTimer>(new(){ Timer = this });
                OnTimerStop.Invoke();
            }
            return this;
        }


        public void Resume()
        {
            OnResume();
            IsRunning = true;
        }

        public void Pause()
        {
            OnPause();
            IsRunning = false;
        }

        public void Cancel()
        { 
            OnCancel();
            IsRunning = false; 
            Event.Push<DeregisterTimer>(new(){ Timer = this });
        }

        public void Reset()
        {
            OnReset();
            Stop();
            HasRun      = false;
        }

        public virtual void Restart()
        {
            OnRestart();
            Reset();
            Start();
        }
    }    


    public class TickKeeper : Timer
    {
        public int Starting         { get;}
        public int Initial          { get; protected set; }
        public int Current          { get; protected set; }

        public TimerMode Mode       { get; }

        public TickKeeper(TimerMode mode, int initial)
        {
            Mode        = mode;
            Starting    = initial;
            Initial     = Starting;
        }

        public override void Tick()
        {
            if (!IsRunning) 
                return;

            Current += Mode == TimerMode.Up ? 1 : -1;

            if (Mode == TimerMode.Down && Current <= 0)
                Stop();
        }

        public override void OnStart()
        {
            Current = Initial;
        }

        public override void OnReset()
        {
            Current = Initial;
        }

        public void Reset(int value)
        {
            Stop();
            Initial = value;
            HasRun  = false;
        }

        public void Restart(int value)
        {
            Reset(value);
            Start();
        }

        public override bool IsFinished => Mode == TimerMode.Up ? HasRun && !IsRunning : Current <= 0;
    }

    public class TimeKeeper: Timer
    {
        public float Starting       { get;}
        public float Initial        { get; protected set; }
        public float Current        { get; protected set; }

        public TimerMode Mode       { get; }

        public TimeKeeper(TimerMode mode, float initial)
        {
            Mode        = mode;
            Starting    = initial;
            Initial     = Starting;
        }
    
        public override void Tick()
        {
            if (!IsRunning) 
                return;

            Current += Mode == TimerMode.Up ? Config.Engine.Clock.Delta : -Config.Engine.Clock.Delta;

            if (Mode == TimerMode.Down && Current <= 0)
                Stop();
        }

        public override void OnStart()
        {
            Current = Initial;
        }

        public override void OnReset()
        {
            Current = Initial;
        }

        public void Reset(float value)
        {
            Stop();
            Initial = value;
            HasRun  = false;
        }

        public void Restart(float value)
        {
            Reset(value);
            Start();
        }

        public override bool IsFinished => Mode == TimerMode.Up ? HasRun && !IsRunning : Current <= 0;
    }
}

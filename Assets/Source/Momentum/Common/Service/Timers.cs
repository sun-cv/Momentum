using System;
using Game.Common.Events;



namespace Game.Common
{

    public interface ITimer 
    {
        public void Tick();
        public TickMode Mode    { get; }
        public TimeCount Count  { get; }
    }

    public enum TimeCount
    {
        Increment,
        Decrement,
    }

    public abstract class Timer : ITimer 
    {
        public Action OnTimerStart = delegate { };
        public Action OnTimerStop  = delegate { };

        public bool  HasRun             { get; protected set; }
        public bool  IsRunning          { get; protected set; }

        public abstract TickMode Mode   { get; protected set; }
        public abstract TimeCount Count { get; protected set; }

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
                Event.Send<RegisterTimer>(new(){ Timer = this });
                OnTimerStart.Invoke();
            }
        }

        public virtual Timer Stop()
        {
            OnStop();

            if (IsRunning)
            {
                IsRunning = false;
                Event.Send<DeregisterTimer>(new(){ Timer = this });
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
            Event.Send<DeregisterTimer>(new(){ Timer = this });
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


    public class TickCounter : Timer
    {
        public int Starting             { get; protected set; }
        public int Initial              { get; protected set; }
        public int Current              { get; protected set; }

        public override TickMode Mode   { get; protected set; }
        public override TimeCount Count { get; protected set; }

        public TickCounter(TickMode mode = TickMode.Real, TimeCount count = TimeCount.Increment, int initial = 0)
        {
            Mode        = mode;
            Count       = count;
            Initial     = initial;
            Starting    = Initial;
        }

        public override void Tick()
        {
            if (!IsRunning) 
                return;

            Current += Count == TimeCount.Increment ? 1 : -1;

            if (Count == TimeCount.Decrement && Current <= 0)
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

        public void CountDown(int value)
        {
            Count       = TimeCount.Decrement;
            Initial     = value;
            Starting    = Initial;
        }

        public override bool IsFinished => Count == TimeCount.Increment ? HasRun && !IsRunning : Current <= 0;
    }

    public class TimeCounter: Timer
    {
        public float Starting           { get; protected set; }
        public float Initial            { get; protected set; }
        public float Current            { get; protected set; }

        public override TickMode Mode   { get; protected set; }
        public override TimeCount Count { get; protected set; }

        public TimeCounter(TickMode mode = TickMode.Real, TimeCount count = TimeCount.Increment, int initial = 0)
        {
            Mode        = mode;
            Count       = count;
            Initial     = initial;
            Starting    = Initial;
        }
    
        public override void Tick()
        {
            if (!IsRunning) 
                return;

            Current += Count == TimeCount.Increment ? Config.Engine.Clock.Delta : -Config.Engine.Clock.Delta;

            if (Count == TimeCount.Decrement && Current <= 0)
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

        public void CountDown(int value)
        {
            Count       = TimeCount.Decrement;
            Initial     = value;
            Starting    = Initial;
        }

        public override bool IsFinished => Count == TimeCount.Increment ? HasRun && !IsRunning : Current <= 0;
    }

    
    public class TimeScaled : TimeCounter
    {
        TimeScaled() : base(TickMode.Game, TimeCount.Increment, 0) {}
    }

    public class TickScaled : TimeCounter
    {
        TickScaled() : base(TickMode.Game, TimeCount.Increment, 0) {}
    }
}

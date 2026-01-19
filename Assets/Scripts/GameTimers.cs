using System;
using System.Collections.Generic;
using UnityEngine;





public class Timers : RegisteredService, IServiceTick
{
    private readonly List<Timer> timers  = new();
    
    public Timers() {}

    public override void Initialize() {}

    public void Tick()
    {
        foreach (var timer in new List<Timer>(timers))
        {
            timer.Tick();
        }
    }
    public void Clear() => timers.Clear();

    public void RegisterTimer(Timer timer)   => timers.Add(timer);
    public void DeregisterTimer(Timer timer) => timers.Remove(timer);

    public UpdatePriority Priority => ServiceUpdatePriority.TimerManager;
}



public abstract class Timer : IDisposable
{   
    protected Timers timers;

    protected float initialTime;
    public float InitialTime    { get; protected set; }
    public float StartedTime    { get; protected set; }
    public float CurrentTime    { get; protected set; }

    protected int initialFrame;
    public int InitialFrame     { get; protected set; }
    public int StartedFrame     { get; protected set; }
    public int CurrentFrame     { get; protected set; }
    
    public bool  IsRunning      { get; protected set; }
    public TimerUnit Unit       { get; protected set; }

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop  = delegate { };

    protected Timer(float value)
    {
        initialTime = value;
        InitialTime = initialTime;
        Unit        = TimerUnit.Time;

        timers      = Services.Get<Timers>();
    }

    protected Timer(int value)
    {
        initialFrame = value;
        InitialFrame = initialFrame;
        Unit         = TimerUnit.Frame;

        timers       = Services.Get<Timers>();
    }

    public virtual void Start()
    {
        if (Unit == TimerUnit.Time)
        {
            CurrentTime = initialTime;
            StartedTime = Time.time;
        }

        if (Unit == TimerUnit.Frame)
        {
            CurrentFrame = initialFrame;
            StartedFrame = Clock.FrameCount;
        }
        
        if (!IsRunning)
        {
            IsRunning = true;
            timers.RegisterTimer(this);
            OnTimerStart.Invoke();
        }
    }

    public virtual Timer Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            timers.DeregisterTimer(this);
            OnTimerStop.Invoke();
        }
        return this;
    }
    
    public abstract void Tick();
    public abstract bool IsFinished { get; }

    public void Resume() => IsRunning = true;
    public void Pause()  => IsRunning = false;
    public void Cancel()  { IsRunning = false; timers.DeregisterTimer(this); }

    public virtual void Restart()
    {
        if (Unit == TimerUnit.Time)
            CurrentTime = initialTime;

        if (Unit == TimerUnit.Frame)
            CurrentFrame = initialFrame;
    }
    public virtual void Restart(float newTime)
    {
        if (Unit != TimerUnit.Time) 
            return;

        initialTime = newTime;
        Reset();
    }
    
    public virtual void Restart(int newFrame)
    {
        if (Unit != TimerUnit.Frame) 
            return;

        initialFrame = newFrame;
        Reset();
    }



    public virtual void Reset()
    {
        Stop();
        if (Unit == TimerUnit.Time)
            CurrentTime = initialTime;

        if (Unit == TimerUnit.Frame)
            CurrentFrame = initialFrame;
    }
    public virtual void Reset(float newTime)
    {
        if (Unit != TimerUnit.Time) 
            return;

        initialTime = newTime;
        Reset();
    }

    public virtual void Reset(int newFrame)
    {
        if (Unit != TimerUnit.Frame) 
            return;

        initialFrame = newFrame;
        Reset();
    }


    bool disposed;
    ~Timer()
    {
        Dispose(false);
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing) 
    {
        if (disposed)
        {
            return;
        }
        if (disposing)
        {
            timers.DeregisterTimer(this);
        }
        disposed = true;
    }
}


public enum TimerMode { Up, Down }
public enum TimerUnit { Time, Frame }

public class GenericTimer : Timer
{
    readonly TimerMode mode;

    float percent;

    public GenericTimer(TimerMode mode, float value = 0 ) : base(value)
    {
        this.mode = mode;
    }

    public GenericTimer(TimerMode mode, int frames = 0) : base(frames)
    {
        this.mode = mode;
    }

    public override void Tick()
    {
        if (!IsRunning) return;

        if (Unit == TimerUnit.Time)
            CurrentTime += mode == TimerMode.Up ? Clock.TickDelta : -Clock.TickDelta;
        
        if (Unit == TimerUnit.Time && mode == TimerMode.Down && CurrentTime <= 0)
            Stop();

        if (Unit == TimerUnit.Frame)
            CurrentFrame += mode == TimerMode.Up ? 1 : -1;

        if (Unit == TimerUnit.Frame && mode == TimerMode.Down && CurrentFrame <= 0)
            Stop();

        CalculatePercentage();
    }

    void CalculatePercentage()
    {
        float total     = Unit == TimerUnit.Time ? InitialTime : InitialFrame;
        float current   = Unit == TimerUnit.Time ? CurrentTime : CurrentFrame;
        float raw       = current / total;

        percent = mode == TimerMode.Down ? 1f - raw : raw;
    }
    public override bool IsFinished => !IsRunning;
    public float PercentComplete    => percent;
}




public class ClockWatch : GenericTimer
{
    public ClockWatch() : base(TimerMode.Up, 0f) {}
}

public class ClockTimer : GenericTimer
{
    public ClockTimer(float startValue) : base(TimerMode.Down, startValue ) {}
}

public class FrameWatch : GenericTimer
{
    public FrameWatch() : base(TimerMode.Up, 0) {}
}

public class FrameTimer : GenericTimer
{
    public FrameTimer(int startValue) : base(TimerMode.Down, startValue) {}
}


public class DualCountdown : GenericTimer
{
    public DualCountdown(int frames) : base(TimerMode.Down, frames) {}
    public DualCountdown(float time) : base(TimerMode.Down, time)   {}
}

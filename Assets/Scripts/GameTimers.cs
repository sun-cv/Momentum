using System;
using System.Collections.Generic;
using UnityEngine;


public class Timers : IServiceTick
{
    private readonly List<Timer> timers  = new();
    private readonly GamePhase phase     = GamePhase.System;

    private static Timers instance;
    public static Timers Instance => instance ??= new Timers();

    private Timers() {}

    public void Initialize() {}

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

    public GamePhase Phase => phase;
}


public abstract class Timer : IDisposable
{   
    protected float initialTime;

    public float InitialTime    { get; protected set; }
    public float StartedTime    { get; protected set; }
    public float CurrentTime    { get; protected set; }
    public bool  IsRunning      { get; protected set; }
    public float Percent => initialTime > 0 ? Mathf.Clamp01(CurrentTime / initialTime) : 1f;

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop  = delegate { };

    protected Timer(float value)
    {
        initialTime = value;
    }

    public virtual void Start()
    {
        CurrentTime = initialTime;
        StartedTime = Time.time;
        
        if (!IsRunning)
        {
            IsRunning = true;
            Timers.Instance.RegisterTimer(this);
            OnTimerStart.Invoke();
        }
    }

    public virtual Timer Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            Timers.Instance.DeregisterTimer(this);
            OnTimerStop.Invoke();
        }
        return this;
    }
    
    public abstract void Tick();
    public abstract bool IsFinished { get; }

    public void Resume() => IsRunning = true;
    public void Pause()  => IsRunning = false;
    public void Cancel()  { IsRunning = false; Timers.Instance.DeregisterTimer(this); }


    public virtual void Reset() => CurrentTime = initialTime;
    public virtual void Reset(float newTime)
    {
        initialTime = newTime;
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
            Timers.Instance.DeregisterTimer(this);
        }
        disposed = true;
    }
}


public class Countdown : Timer
{
    public Countdown(float value) : base(value) {}

        public override void Tick()
        {
            if (IsRunning && CurrentTime > 0)
            {
                CurrentTime -= Clock.TickDelta;
            }
            if (IsRunning && CurrentTime <= 0)
            {
                Stop();
            }
        }

    public override bool IsFinished => CurrentTime <= 0;
}


public class Stopwatch : Timer
{
    private readonly List<float> lapTimes = new();

    public IReadOnlyList<float> LapTimes => lapTimes;

    public Stopwatch() : base(0) {}

    public override void Tick()
    {
        if (IsRunning)
        {
            CurrentTime -= Clock.TickDelta;
        }
    }

    public override bool IsFinished => !IsRunning;
    public float LastLap => lapTimes.Count > 0 ? lapTimes[^1] : 0;
    
    public void Lap()
    {
        lapTimes.Add(CurrentTime);
    }

    public void ClearLaps()
    {
        lapTimes.Clear();
    }

    public override void Reset()
    {
        base.Reset();
        ClearLaps();
        CurrentTime = 0;
    }
}


public class ProgressTimer : Timer
{
    public ProgressTimer(float value) : base(value){}

    public override void Start()
    {
        CurrentTime = 0;

        if (!IsRunning)
        {
            IsRunning = true;
            Timers.Instance.RegisterTimer(this);
            OnTimerStart.Invoke();
        }
    }

    public override void Tick()
    {
        if (IsRunning)
        {
            CurrentTime -= Clock.TickDelta;
        }
        if (Percent >= 1)
        {
            Stop();
            Reset();
        }
    }

    public override void Reset() => CurrentTime = 0;
    public override void Reset(float newTime)
    {
        initialTime = newTime;
        Reset();
    }

    public override bool IsFinished => !IsRunning;
    public bool Complete            => IsFinished;
}


public class Frames : Countdown
{
    public Frames(int frames) : base(frames * Clock.TickDelta) {}
}

public class FrameCount : Timer
{
    
    public int CurrentFrame { get; protected set; }
    public int StartFrame   { get; protected set; }
    public int CountLimit = 60 * 60 * 10; // 10

    public FrameCount() : base(0) { }

    public override void Start()
    {
        StartFrame = Clock.FrameCount;
        
        if (!IsRunning)
        {
            IsRunning = true;
            Timers.Instance.RegisterTimer(this);
            OnTimerStart.Invoke();
        }
    }

    public override void Tick()
    {
        if (IsRunning)
            CurrentFrame++;
        
        if (CurrentFrame > CountLimit)
            Stop();
    }

    public override void Reset()    => CurrentFrame = 0;    
    public override bool IsFinished => !IsRunning;
    public float ElapsedSeconds     => CurrentFrame * Clock.TickDelta;

}
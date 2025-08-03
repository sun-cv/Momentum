using System;
using UnityEngine;

namespace Momentum
{


public abstract class Timer : IDisposable
{   
    protected float initialTime;

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

        if (!IsRunning)
        {
            IsRunning = true;
            TimerManager.RegisterTimer(this);
            OnTimerStart.Invoke();
        }


    }

    public virtual void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            TimerManager.DeregisterTimer(this);
            OnTimerStop.Invoke();
        }
    }
    
    public abstract void Tick();
    public abstract bool IsFinished { get; }

    public void Resume() => IsRunning = true;
    public void Pause()  => IsRunning = false;
    public void Cancel()  { IsRunning = false; TimerManager.DeregisterTimer(this); }


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
            TimerManager.DeregisterTimer(this);
        }
        disposed = true;
    }
}
}
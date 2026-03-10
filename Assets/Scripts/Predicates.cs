using System;



public class LivePredicate : Service, IServiceLoop
{
    private readonly Func<bool> evaluator;

        // -----------------------------------

    private bool value;
    private bool autoUpdateEnabled = true;

    // ===============================================================================

    public LivePredicate(Func<bool> evaluator)
    {
        Services.Lane.Register(this);

        this.evaluator  = evaluator;
        value           = evaluator();
    }

    // ===============================================================================

    public void Loop()
    {
        if (autoUpdateEnabled)
            value = evaluator();
    }

    // ===============================================================================

    public void SetManual(bool manualValue)
    {
        autoUpdateEnabled = false;
        value = manualValue;
    }

    public void ResumeAuto()
    {
        autoUpdateEnabled = true;
        value = evaluator();
    }

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    // ===============================================================================


    public static implicit operator bool(LivePredicate b) => b.value;
    public bool IsAuto  => autoUpdateEnabled;
    public bool Value   => value;
    public UpdatePriority Priority => ServiceUpdatePriority.SystemLoop;
}


public class LazyPredicate
{
    private readonly Func<bool> evaluator;
    
    private bool cachedValue;
    private bool autoUpdateEnabled = true;

    // ===============================================================================

    public LazyPredicate(Func<bool> evaluator)
    {
        this.evaluator  = evaluator;
        cachedValue     = evaluator();
    }

    // ===============================================================================

    public void SetManual(bool manualValue)
    {
        autoUpdateEnabled = false;
        cachedValue = manualValue;
    }

    public void ResumeAuto()
    {
        autoUpdateEnabled = true;
    }

    // ===============================================================================


    public static implicit operator bool(LazyPredicate b) => b.Value;
    public bool IsAuto => autoUpdateEnabled;
    public bool Value
    {
        get
        {
            if (autoUpdateEnabled)
                cachedValue = evaluator();

            return cachedValue;
        }
    }

}


public class TimePredicate : Service, IServiceLoop
{
    private readonly bool           resetOnFalse;

        // -----------------------------------

    private readonly Func<bool>     condition;

    private readonly TimerUnit unit;

    private readonly ClockWatch     clock;
    private readonly FrameWatch     frame;
        // -----------------------------------

    private bool value              = false;

    // ===============================================================================

    public TimePredicate(TimerUnit unit, Func<bool> condition, bool resetOnFalse = true)
    {
        Services.Lane.Register(this);

        this.unit           = unit;
        this.condition      = condition;
        this.resetOnFalse   = resetOnFalse;
        this.clock          = new ClockWatch();
        this.frame          = new FrameWatch();
    }

    // ===============================================================================

    public void Loop()
    {
        bool currentCondition = condition();

        if (currentCondition && !value)
        {
            value = true;
            StartTimers();
        }
        else if (!currentCondition && value)
        {
            value = false;

            if (resetOnFalse)
                ResetTimers();
        }
    }

    private void StartTimers()
    {
        if (UseClock) clock.Start();
        if (UseFrame) frame.Start();
    }

    private void ResetTimers()
    {
        if (UseClock) clock.Reset();
        if (UseFrame) frame.Reset();
    }

    private bool UseClock => unit == TimerUnit.Time || unit == TimerUnit.TimeAndFrame;
    private bool UseFrame => unit == TimerUnit.Frame || unit == TimerUnit.TimeAndFrame;

    // ===============================================================================

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public static implicit operator bool(TimePredicate time) => time.value;
    public bool Value               => value;
    public float Duration           => clock.CurrentTime;
    public int Frame                => frame.CurrentFrame;
    public ClockWatch ClockWatch    => clock;
    public FrameWatch FrameWatch    => frame;
    
    public UpdatePriority Priority => ServiceUpdatePriority.SystemLoop;
}





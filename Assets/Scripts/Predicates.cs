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

        // -----------------------------------

    private bool value              = false;
    private readonly ClockWatch     timer;

    // ===============================================================================

    public TimePredicate(Func<bool> condition, bool resetOnFalse = true)
    {
        Services.Lane.Register(this);

        this.condition      = condition;
        this.resetOnFalse   = resetOnFalse;
        this.timer          = new ClockWatch();
    }

    // ===============================================================================

    public void Loop()
    {
        bool currentCondition = condition();

        if (currentCondition && !value)
        {
            value = true;
            timer.Start();
        }
        else if (!currentCondition && value)
        {
            value = false;
            
            if (resetOnFalse)
                timer.Reset();
        }
    }

    // ===============================================================================

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    public static implicit operator bool(TimePredicate b) => b.value;
    public bool Value       => value;
    public float Duration   => timer.CurrentTime;
    public ClockWatch Timer => timer;
    
    public UpdatePriority Priority => ServiceUpdatePriority.SystemLoop;
}



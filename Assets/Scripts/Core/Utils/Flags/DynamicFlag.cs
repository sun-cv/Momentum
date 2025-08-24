
using System;


namespace Momentum
{


public class DynamicFlag

{
    private bool        value;
    private bool        autoUpdateEnabled = true;

    private readonly    Func<bool> autoUpdateFunction;

    private WeakSubscriber<Tick> subscription;

    public DynamicFlag (Func<bool> autoUpdateFunction)
    {
        value                   = autoUpdateFunction();
        this.autoUpdateFunction = autoUpdateFunction;
        
        subscription = new WeakSubscriber<Tick>(GameTickBinding.Tick, Tick, this);
    }

    public void Tick()
    {
        if (autoUpdateEnabled)
        {
            value = autoUpdateFunction();
        }
    }

    public void Override(bool _overrideValue)
    {
        autoUpdateEnabled   = false;
        value               = _overrideValue;
    }

    public void Resume()
    {
        autoUpdateEnabled   = true;
    }

    public static implicit operator bool(DynamicFlag flag) => flag.value;
    public bool Value => value;
}
}
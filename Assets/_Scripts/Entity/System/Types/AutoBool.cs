
using System;
using Momentum.Events;
using Momentum.GameLoop;

namespace Momentum.Markers
{


public class AutoBool 
{
    private bool        value;
    private bool        autoUpdateEnabled = true;

    private readonly    Func<bool> autoUpdateFunction;

    public AutoBool (Func<bool> autoUpdateFunction)
    {
        value                   = autoUpdateFunction();
        this.autoUpdateFunction = autoUpdateFunction;
        
        GameTickBinding.Tick.Add(Update);
    }

    public void Update()
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

    public static implicit operator bool(AutoBool flag) => flag.value;
}
}
using System;

public class FlagManaged 
{
    private bool        value;
    private bool        autoUpdateEnabled = true;

    private readonly    Func<bool> autoUpdateFunction;

    public FlagManaged (Func<bool> _autoUpdateFunction)
    {
        value               = _autoUpdateFunction();
        autoUpdateFunction  = _autoUpdateFunction;
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

    public static implicit operator bool(FlagManaged flag) => flag.value;
}
using UnityEngine;


public class FlagStatus
{
    public bool value           { get; private set; } = false;
    public float LastSet        { get; private set; }

    public void Set()
    {
        value   = true;
        LastSet = Time.time;
    }

    public void Clear()
    {
        value = false;
    }

    public static implicit operator bool(FlagStatus flag) => flag.value;
}
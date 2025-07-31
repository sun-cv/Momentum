using UnityEngine;


namespace Momentum.Markers
{

    public class StatusFlag
    {
        public bool Value           { get; private set; } = false;
        public float LastSet        { get; private set; }
    
        public void Set()
        {
            Value   = true;
            LastSet = Time.time;
        }
    
        public void Clear()
        {
            Value = false;
        }
    
        public static implicit operator bool(StatusFlag flag) => flag.Value;
    }
}
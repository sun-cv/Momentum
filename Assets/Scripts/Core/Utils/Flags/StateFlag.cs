using UnityEngine;

namespace Momentum
{


    public class StateFlag : Flag
    {
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
    }




}
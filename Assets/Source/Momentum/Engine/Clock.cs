using System;
using UnityEngine;


namespace Game.Engine
{
    internal class Clock
    {

        private const float tickRate    = Config.Engine.TICK_RATE; 
        private const float loopRate    = Config.Engine.LOOP_RATE; 
        private const float utilRate    = Config.Engine.UTIL_RATE; 
        
        private const float tickDelta   = 1 / tickRate; 
        private const float loopDelta   = 1 / loopRate; 
        private const float utilDelta   = 1 / utilRate; 

        private float tickAccumulator;
        private float loopAccumulator;
        private float utilAccumulator;

        private int   tick;
        private float time;

        private bool tickFired;
        private bool loopFired;
        private bool utilFired;

        public event Action OnTick;
        public event Action OnLoop;
        public event Action OnUtil;

        public event Action OnLate;

        public Clock()
        {
            Time.fixedDeltaTime = Delta;
        }

        public void Tick()
        {
            time            += Delta;

            tickAccumulator += Delta;
            loopAccumulator += Delta;
            utilAccumulator += Delta;

            tickFired       = false;
            loopFired       = false;
            utilFired       = false;

            while (tickAccumulator >= tickDelta) { tickAccumulator -= tickDelta; tickFired = true; tick++; }
            while (loopAccumulator >= loopDelta) { loopAccumulator -= loopDelta; loopFired = true; }
            while (utilAccumulator >= utilDelta) { utilAccumulator -= utilDelta; utilFired = true; }
            
            if (loopFired) OnLoop?.Invoke();
            if (tickFired) OnTick?.Invoke();
            if (utilFired) OnUtil?.Invoke();
        }

        public void Late()
        {
            OnLate?.Invoke();
        }
    
        public void Dispose()
        {

        }

        public float Delta => tickDelta;
    }
}




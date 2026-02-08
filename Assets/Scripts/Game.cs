using System;
using System.Collections.Generic;
using UnityEngine;





public class Game : MonoBehaviour
{
    private GameEngine engine;

    public void Awake()
    {
        engine = new();
        engine.Startup();
    }

    public void FixedUpdate()
    {
        engine.Tick();
    }

    public void LateUpdate()
    {
        engine.Late();
    }

    public void OnDisable()
    {
        engine.Shutdown();
    }
}




public class GameEngine
{
    readonly Clock   clock          = new();
    readonly GameLoop loop          = new();

    public void Startup()
    {
        Services.Start();

        Initialize();
    }

    public void Initialize()
    {
        Assets  .Initialize();

        Services.Initialize();
        Services.Bind();

        loop    .Initialize(clock);
    }

    public void Tick()
    {
        clock.Tick();
    }

    public void Late()
    {
        clock.Late();
    }

    public void Shutdown()
    {
        Services.Dispose();   
    }
}



public class Clock
{
    public const float TickRate     = Config.Timing.TICK_RATE_TICK;
    public const float LoopRate     = Config.Timing.TICK_RATE_LOOP;
    public const float StepRate     = Config.Timing.TICK_RATE_STEP;
    public const float UtilRate     = Config.Timing.TICK_RATE_UTIL;

    public const float TickDelta = 1f/TickRate;
    public const float LoopDelta = 1f/LoopRate;
    public const float StepDelta = 1f/StepRate;
    public const float UtilDelta = 1f/UtilRate;

    public event Action OnTick;
    public event Action OnLoop;
    public event Action OnStep;
    public event Action OnUtil;
    public event Action OnLate;

    float tickAccumulator; 
    float loopAccumulator; 
    float stepAccumulator; 
    float utilAccumulator; 

    static int frameCount;
    
    public static int FrameCount    => frameCount;
    public static float DeltaTime   => UnityEngine.Time.fixedDeltaTime;
    public static float Time        => UnityEngine.Time.time;

    public void Tick()
    {

        tickAccumulator += DeltaTime;
        loopAccumulator += DeltaTime;
        stepAccumulator += DeltaTime;
        utilAccumulator += DeltaTime;

        while (tickAccumulator >= TickDelta)
        {
            tickAccumulator -= TickDelta;
            frameCount++;            
            OnTick?.Invoke();
        }
        while (loopAccumulator >= LoopDelta)
        {
            loopAccumulator -= LoopDelta;
            OnLoop?.Invoke();
        }
        while (stepAccumulator >= StepDelta)
        {
            stepAccumulator -= StepDelta;
            OnStep?.Invoke();
        }
        while (utilAccumulator >= UtilDelta)
        {
            utilAccumulator -= UtilDelta;
            OnUtil?.Invoke();
        }
    }

    public void Late()
    {
        OnLate?.Invoke();
    }
}




public class GameLoop
{
    private Clock clock;

    static int   tickHerz;
    static float timeHerz;

    public void Initialize(Clock clock)
    {
        this.clock = clock;

        this.clock.OnTick += MeasureTickRate;

        this.clock.OnTick += Tick;
        this.clock.OnLoop += Loop;
        this.clock.OnStep += Step;
        this.clock.OnUtil += Util;
        this.clock.OnLate += Late;

        Time.fixedDeltaTime = Clock.TickDelta;
    }

    public void Tick()
    {
        Services.Lane.Tick();
    }
    public void Loop()
    {
        Services.Lane.Loop();
    }
    public void Step()
    {
        Services.Lane.Step();
    }
    public void Util()
    {
        Services.Lane.Util();
    }
    public void Late()
    {
        Services.Lane.Late();
    }

    static void MeasureTickRate()
    {
        timeHerz += Clock.DeltaTime;
        tickHerz++;
        if (timeHerz >= 1f)
        {
            Logging.For(LogSystem.Engine).Debug("Tick Rate", () => tickHerz / timeHerz);
            timeHerz = 0f;
            tickHerz = 0;
        }
    }

    
}



public enum UpdatePhase
{
    System,
    Input,
    Logic,
    Physics,
    Resolve,
    Render
}


public readonly struct UpdatePriority : IComparable<UpdatePriority>
{
    public UpdatePhase Phase { get; }
    public int Priority { get; }

    public UpdatePriority(UpdatePhase phase, int priority = 50)
    {
        Phase    = phase;
        Priority = priority;
    }

    public int CompareTo(UpdatePriority other)
    {
        int phaseCompare = Phase.CompareTo(other.Phase);
        return phaseCompare != 0 ? phaseCompare : Priority.CompareTo(other.Priority);
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;



public class Game : MonoBehaviour
{
    private GameEngine engine;

    // ===============================================================================
    
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

    // ===============================================================================

    public void Startup()
    {
        Definitions .Start();
        Factories   .Start();
        Services    .Start();

        Initialize();
    }

    public void Initialize()
    {
        Assets      .Initialize();
    
        Services    .Initialize();
        Services    .Bind();
    
        loop        .Initialize(clock);
    }

    // ===============================================================================
    
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
    public const float TickRate = Config.Timing.TICK_RATE_TICK;
    public const float LoopRate = Config.Timing.TICK_RATE_LOOP;
    public const float StepRate = Config.Timing.TICK_RATE_STEP;
    public const float UtilRate = Config.Timing.TICK_RATE_UTIL;

        // -----------------------------------

    public const float TickDelta = 1f / TickRate;
    public const float LoopDelta = 1f / LoopRate;
    public const float StepDelta = 1f / StepRate;
    public const float UtilDelta = 1f / UtilRate;

        // -----------------------------------

    public event Action OnTick;
    public event Action OnLate;

        // -----------------------------------

    public int TickFired { get; private set; }
    public int LoopFired { get; private set; }
    public int StepFired { get; private set; }
    public int UtilFired { get; private set; }

        // -----------------------------------

    float tickAccumulator;
    float loopAccumulator;
    float stepAccumulator;
    float utilAccumulator;

    static int frameCount;

    // ===============================================================================

    public void Tick()
    {
        tickAccumulator += DeltaTime;
        loopAccumulator += DeltaTime;
        stepAccumulator += DeltaTime;
        utilAccumulator += DeltaTime;

        TickFired = 0;
        LoopFired = 0;
        StepFired = 0;
        UtilFired = 0;

        while (tickAccumulator >= TickDelta) { tickAccumulator -= TickDelta; frameCount++; TickFired++; }
        while (loopAccumulator >= LoopDelta) { loopAccumulator -= LoopDelta; LoopFired++; }
        while (stepAccumulator >= StepDelta) { stepAccumulator -= StepDelta; StepFired++; }
        while (utilAccumulator >= UtilDelta) { utilAccumulator -= UtilDelta; UtilFired++; }

        OnTick?.Invoke();
    }

    // ===============================================================================

    public void Late()
    {
        OnLate?.Invoke();
    }

    // ===============================================================================

    public static int   FrameCount => frameCount;
    public static float DeltaTime  => UnityEngine.Time.fixedDeltaTime;
    public static float Time       => UnityEngine.Time.time;
}



public class GameLoop
{
    private enum LaneType { Tick, Loop, Step, Util }

        // -----------------------------------

    private Clock clock;

        // -----------------------------------

    static int   tickHerz;
    static float timeHerz;

        // -----------------------------------

    readonly List<ExecutionEntry> due = new();

    // ===============================================================================

    public void Initialize(Clock clock)
    {
        this.clock         = clock;
        this.clock.OnTick += MeasureTickRate;
        this.clock.OnTick += Execute;
        this.clock.OnLate += Late;

        Time.fixedDeltaTime = Clock.TickDelta;
    }

    // ===============================================================================

    void Execute()
    {
        due.Clear();

        CollectDue(clock.TickFired,  Services.TickServices,  LaneType.Tick);
        CollectDue(clock.LoopFired,  Services.LoopServices,  LaneType.Loop);
        CollectDue(clock.StepFired,  Services.StepServices,  LaneType.Step);
        CollectDue(clock.UtilFired,  Services.UtilServices,  LaneType.Util);

        due.Sort();

        foreach (var entry in due)
            entry.Execute();
    }

    void CollectDue<T>(int fires, List<T> services, LaneType lane) where T : IService, IServicePriority
    {
        if (fires == 0) return;

        foreach (var service in services)
        {
            if (service.IsEnabled) due.Add(new ExecutionEntry(service.Priority, service, lane));
        }
    }

    void Late()
    {
        Services.Lane.Late();
    }

    // ===============================================================================

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

    private readonly struct ExecutionEntry : IComparable<ExecutionEntry>
    {
        public readonly UpdatePriority Priority;
        public readonly IService Service;
        public readonly LaneType Lane;

        public ExecutionEntry(UpdatePriority priority, IService service, LaneType lane)
        {
            Priority = priority;
            Service  = service;
            Lane     = lane;
        }

        public void Execute()
        {
            switch (Lane)
            {
                case LaneType.Tick: ((IServiceTick)Service).Tick(); break;
                case LaneType.Loop: ((IServiceLoop)Service).Loop(); break;
                case LaneType.Step: ((IServiceStep)Service).Step(); break;
                case LaneType.Util: ((IServiceUtil)Service).Util(); break;
            }
        }

        public int CompareTo(ExecutionEntry other) => Priority.CompareTo(other.Priority);
    }
}

// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum UpdatePhase
{
    System,
    Input,
    Logic,
    Physics,
    Resolve,
    Render
}

        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        //                                 Structs                                                   
        // ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
        
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


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
        Registry.Initialize();
        Services.Initialize();
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
    public const float TickRate     = Config.TICK_RATE_TICK;
    public const float LoopRate     = Config.TICK_RATE_LOOP;
    public const float StepRate     = Config.TICK_RATE_STEP;
    public const float UtilRate     = Config.TICK_RATE_UTIL;

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
    public static float DeltaTime   => Time.deltaTime;

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

    public void Initialize(Clock clock)
    {
        this.clock = clock;

        this.clock.OnTick += Tick;
        this.clock.OnLoop += Loop;
        this.clock.OnStep += Step;
        this.clock.OnUtil += Util;
        this.clock.OnLate += Late;

        Time.fixedDeltaTime  = Clock.TickDelta;
    }

    public void Tick()
    {
        GameTick.Tick();
    }
    public void Loop()
    {
        GameTick.Loop();
    }
    public void Step()
    {
        GameTick.Step();
    }
    public void Util()
    {
        GameTick.Util();
    }
    public void Late()
    {
        GameTick.Late();
    }
}




public static class GameTick
{
    private static readonly List<IServiceTick> tickServices         = new();
    private static readonly List<IServiceLoop> loopServices         = new();
    private static readonly List<IServiceStep> stepServices         = new();
    private static readonly List<IServiceUtil> utilServices         = new();
    private static readonly List<IServiceLate> lateServices         = new();

    private static readonly List<IService> pendingRegistrations     = new();
    private static readonly List<IService> pendingDeregistrations   = new();

    static int   tickHerz;
    static float timeHerz;

    public static void Tick()
    {
        ProcessPending();
        MeasureTickRate();
        
        foreach(var service in tickServices)
            service.Tick();
    }
    
    public static void Loop()
    {        
        foreach(var service in loopServices)
            service.Loop();
    }
    
    public static void Step()
    {        
        foreach(var service in stepServices)
            service.Step();
    }
    
    public static void Util()
    {        
        foreach(var service in utilServices)
            service.Util();
    }

    public static void Late()
    {
        foreach(var service in lateServices)
            service.Late();
    }

    private static void ProcessPending()
    {
        foreach (var service in pendingDeregistrations)
        {
            if (service is IServiceTick ServiceTick) tickServices.Remove(ServiceTick);
            if (service is IServiceLoop ServiceLoop) loopServices.Remove(ServiceLoop);
            if (service is IServiceStep ServiceStep) stepServices.Remove(ServiceStep);
            if (service is IServiceUtil ServiceUtil) utilServices.Remove(ServiceUtil);
            if (service is IServiceLate ServiceLate) lateServices.Remove(ServiceLate);
        }

        pendingDeregistrations.Clear();

        foreach (var service in pendingRegistrations)
        {
            if (service is IServiceTick tickService)
            {
                tickServices.Add(tickService);
                tickServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            if (service is IServiceLoop loopService)
            {
                loopServices.Add(loopService);
                loopServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            if (service is IServiceStep stepService)
            {
                stepServices.Add(stepService);
                stepServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            if (service is IServiceUtil utilService)
            {
                utilServices.Add(utilService);
                utilServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            if (service is IServiceLate lateService)
            {
                lateServices.Add(lateService);
                lateServices.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
        }
        pendingRegistrations.Clear();
    }

    private static void MeasureTickRate()
    {
        timeHerz += Clock.DeltaTime;
        tickHerz++;

        if (timeHerz >= 1f)
        {
            Log.Debug(LogSystem.Engine, LogCategory.State, "Engine", "Tick Rate", () => tickHerz / timeHerz);

            timeHerz = 0f;
            tickHerz = 0;
        }
    }

    public static void Register(IService service)
    {
        pendingRegistrations.Add(service);
    }

    public static void Deregister(IService service)
    {
        pendingDeregistrations.Add(service);
    }
}


public static class Services
{
    public static void Initialize()
    {
        var services = Registry.Services.RegisteredServices.Values;

        foreach (var service in services)
        {
            if (service is IInitialize instance)
                instance.Initialize();
        }
    }

    public static void Dispose()
    {
        var services = Registry.Services.RegisteredServices;

        foreach (var service in services.Values)
        {
            if (service is IDisposable instance)
                instance.Dispose();
        }
    }

    public static T Get<T>() => Registry.Services.Get<T>();
    public static void Register<T>(T service) => Registry.Services.Register<T>(service);
    public static void RegisterTick(IService service) => GameTick.Register(service);
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


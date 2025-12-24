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

    public void OnDisable()
    {
        engine.Shutdown();
    }

}





public class GameEngine
{
    readonly Clock   clock  = new();
    readonly GameLoop loop  = new();

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

    public const float TickDuration = 1f/TickRate;
    public const float LoopDuration = 1f/LoopRate;
    public const float StepDuration = 1f/StepRate;
    public const float UtilDuration = 1f/UtilRate;

    public event Action OnTick;
    public event Action OnLoop;
    public event Action OnStep;
    public event Action OnUtil;

    float accumulator1; 
    float accumulator2; 
    float accumulator3; 
    float accumulator4; 

    static int frameCount;

    public Clock() => Time.fixedDeltaTime = TickDuration;
    
    public static int FrameCount    => frameCount;
    public static float DeltaTime   => TickDuration;

    public void Tick()
    {
        accumulator1 += TickDuration;
        accumulator2 += TickDuration;
        accumulator3 += TickDuration;
        accumulator4 += TickDuration;

        while (accumulator1 >= TickDuration)
        {
            accumulator1 -= TickDuration;
            frameCount++;            
            OnTick?.Invoke();
        }
        while (accumulator2 >= LoopDuration)
        {
            accumulator2 -= LoopDuration;
            OnLoop?.Invoke();
        }
        while (accumulator3 >= StepDuration)
        {
            accumulator3 -= StepDuration;
            OnStep?.Invoke();
        }
        while (accumulator4 >= UtilDuration)
        {
            accumulator4 -= UtilDuration;
            OnUtil?.Invoke();
        }
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
}




public static class GameTick
{
    
    private static readonly List<IServiceTick> tickServices = new();
    private static readonly List<IServiceLoop> loopServices = new();
    private static readonly List<IServiceStep> stepServices = new();
    private static readonly List<IServiceUtil> utilServices = new();

    public static void Tick()
    {
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

    public static void Register(IService service)
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
    }
    public static void Deregister(IService service)
    {
        if (service is IServiceTick st)   tickServices.Remove(st);
        if (service is IServiceLoop sl)   loopServices.Remove(sl);
        if (service is IServiceStep ss)   stepServices.Remove(ss);
        if (service is IServiceUtil su)   utilServices.Remove(su);
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
    PreUpdate,
    Update,
    PostUpdate
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


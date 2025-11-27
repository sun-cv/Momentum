using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Game : MonoBehaviour
{
    private GameEngine engine;

    public void Awake()
    {
        engine = new();
        engine.Startup();
    }

    public void Start()
    {
        engine.Start();
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
    public readonly Runtime runtime     = new();

    public HeroController controller;
    public bool triggered;

    public void Startup()
    {
        runtime .Initialize();
        Service .Initialize();
        Registry.Initialize();
    }

    public void Start()
    {
        Register();
        controller = HeroFactory.Create();
    }

    private void Register()
    {
        Service.Register(Timers.Instance);
    }

    public void Tick()
    {
        runtime.Tick();
        Logwin.Log("MaxHealth", controller.Hero.MaxHealth);
        Logwin.Log("Health", controller.Hero.Health);

        var button = Registry.Service.Get<PlayerInput>().ActiveInput.FirstOrDefault(b => b.Input == InputIntent.Attack2);

        if ( button != null && !triggered)
        {
            triggered = true;
            controller.Hero.Stats.Mediator.AddModifier(new BasicStatModifier("MaxHealth", 120, (value) => value + 20));
            
            Function.Call("ApplyDamage", new { victim = controller.Hero, damage = 10});
            Function.Call("CalculateDamage", new { victim = controller.Hero, attacker = controller.Hero, args = new { Name = "test"}});
        }

        if (controller.Hero.MaxHealth == 20)
            triggered = false;
    }

    public void Shutdown()
    {
        Service.Dispose();   
    }
}

public class Runtime
{
    Clock clock = new();
    GameLoop  loop  = new();

    public void Initialize()
    {
        loop.Initialize(clock);
    }

    public void Tick()
    {
        clock.Tick();
    }

}


public class Clock
{
    public const float TickRate     = GameSettings.TICK_RATE_TICK;
    public const float LoopRate     = GameSettings.TICK_RATE_LOOP;
    public const float StepRate     = GameSettings.TICK_RATE_STEP;
    public const float UtilRate     = GameSettings.TICK_RATE_UTIL;

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
    public static float TickDelta   => TickDuration;

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

    public void Start()
    {
        Service.Initialize();
    }

    public void Tick()
    {
        Service.Tick();
    }
    public void Loop()
    {
        Service.Loop();
    }
    public void Step()
    {
        Service.Step();
    }
    public void Util()
    {
        Service.Util();
    }
}


public enum GamePhase
{
    System,
    Input,
    Stats,
    Combat,
}

public interface IService { GamePhase Phase { get; } public void Initialize(); };
public interface IServiceTick : IService { public void Tick(); };
public interface IServiceLoop : IService { public void Loop(); };
public interface IServiceStep : IService { public void Step(); };
public interface IServiceUtil : IService { public void Util(); };

public static class Service
{
    
    private static readonly List<IService> services = new();

    public static void Tick()
    {
        foreach(var service in services)
            if (service is IServiceTick registered)
                registered.Tick();
    }
    public static void Loop()
    {
        foreach(var service in services)
            if (service is IServiceLoop registered)
                registered.Loop();
    }
    public static void Step()
    {
        foreach(var service in services)
            if (service is IServiceStep registered)
                registered.Step();
    }
    public static void Util()
    {
        foreach(var service in services)
            if (service is IServiceUtil registered)
                registered.Util();
    }

    public static void Register(IService service)
    {
        if (!services.Contains(service))
        {
            services.Add(service);
            services.Sort((a, b) => a.Phase.CompareTo(b.Phase));
        }
        else
            Debug.LogWarning($"GameService: Attempted to register {service.GetType().Name}, but it was already registered.");
    }

    public static void Deregister(IService service)
    {
        if (services.Contains(service))
            services.Remove(service);
        else
            Debug.LogWarning($"GameService: Attempted to deregister {service.GetType().Name}, but it was not registered.");
    }

    public static void Initialize()
    {
        foreach (var service in services)
            service.Initialize();
    }

    public static void Dispose()
    {
        foreach (var service in services)
            if (service is IDisposable disposable)
                disposable.Dispose();
    }
}



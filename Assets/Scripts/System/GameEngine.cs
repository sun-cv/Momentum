using System;
using System.Collections.Generic;
using UnityEngine;


public class GameEngine : MonoBehaviour
{
    private GameClock clock = new();
    private GameLoop loop   = new();

    public void Awake()
    {
        loop.Initialize(clock);
    }

    public void Start()
    {
        GameService.Initialize();
    }

    public void FixedUpdate()
    {
        clock.Tick();
    }


    public GameClock Clock => clock;
}


public class GameClock
{
    public const float TickRate = GameSettings.TickRate;
    public const float LoopRate = GameSettings.LoopRate;
    public const float StepRate = GameSettings.StepRate;
    public const float UtilRate = GameSettings.UtilRate;

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

    public void Tick()
    {
        accumulator1 += Time.fixedDeltaTime;
        accumulator2 += Time.fixedDeltaTime;
        accumulator3 += Time.fixedDeltaTime;
        accumulator4 += Time.fixedDeltaTime;

        while (accumulator1 >= TickDuration)
        {
            accumulator1 -= TickDuration;
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
    private GameClock clock;

    public void Initialize(GameClock clock)
    {
        this.clock = clock;

        this.clock.OnTick += Tick;
        this.clock.OnLoop += Loop;
        this.clock.OnStep += Step;
        this.clock.OnUtil += Util;
    }

    public void Start()
    {
        GameService.Initialize();
    }

    public void Tick()
    {
        GameService.Tick();
    }
    public void Loop()
    {
        GameService.Loop();
    }
    public void Step()
    {
        GameService.Step();
    }
    public void Util()
    {
        GameService.Util();
    }
}



public interface IService { GamePhase Phase { get; } public void Initialize(); };
public interface IServiceTick : IService { public void Tick(); };
public interface IServiceLoop : IService { public void Loop(); };
public interface IServiceStep : IService { public void Step(); };
public interface IServiceUtil : IService { public void Util(); };


public static class GameService
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
}


public static class GameRegistry
{
    public static readonly Dictionary<string, EntityData> entity = new();
    public static readonly Dictionary<string, WeaponData> weapon = new();

    public static readonly Dictionary<string, GameObject> prefab = new();

    static GameRegistry()
    {
        LoadData();
        LoadPrefab();
    }

    private static void LoadData()
    {
        foreach (var data in Resources.LoadAll<EntityData>("Data/Entity"))
            entity[data.name] = data;

        foreach (var data in Resources.LoadAll<WeaponData>("Data/Weapon/"))
            weapon[data.name] = data;
    }

    private static void LoadPrefab()
    {
        foreach (var data in Resources.LoadAll<GameObject>("Prefab/Entity"))
            prefab[data.name] = data;
    }

    public static EntityData GetEntity(string name) => entity.TryGetValue(name, out var value) ? value : null;
    public static WeaponData GetWeapon(string name) => weapon.TryGetValue(name, out var value) ? value : null;
    public static GameObject GetPrefab(string name) => prefab.TryGetValue(name, out var value) ? value : null;

}



public enum GamePhase
{

}



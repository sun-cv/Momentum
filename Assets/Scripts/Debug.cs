using System;
using System.Collections.Generic;



public static class Logging
{
    private static LogLevel globalLevel                     = LogLevel.Event;
    private static Dictionary<LogSystem, Logger> loggers    = new();

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public static Logger For(LogSystem system, LogLevel defaultLevel = LogLevel.None)
    {
        if (!loggers.TryGetValue(system, out var logger))
        {
            logger = new Logger(system, defaultLevel);
            loggers[system] = logger;
        }
        return logger;
    }

    public static Logger Find(LogSystem system)
    {   
        return loggers.TryGetValue(system, out var logger) ? logger : null;
    }

    public static void SetGlobalLevel(LogLevel level)
    {
        globalLevel = level;
    }

    // ===============================================================================

    
    internal static LogLevel GlobalLevel            => globalLevel;

    public static IEnumerable<Logger> AllLoggers    => loggers.Values;
}

public class Logger
{
    private LogLevel                level;
    private readonly LogSystem      system;

    // ===============================================================================

    internal Logger(LogSystem system, LogLevel level)
    {
        this.system     = system;
        this.level      = level;
    }

    // ===============================================================================
    //  Public API
    // ===============================================================================

    public void SetLevel(LogLevel newLevel)
    {
        level = newLevel;
    }

    public void Disable()
    {
        level = LogLevel.None;
    }

    public void Enable(LogLevel newLevel = LogLevel.Event)
    {
        level = newLevel;
    }

    public void Trace(string message)
    {
        if (IsEnabled(LogLevel.Trace))
            UnityEngine.Debug.Log($"[{system}] {message}");
    }

    public void Debug(string message)
    {
        if (IsEnabled(LogLevel.Debug))
            UnityEngine.Debug.Log($"[{system}] {message}");
    }

    public void Event(string message)
    {
        if (IsEnabled(LogLevel.Event))
            UnityEngine.Debug.Log($"[{system}] {message}");
    }

    public void Admin(string message)
    {
        if (IsEnabled(LogLevel.Admin))
            UnityEngine.Debug.Log($"[{system}] {message}");
    }

    public void Error(string message)
    {
        if (IsEnabled(LogLevel.Error))
            UnityEngine.Debug.Log($"[{system}] {message}");
    }

    public void Trace(string tag, Func<object> value)
    {
        if (IsEnabled(LogLevel.Trace))
            Logwin.Log(tag, value(), $"Trace: {system}");
    }

    public void Debug(string tag, Func<object> value)
    {
        if (IsEnabled(LogLevel.Debug))
            Logwin.Log(tag, value(), $"Debug: {system}");
    }

    // ===============================================================================
    //  Predicates
    // ===============================================================================

    private bool IsEnabled(LogLevel logLevel)
    {
        if (level == LogLevel.None)
            return false;

        return logLevel >= level || logLevel >= Logging.GlobalLevel;
    }
}


// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬
//                                      Declarations
// ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬

public enum LogLevel
{
    None,
    Trace,
    Debug,
    Event,
    Admin,
    Error,
}

public enum LogSystem
{
    None,
    
    // Core Engine
    Engine,
    Timers,
    System,
    Services,
    Definitions,
    Assets,
    Actors,
    Factories,
    Dev,
    
    // Rendering & Visual
    Animation,
    Camera,
    SpriteLayering,
    DepthSorting,
    Particles,
    Effects,
    
    // Physics & Movement
    Physics,
    Collision,
    Movement,
    Teleport,
    
    // Combat & Equipment
    Weapons,
    Hitboxes,
    Equipment,
    Combat,
    Spawners,
    
    // Player & Input
    Hero,
    Input,
    Command,
    Direction,
    
    // Gameplay Systems
    Stats,
    Lifecycle,
    Presence,

}

using System;
using System.Collections.Generic;



public class LoggingSystem : RegisteredService, IServiceUtil
{
    public void Util()
    {
        Logging.Tick();
    }

    // ===============================================================================

    public override void Dispose()
    {
        Services.Lane.Deregister(this);
    }

    // ===============================================================================

    public UpdatePriority Priority  => ServiceUpdatePriority.LoggingSystem;
}



public static class Logging
{
    private static LogLevel globalLevel                     = LogLevel.Event;
    private static Dictionary<LogSystem, Logger> loggers    = new();

    public static void Tick()
    {
        foreach (var logger in loggers.Values)
            logger.BeginFrame();
    }

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
    LogLevel            level;
    readonly LogSystem  system;
    
    Dictionary<string, string> currentTags  = new();
    Dictionary<string, string> previousTags = new();


    // ===============================================================================

    internal Logger(LogSystem system, LogLevel level)
    {
        this.system     = system;
        this.level      = level;
    }

    // ===============================================================================

    public void BeginFrame()
    {

        foreach (var (tag, category) in previousTags)
        {
            if (!currentTags.ContainsKey(tag))
            {
                Logwin.DeleteLog(tag, category);

                UnityEngine.Debug.Log($"After delete, exists: {LogWinInternal.Logwin_Internal.sCatDico.ContainsKey(category) && LogWinInternal.Logwin_Internal.sCatDico[category].GetLog(tag) != null}");
            
            }
        }
        (previousTags, currentTags) = (currentTags, previousTags);
        currentTags.Clear();
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

    void Log(string message, LogLevel level)
    {
        if (!IsEnabled(level))
            return;

        UnityEngine.Debug.Log($"[{system}] {message}");
    }

    void Log(string tag, LogLevel level, Func<object> value, bool clean)
    {
        if (!IsEnabled(level))
            return;

        var category = Category(level);

        if (clean)
            currentTags[tag] = category;

        Logwin.Log(tag, value(), category);
    }

    public void Trace(string message)
    {
        Log(message, LogLevel.Trace);
    }

    public void Debug(string message)
    {
        Log(message, LogLevel.Debug);
    }

    public void Event(string message)
    {
        Log(message, LogLevel.Event);
    }

    public void Admin(string message)
    {
        Log(message, LogLevel.Admin);
    }

    public void Error(string message)
    {
        Log(message, LogLevel.Error);
    }

    public void Trace(string tag, Func<object> value, bool clean = false)
    {
        Log(tag, LogLevel.Trace, value, clean);
    }

    public void Debug(string tag, Func<object> value, bool clean = false)
    {
        Log(tag, LogLevel.Debug, value, clean);
    }

    public void Event(string tag, Func<object> value, bool clean = false)
    {
        Log(tag, LogLevel.Event, value, clean);
    }

    // ===============================================================================
    //  Helpers
    // ===============================================================================

    string Category(LogLevel atLevel)
    {
        return $"{atLevel}: {system}";
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
    ContactResolver,
    Collision,
    MovementEngine,
    Movement,
    Teleport,
    
    // Combat & Equipment
    Weapons,
    Hitboxes,
    Equipment,
    Combat,
    Damage,
    Spawners,
    
    // Player & Input
    Hero,
    Input,
    Command,
    Direction,
    Resources,

    
    // Gameplay Systems
    Stats,
    Lifecycle,
    Presence,

}

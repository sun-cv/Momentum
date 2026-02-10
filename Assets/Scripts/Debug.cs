using System;
using System.Collections.Generic;





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
    Assets,
    Actors,
    Factories,
    
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
    
    // Combat & Equipment
    Weapons,
    Hitboxes,
    Equipment,
    Combat,
    
    // Player & Input
    Hero,
    Input,
    Command,
    Direction,
    
    // Gameplay Systems
    Stats,
    Lifecycle,

}

public static class Logging
{
    private static LogLevel globalLevel                     = LogLevel.Event;
    private static Dictionary<LogSystem, Logger> loggers    = new();

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

    internal static LogLevel GlobalLevel => globalLevel;

    public static IEnumerable<Logger> AllLoggers => loggers.Values;
}

public class Logger
{
    private LogLevel                level;
    private readonly LogSystem      system;

    internal Logger(LogSystem system, LogLevel level)
    {
        this.system     = system;
        this.level      = level;
    }

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

    private bool IsEnabled(LogLevel logLevel)
    {
        if (level == LogLevel.None)
            return false;

        return logLevel >= level || logLevel >= Logging.GlobalLevel;
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

}


















// public enum LogLevel
// {
//     None,
//     Trace,
//     Debug,
//     Event,
//     Admin,
//     Error,
// }

// public enum LogSystem
// {
//     Timers,
//     Engine,
//     System,
//     Dev,
//     Input,
//     Stats,
//     Command,
//     Weapon,
//     Effects,
//     Movement,
//     Direction,
//     Equipment,
//     Hitboxes,
//     DepthSorting,
//     Physics,
//     Animation,
//     Camera,

//     Hero,
//     Lifecycle,
// }

// public enum LogCategory
// {
//     Admin,
//     Phase,
//     State,
//     Activation,
//     Validation,
//     Deactivation,
//     Control,
//     Command,
//     Effect,
//     Lock,
//     Input,
//     Cooldown,
// // }


// public static class Log
// {
//     private static LogLevel globalLevel = LogLevel.Event;
//     private static Dictionary<LogSystem, LogLevel> systemLevels = new();
//     private static Dictionary<(LogSystem, LogCategory), LogLevel> categoryLevels = new();

//     public static bool IsEnabled(LogLevel level, LogSystem system, LogCategory category)
//     {
//         if (categoryLevels.TryGetValue((system, category), out var catLevel))
//             return level >= catLevel;

//         if (systemLevels.TryGetValue(system, out var sysLevel))
//             return level >= sysLevel;

//         return level >= globalLevel;
//     }

//     private static void WriteLog(LogLevel level, LogSystem system, LogCategory category, Func<string> messageBuilder)
//     {
//         if (IsEnabled(level, system, category))
//             UnityEngine.Debug.Log(messageBuilder());
//     }

//     private static void WriteLogwin(LogLevel level, LogSystem system, LogCategory category, string handle, string key, Func<object> valueBuilder)
//     {
//         if (IsEnabled(level, system, category))
//             Logwin.Log(key, valueBuilder(), handle);
//     }

//     public static void Configure(Action<LogConfig> configure)
//     {
//         var config = new LogConfig();
//         configure(config);
//         config.Apply();
//     }

//     public class LogConfig
//     {

//         public LogConfig Global(LogLevel level) { globalLevel = level; return this; }
        
//         public LogConfig System(LogSystem system, LogLevel level) 
//         { 
//             systemLevels[system] = level; 
//             return this; 
//         }
        
//         public LogConfig Category(LogSystem system, LogCategory category, LogLevel level) 
//         { 
//             categoryLevels[(system, category)] = level; 
//             return this; 
//         }

//         internal void Apply() {}
//     }

//     public static void SetGlobalLevel(LogLevel level)
//     {
//         globalLevel = level;
//     }

//     public static void SetSystemLevel(LogSystem system, LogLevel level)
//     {
//         systemLevels[system] = level;
//     }

//     public static void SetCategoryLevel(LogSystem system, LogCategory category, LogLevel level)
//     {
//         categoryLevels[(system, category)] = level;
//     }

//     public static void DisableSystem(LogSystem system)
//     {
//         systemLevels[system] = LogLevel.None;
//     }


//     public static void Trace(LogSystem system, LogCategory category, Func<string> log) 
//         => WriteLog(LogLevel.Trace, system, category, log);
//     public static void Trace(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
//         => WriteLogwin(LogLevel.Trace, system, category, handle,  key, log);

//     public static void Debug(LogSystem system, LogCategory category, Func<string> log) 
//         => WriteLog(LogLevel.Debug, system, category, log);
//     public static void Debug(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
//         => WriteLogwin(LogLevel.Debug, system, category, handle, key, log);

//     public static void Event(LogSystem system, LogCategory category, Func<string> log) 
//         => WriteLog(LogLevel.Event, system, category, log);
//     public static void Event(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
//         => WriteLogwin(LogLevel.Event, system, category, handle, key, log);

//     public static void Admin(LogSystem system, LogCategory category, Func<string> log) 
//         => WriteLog(LogLevel.Admin, system, category, log);
//     public static void Admin(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
//         => WriteLogwin(LogLevel.Admin, system, category, handle, key, log);

//     public static void Error(LogSystem system, LogCategory category, Func<string> log) 
//         => WriteLog(LogLevel.Error, system, category, log);
//     public static void Error(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
//         => WriteLogwin(LogLevel.Error, system, category, handle, key, log);

// }


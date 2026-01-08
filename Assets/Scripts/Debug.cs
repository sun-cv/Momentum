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
    Timers,
    Engine,
    System,
    Dev,
    Input,
    Stats,
    Command,
    Weapon,
    Effects,
    Movement,
    Equipment,
    Physics,
    Camera,
}

public enum LogCategory
{
    Phase,
    State,
    Activation,
    Validation,
    Control,
    Command,
    Effect,
    Lock,
    Input,
    Cooldown,
}


public static class Log
{
    private static LogLevel globalLevel = LogLevel.Event;
    private static Dictionary<LogSystem, LogLevel> systemLevels = new();
    private static Dictionary<(LogSystem, LogCategory), LogLevel> categoryLevels = new();

    public static bool IsEnabled(LogLevel level, LogSystem system, LogCategory category)
    {
        if (categoryLevels.TryGetValue((system, category), out var catLevel))
            return level >= catLevel;

        if (systemLevels.TryGetValue(system, out var sysLevel))
            return level >= sysLevel;

        return level >= globalLevel;
    }

    private static void WriteLog(LogLevel level, LogSystem system, LogCategory category, Func<string> messageBuilder)
    {
        if (IsEnabled(level, system, category))
            UnityEngine.Debug.Log(messageBuilder());
    }

    private static void WriteLogwin(LogLevel level, LogSystem system, LogCategory category, string handle, string key, Func<object> valueBuilder)
    {
        if (IsEnabled(level, system, category))
            Logwin.Log(key, valueBuilder(), handle);
    }

    public static void Configure(Action<LogConfig> configure)
    {
        var config = new LogConfig();
        configure(config);
        config.Apply();
    }

    public class LogConfig
    {
        public LogConfig Global(LogLevel level) { globalLevel = level; return this; }
        
        public LogConfig System(LogSystem system, LogLevel level) 
        { 
            systemLevels[system] = level; 
            return this; 
        }
        
        public LogConfig Category(LogSystem system, LogCategory category, LogLevel level) 
        { 
            categoryLevels[(system, category)] = level; 
            return this; 
        }

        internal void Apply() {}
    }

    public static void SetGlobalLevel(LogLevel level)
    {
        globalLevel = level;
    }

    public static void SetSystemLevel(LogSystem system, LogLevel level)
    {
        systemLevels[system] = level;
    }

    public static void SetCategoryLevel(LogSystem system, LogCategory category, LogLevel level)
    {
        categoryLevels[(system, category)] = level;
    }

    public static void DisableSystem(LogSystem system)
    {
        systemLevels[system] = LogLevel.None;
    }


    public static void Trace(LogSystem system, LogCategory category, Func<string> log) 
        => WriteLog(LogLevel.Trace, system, category, log);
    public static void Trace(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
        => WriteLogwin(LogLevel.Trace, system, category, handle,  key, log);

    public static void Debug(LogSystem system, LogCategory category, Func<string> log) 
        => WriteLog(LogLevel.Debug, system, category, log);
    public static void Debug(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
        => WriteLogwin(LogLevel.Debug, system, category, handle, key, log);

    public static void Event(LogSystem system, LogCategory category, Func<string> log) 
        => WriteLog(LogLevel.Event, system, category, log);
    public static void Event(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
        => WriteLogwin(LogLevel.Event, system, category, handle, key, log);

    public static void Admin(LogSystem system, LogCategory category, Func<string> log) 
        => WriteLog(LogLevel.Admin, system, category, log);
    public static void Admin(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
        => WriteLogwin(LogLevel.Admin, system, category, handle, key, log);

    public static void Error(LogSystem system, LogCategory category, Func<string> log) 
        => WriteLog(LogLevel.Error, system, category, log);
    public static void Error(LogSystem system, LogCategory category,string handle, string key, Func<object> log) 
        => WriteLogwin(LogLevel.Error, system, category, handle, key, log);

}


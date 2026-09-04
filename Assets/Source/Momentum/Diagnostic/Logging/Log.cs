using System;
using System.Collections.Generic;
using Game.Common;
using UnityEngine;



namespace Game.Diagnostic
{
    namespace Log
    {
        public enum Level
        {
            None,
            Trace,
            Debug,
            Event,
            Admin,
            Alert,
            Error,
        }
    }

    public class LoggingController : RegisteredService, IRateUtil
    {
        public void Tick()
        {
            Logging.Tick(); 
        }
    }

    public static class Logging
    {
        
        private static Log.Level globalLevel                        = Log.Level.None;

        private static bool alwaysLogAlert                          = true;
        private static bool alwaysLogError                          = true;

        private static readonly Dictionary<Type, Logger> instance   = new();
        
        public static void Tick()
        {
            foreach (var log in instance.Values)
                log.Tick();
        }

        public static Logger Get<T>(Log.Level level = Log.Level.None)
        {
            if (!instance.TryGetValue(typeof(T), out var log))
            {
                log                 = new(typeof(T), level);
                instance[typeof(T)] = log;
            }

            return log;
        }

        public static void SetGlobalLevel(Log.Level level)
        {
            globalLevel = level;
        }

        public static void SetGlobalLogAlert(bool value)
        {
            alwaysLogAlert = value;
        }

        public static void SetGlobalLogError(bool value)
        {
            alwaysLogError = value;
        }

        public static bool AlwaysLogAlert               => alwaysLogAlert;
        public static bool AlwaysLogError               => alwaysLogError;

        internal static Log.Level Level                 => globalLevel;
        public static IEnumerable<Logger> AllLoggers    => instance.Values;
    }

    public class Logger
    {
        private Log.Level level;
            
        private readonly Type system;

        private Dictionary<string, string> currentTags  = new();
        private Dictionary<string, string> previousTags = new();

        internal Logger(Type system, Log.Level level)
        {
            this.system = system;
            this.level  = level;
        }

        public void Tick()
        {

            foreach (var (tag, category) in previousTags)
            {
                if (!currentTags.ContainsKey(tag))
                    Logwin.DeleteLog(tag, category);
            }
            (previousTags, currentTags) = (currentTags, previousTags);
            currentTags.Clear();
        }

        public void SetLevel(Log.Level newLevel)
        {
            level = newLevel;
        }

        public void Disable()
        {
            level = Log.Level.None;
        }

        public void Enable(Log.Level newLevel = Log.Level.Event)
        {
            level = newLevel;
        }

        public void Print(string message, Log.Level level)
        {
            if (!IsEnabled(level))
                return;

            UnityEngine.Debug.Log($"[{system}] {message}");
        }

        public void Print(string tag, Log.Level level, Func<object> value, bool clean)
        {
            if (!IsEnabled(level))
                return;

            var category = Category(level);

            if (clean)
                currentTags[tag] = category;

            Logwin.Log(tag, value(), category);
        }

        private string Category(Log.Level atLevel)
        {
            return $"{atLevel}: {system}";
        }

        private bool IsEnabled(Log.Level logLevel)
        {
            if (Logging.AlwaysLogAlert && logLevel == Log.Level.Alert)
                return true;

            if (Logging.AlwaysLogError && logLevel == Log.Level.Error)
                return true;

            if (level == Log.Level.None)
                return false;

            return logLevel >= level || logLevel >= Logging.Level;
        }
    }

    public static class Log<T>
    {
        private static readonly Logger instance = Logging.Get<T>();

        public static void Level(Log.Level level)
        {
            instance.SetLevel(level);
        }

        public static void Trace(string message)
        {
            instance.Print(message, Log.Level.Trace);
        }

        public static void Debug(string message)
        {
            instance.Print(message, Log.Level.Debug);
        }

        public static  void Event(string message)
        {
            instance.Print(message, Log.Level.Event);
        }

        public static  void Admin(string message)
        {
            instance.Print(message, Log.Level.Alert);
        }

        public static void Alert(string message)
        {
            instance.Print(message, Log.Level.Admin);
        }

        public static  void Error(string message)
        {
            instance.Print(message, Log.Level.Error);
        }

        public static  void Trace(object value)
        {
            instance.Print($"{value}", Log.Level.Trace);
        }

        public static  void Debug(object value)
        {
            instance.Print($"{value}", Log.Level.Debug);
        }

        public static  void Event(object value)
        {
            instance.Print($"{value}", Log.Level.Event);
        }

        public static  void Admin(object value)
        {
            instance.Print($"{value}", Log.Level.Admin);
        }

        public static  void Alert(object value)
        {
            instance.Print($"{value}", Log.Level.Alert);
        }

        public static  void Error(object value)
        {
            instance.Print($"{value}", Log.Level.Error);
        }

        public static  void Trace(string tag, Func<object> value, bool clean = false)
        {
            instance.Print(tag, Log.Level.Trace, value, clean);
        }

        public static  void Debug(string tag, Func<object> value, bool clean = false)
        {
            instance.Print(tag, Log.Level.Debug, value, clean);
        }

        public static  void Event(string tag, Func<object> value, bool clean = false)
        {
            instance.Print(tag, Log.Level.Event, value, clean);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using Game.Realm;
using Game.Common;
using Game.Content;
using Game.Diagnostic;


namespace Game.Development
{

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Name;
        public string Description;

        public CommandAttribute(string name, string description = "")
        {
            Name        = name;
            Description = description;
        }
    }

    public partial class Console : RegisteredService, IWorld, IData, IAsset, IRealBase
    {
        private delegate bool Parser(string submission, out object value);

        private class Entry
        {
            public MethodInfo      Method;
            public ParameterInfo[] Parameters;
            public string          Description;
            public bool            Raw;
        }

        private readonly Dictionary<string, Entry> commands = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Type, Parser>  parsers  = new();
        private int longest;

        private readonly Data   Data;
        private readonly Assets Asset;
        private readonly World  World;

        public Console(World world, Data data, Assets asset)
        {
            Data    = data;
            Asset   = asset;
            World   = world;

            RegisterParsers();
            RegisterCommands();
        }

        public void Tick()
        {
        
        }

        private void RegisterParsers()
        {
            parsers.Add(typeof(string), ParseString);
            parsers.Add(typeof(int),    ParseInt);
            parsers.Add(typeof(float),  ParseFloat);
            parsers.Add(typeof(Entity), ParseEntity);
        }

        private void RegisterCommands()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var method in GetType().GetMethods(flags))
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();

                if (attribute == null)
                    continue;

                if (commands.ContainsKey(attribute.Name))
                    throw new Exception($"[Console.Register] {attribute.Name} command already registered");

                var  parameters = method.GetParameters();
                bool raw        = parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]);

                if (!raw)
                {
                    foreach (var parameter in parameters)
                    {
                        if (!parsers.ContainsKey(parameter.ParameterType))
                            throw new Exception($"[Console.Register] {attribute.Name} has no parser for {parameter.ParameterType.Name}");
                    }
                }

                commands.Add(attribute.Name, new Entry
                {
                    Method      = method,
                    Parameters  = parameters,
                    Description = attribute.Description,
                    Raw         = raw,
                });

                longest = Math.Max(longest, attribute.Name.Split(' ').Length);
            }
        }

        public void Execute(string line)
        {
            var words = line.TrimStart('/').Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return;

            for (int count = Math.Min(words.Length, longest); count > 0; count--)
            {
                string name = string.Join(" ", words, 0, count);

                if (commands.TryGetValue(name, out var entry))
                {
                    Run(name, entry, words[count..]);
                    return;
                }
            }

            Log<Console>.Debug($"Submitted unknown command ({words[0]})");
        }

        private void Run(string name, Entry entry, string[] arguments)
        {
            if (entry.Raw)
            {
                entry.Method.Invoke(this, new object[] { arguments });
                return;
            }

            if (arguments.Length != entry.Parameters.Length)
            {
                Log<Console>.Debug($"Submitted incorrect number of arguments. Expected {entry.Parameters.Length}; Received {arguments.Length}");
                Log<Console>.Debug(Usage(name, entry));
                return;
            }

            var values = new object[arguments.Length];

            for (int index = 0; index < arguments.Length; index++)
            {
                var parser = parsers[entry.Parameters[index].ParameterType];

                if (!parser(arguments[index], out values[index]))
                {
                    Log<Console>.Debug(Usage(name, entry));
                    return;
                }
            }

            entry.Method.Invoke(this, values);
        }

        private string Usage(string name, Entry entry)
        {
            if (entry.Raw)
                return $"{name}  {entry.Description}";

            var names = new List<string>();

            foreach (var parameter in entry.Parameters)
            {
                names.Add($"<{parameter.Name}>");
            }

            return $"{name} {string.Join(" ", names)}  {entry.Description}";
        }

        [Command("help", "List commands, or those starting with a prefix")]
        private void Help(string[] words)
        {
            string prefix = string.Join(" ", words);

            foreach (var pair in commands)
            {
                if (!pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                Log<Console>.Debug(Usage(pair.Key, pair.Value));
            }
        }

        private bool ParseString(string submission, out object value)
        {
            value = submission;
            return true;
        }

        private bool ParseInt(string submission, out object value)
        {
            value = null;

            if (!int.TryParse(submission, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                Log<Console>.Debug($"Submitted invalid int ({submission})");
                return false;
            }

            value = result;
            return true;
        }

        private bool ParseFloat(string submission, out object value)
        {
            value = null;

            if (!float.TryParse(submission, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                Log<Console>.Debug($"Submitted invalid float ({submission})");
                return false;
            }

            value = result;
            return true;
        }

        private bool ParseEntity(string submission, out object value)
        {
            value = null;

            // "hero", "12" or "12:4". Needs an index lookup on World, which doesn't exist yet.
            Log<Console>.Debug($"Submitted invalid entity ({submission})");
            return false;
        }

        static Console() => Log<Console>.Level(Diagnostic.Log.Level.Debug);
    }
}

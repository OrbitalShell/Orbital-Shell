using System;
using System.Reflection;

namespace DotNetConsoleAppToolkit.Component.CommandLine
{
    public class CommandsModule
    {
        public readonly Assembly Assembly;
        public readonly Type Type;
        public readonly string Name;
        public readonly string Description;
        public readonly int TypesCount;
        public readonly int CommandsCount;

        public CommandsModule(string name,string description,Assembly assembly,int typesCount,int commandsCount,Type type = null)
        {
            Name = name;
            Assembly = assembly;
            Type = type;
            TypesCount = typesCount;
            CommandsCount = commandsCount;
            Description = description;
        }

        public static string DeclaringTypeShortName(Type type)
        {
            var r = type.Name;
            var i = r.LastIndexOf("Commands");
            if (i > 0)
                r = r.Substring(0, i);
            return r;
        }
    }
}

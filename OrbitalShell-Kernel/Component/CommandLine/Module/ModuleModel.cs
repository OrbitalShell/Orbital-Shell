using System;
using System.Reflection;

namespace OrbitalShell.Component.CommandLine.Module
{
    /// <summary>
    /// module model and related util methods
    /// </summary>
    public class ModuleModel
    {
        public readonly Assembly Assembly;
        public readonly Type Type;
        public readonly string Name;
        public readonly string Description;
        public readonly int TypesCount;
        public readonly int CommandsCount;

        public ModuleModel(string name,string description,Assembly assembly,int typesCount,int commandsCount,Type type = null)
        {
            Name = name;
            Assembly = assembly;
            Type = type;
            TypesCount = typesCount;
            CommandsCount = commandsCount;
            Description = description;
        }

        /// <summary>
        /// format and returns command declaring type from given type class name
        /// </summary>
        /// <param name="type">a command declaring type</param>
        /// <returns>camel case name , ends 'Commands' removed if present</returns>
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

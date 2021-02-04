using System;
using System.Reflection;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// module specification
    /// </summary>
    public class ModuleSpecification
    {
        public readonly Assembly Assembly;

        public readonly Type Type;

        public readonly string Name;

        public readonly string Description;

        public readonly ModuleInfo Info;

        public readonly string Key;

        public static ModuleSpecification ModuleSpecificationNotDefined = new ModuleSpecification();

        public ModuleSpecification(
            string key,
            string name,
            string description,
            Assembly assembly,
            ModuleInfo info,
            Type type = null)
        {
            Key = key;
            Name = name;
            Assembly = assembly;
            Type = type;
            Info = info;
            Description = description;
        }

        public ModuleSpecification() { }
    }
}

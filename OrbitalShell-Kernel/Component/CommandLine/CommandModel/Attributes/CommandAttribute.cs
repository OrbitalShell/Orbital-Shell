using System;

namespace OrbitalShell.Component.CommandLine.CommandModel.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public readonly string Description;
        public readonly string LongDescription;
        public readonly string Name;
        public readonly string Documentation;

        public CommandAttribute(string description, string longDescription = null, string documentation = null, string name = null)
        {
            Description = description;
            Documentation = documentation;
            LongDescription = longDescription;
            Name = name;
        }
    }
}

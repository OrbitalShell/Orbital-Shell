using System;

namespace OrbitalShell.Component.CommandLine.CommandModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CommandsAttribute : Attribute
    {
        public readonly string Description;

        public CommandsAttribute(string description)
        {
            Description = description;
        }
    }
}

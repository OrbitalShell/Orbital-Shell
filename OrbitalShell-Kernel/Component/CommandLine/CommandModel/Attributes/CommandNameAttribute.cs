using System;

namespace OrbitalShell.Component.CommandLine.CommandModel.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandNameAttribute : Attribute
    {
        public readonly string Name;

        public CommandNameAttribute(string name = null)
        {
            Name = name;
        }
    }
}

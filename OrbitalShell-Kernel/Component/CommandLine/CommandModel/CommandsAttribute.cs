using System;

namespace DotNetConsoleAppToolkit.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =true)]
    public class CommandsAttribute : Attribute
    {
        public readonly string Description;

        public CommandsAttribute(string description)
        {
            Description = description;
        }
    }
}

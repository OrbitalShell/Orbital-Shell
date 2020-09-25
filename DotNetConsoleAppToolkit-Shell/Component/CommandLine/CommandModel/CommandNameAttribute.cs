using System;

namespace DotNetConsoleAppToolkit.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited =false)]
    public class CommandNameAttribute : Attribute
    {
        public readonly string Name;

        public CommandNameAttribute(string name=null)
        {
            Name = name;
        }
    }
}

using System;

namespace OrbitalShell.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = true,Inherited = false)]
    public class RequireOSCommandAttribute : Attribute
    {
        public readonly string CommandName;

        public RequireOSCommandAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}

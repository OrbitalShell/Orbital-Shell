using System;

namespace OrbitalShell.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandsNamespaceAttribute : Attribute
    {
        public readonly string[] Segments;

        public CommandsNamespaceAttribute(params string[] segments)
        {
            Segments = segments;
        }

        public CommandsNamespaceAttribute(CommandNamespace @namespace)
        {
            Segments = new string[] { "" + @namespace };
        }
    }
}

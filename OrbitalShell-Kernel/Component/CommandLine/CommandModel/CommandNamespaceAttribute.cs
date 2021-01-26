using System;

namespace OrbitalShell.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandNamespaceAttribute : Attribute
    {
        public readonly string[] Segments;

        public CommandNamespaceAttribute(params string[] segments)
        {
            Segments = segments;
        }

        public CommandNamespaceAttribute(CommandNamespace @namespace)
        {
            Segments = new string[] { "" + @namespace };
        }
    }
}

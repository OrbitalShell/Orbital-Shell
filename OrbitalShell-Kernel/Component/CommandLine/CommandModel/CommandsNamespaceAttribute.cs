using System;
using System.Collections.Generic;
using System.Linq;
using OrbitalShell.Component.Shell;

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

        public CommandsNamespaceAttribute(params CommandNamespace[] segments)
        {
            Segments = segments.Select(x => x + "").ToArray();
        }

        public CommandsNamespaceAttribute(CommandNamespace @namespace, params string[] segments)
        {
            var s = new List<string> { "" + @namespace };
            s.AddRange(segments);
            Segments = s.ToArray();
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using OrbitalShell.Component.Shell;

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

        public CommandNamespaceAttribute(CommandNamespace rootNamespace, params string[] segments)
        {
            var s = new List<string> { rootNamespace + "" };
            s.AddRange(segments);
            Segments = s.ToArray();
        }

        public CommandNamespaceAttribute(params CommandNamespace[] segments)
        {
            Segments = segments.Select(x => x + "").ToArray();
        }
    }
}

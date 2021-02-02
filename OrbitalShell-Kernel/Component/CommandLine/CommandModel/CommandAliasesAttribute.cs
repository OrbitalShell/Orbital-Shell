using System;

namespace OrbitalShell.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAliasesAttribute : Attribute
    {
        public readonly string[] Aliases;

        public CommandAliasesAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

    }
}

using System;

namespace OrbitalShell.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CommandAliasAttribute : Attribute
    {
        public string AliasName;
        public string AliasText;

        public CommandAliasAttribute(string aliasName, string aliasText)
        {
            AliasName = aliasName;
            AliasText = aliasText;
        }

    }
}

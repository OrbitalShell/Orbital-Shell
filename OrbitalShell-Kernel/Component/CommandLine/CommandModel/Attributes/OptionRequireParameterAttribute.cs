using System;

namespace OrbitalShell.Component.CommandLine.CommandModel.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public class OptionRequireParameterAttribute : Attribute
    {
        public readonly string RequiredParameterName;

        public OptionRequireParameterAttribute(string requiredParameterName)
        {
            RequiredParameterName = requiredParameterName;
        }
    }
}

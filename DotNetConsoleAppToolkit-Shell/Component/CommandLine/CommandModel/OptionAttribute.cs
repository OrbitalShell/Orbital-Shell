using System;

namespace DotNetConsoleAppToolkit.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class OptionAttribute : Attribute
    {
        public readonly bool IsOptional = false;
        public readonly string Description;
        public readonly string OptionName = null;
        public readonly bool HasValue = false;
        public object? DefaultValue;

        public OptionAttribute(string optionName, string description, bool isOptional)
        {
            OptionName = optionName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
        }

        public OptionAttribute(string optionName, string description,bool hasValue, bool isOptional)
        {
            HasValue = hasValue;
            OptionName = optionName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
        }

        public OptionAttribute(string optionName, string description)
        {
            OptionName = optionName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = true;
        }

        public OptionAttribute(string optionName, string description, bool isOptional, object defaultValue)
        {
            OptionName = optionName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
        }

        public OptionAttribute(string optionName, string description, bool hasValue, bool isOptional, object defaultValue)
        {
            HasValue = hasValue;
            OptionName = optionName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
        }

        public OptionAttribute(string optionName, string description, object defaultValue)
        {
            OptionName = optionName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = true;
            DefaultValue = defaultValue;
        }
    }
}

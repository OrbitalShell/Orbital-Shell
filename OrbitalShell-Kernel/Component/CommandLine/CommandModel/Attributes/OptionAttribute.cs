using System;

namespace OrbitalShell.Component.CommandLine.CommandModel.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class OptionAttribute : Attribute
    {
        public readonly bool IsOptional = false;
        public readonly string Description;

        /// <summary>
        /// option name is short name : 💥 NULL implies is parameter in ParameterSyntax syntax analyser
        /// </summary>
        public readonly string OptionName = null;

        /// <summary>
        /// option long name. might be null
        /// </summary>
        public readonly string OptionLongName = null;

        public readonly bool HasValue = false;
        public object DefaultValue;
        public bool HasDefaultValue { get; protected set; }

        public OptionAttribute(string optionName, string optionLongName, string description, bool isOptional)
        {
            OptionName = optionName;
            OptionLongName = optionLongName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
        }

        public OptionAttribute(string optionName, string optionLongName, string description, bool hasValue, bool isOptional)
        {
            HasValue = hasValue;
            OptionName = optionName;
            OptionLongName = optionLongName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
        }

        public OptionAttribute(string optionName, string optionLongName, string description)
        {
            OptionName = optionName;
            OptionLongName = optionLongName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = true;
        }

        public OptionAttribute(string optionName, string optionLongName, string description, bool isOptional, object defaultValue)
        {
            OptionName = optionName;
            OptionLongName = optionLongName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }

        public OptionAttribute(string optionName, string optionLongName, string description, bool hasValue, bool isOptional, object defaultValue)
        {
            HasValue = hasValue;
            OptionName = optionName;
            OptionLongName = optionLongName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }

        public OptionAttribute(string optionName, string optionLongName, string description, object defaultValue)
        {
            OptionName = optionName;
            OptionLongName = optionLongName;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = true;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }
    }
}

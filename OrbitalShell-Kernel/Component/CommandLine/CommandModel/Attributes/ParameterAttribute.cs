using System;

namespace OrbitalShell.Component.CommandLine.CommandModel.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class ParameterAttribute : Attribute
    {
        public readonly bool IsOptional = false;
        public readonly int Index = -1;
        public readonly string Description;
        public object DefaultValue;
        public bool HasDefaultValue { get; protected set; }

        /// <summary>
        /// fixed at position=index, non optional or optinal if alone
        /// </summary>
        /// <param name="index"></param>
        /// <param name="description"></param>
        public ParameterAttribute(int index, string description, bool isOptional = false)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
        }

        /// <summary>
        /// index 0, can be optional, must have a value if not optional
        /// </summary>
        /// <param name="description"></param>
        /// <param name="isOptional"></param>
        public ParameterAttribute(string description, bool isOptional = false)
        {
            Index = 0;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
        }

        /// <summary>
        /// fixed at position=index, non optional or optinal if alone
        /// </summary>
        /// <param name="index"></param>
        /// <param name="description"></param>
        public ParameterAttribute(int index, string description, bool isOptional, object defaultValue)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }

        /// <summary>
        /// index 0, can be optional, must have a value if not optional
        /// </summary>
        /// <param name="description"></param>
        /// <param name="isOptional"></param>
        public ParameterAttribute(string description, bool isOptional, object defaultValue)
        {
            Index = 0;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }
    }
}

using System;

namespace DotNetConsoleAppToolkit.Component.CommandLine.CommandModel
{
    [AttributeUsage(AttributeTargets.Parameter,AllowMultiple =false,Inherited =false)]
    public class ParameterAttribute : Attribute
    {
        public readonly bool IsOptional = false;
        public readonly int Index = -1;
        public readonly string Description;
        public object? DefaultValue;
        
        /// <summary>
        /// fixed at position=index, non optional or optinal if alone
        /// </summary>
        /// <param name="index"></param>
        /// <param name="description"></param>
        public ParameterAttribute(int index,string description,bool isOptional=false, object defaultValue = null)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// index 0, can be optional, must have a value if not optional
        /// </summary>
        /// <param name="description"></param>
        /// <param name="isOptional"></param>
        public ParameterAttribute(string description,bool isOptional=false, object defaultValue = null)
        {
            Index = 0;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsOptional = isOptional;
            DefaultValue = defaultValue;
        }
    }
}

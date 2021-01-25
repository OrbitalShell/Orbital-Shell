using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public class MatchingParameter<T> : IMatchingParameter
    {
        public CommandParameterSpecification CommandParameterSpecification { get; protected set; }
        public T Value { get; protected set; }
        public bool IsCustom { get; protected set; }

        public object GetValue() => Value;

        public void SetValue(object value)
        {
            Value = (T)value;
        }

        public MatchingParameter(CommandParameterSpecification commandParameterSpecification, T value,bool isCustom = false) 
        {
            Value = value;
            CommandParameterSpecification = commandParameterSpecification;
            IsCustom = isCustom;
        }

        public MatchingParameter(CommandParameterSpecification commandParameterSpecification, bool isCustom = false)
        {
            CommandParameterSpecification = commandParameterSpecification;
            IsCustom = isCustom;
        }

        public override string ToString()
        {
            return $"{CommandParameterSpecification} = {Value}";
        }
    }
}

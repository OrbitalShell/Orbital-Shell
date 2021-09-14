using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing.Command.Parameter
{
    public interface IMatchingParameter
    {
        CommandParameterSpecification CommandParameterSpecification { get; }
        object GetValue();
        void SetValue(object value);
    }
}

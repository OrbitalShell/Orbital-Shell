using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public interface IMatchingParameter
    {
        CommandParameterSpecification CommandParameterSpecification { get; }
        object GetValue();
        void SetValue(object value);
    }
}

using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
{
    public interface IMatchingParameter
    {
        CommandParameterSpecification CommandParameterSpecification { get; }
        object GetValue();
        void SetValue(object value);
    }
}

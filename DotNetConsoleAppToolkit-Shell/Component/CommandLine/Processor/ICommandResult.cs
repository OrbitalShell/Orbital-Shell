namespace DotNetConsoleAppToolkit.Component.CommandLine.Processor
{
    public interface ICommandResult
    {
        object GetOuputData();
        int ReturnCode { get; }
    }
}
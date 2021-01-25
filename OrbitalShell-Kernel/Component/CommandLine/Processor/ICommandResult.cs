namespace OrbitalShell.Component.CommandLine.Processor
{
    public interface ICommandResult
    {
        object GetOuputData();
        int ReturnCode { get; }
        string ExecErrorText { get; }
    }
}
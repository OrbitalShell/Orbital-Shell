using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.CommandLine.Batch
{
    public interface ICommandBatchProcessor
    {
        int RunBatch(CommandEvaluationContext context, string path);
        int RunBatchText(CommandEvaluationContext context, string batch);
    }
}
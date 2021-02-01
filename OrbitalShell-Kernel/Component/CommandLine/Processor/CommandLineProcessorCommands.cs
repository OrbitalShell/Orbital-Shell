using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Lib.FileSystem;

namespace OrbitalShell.Component.CommandLine.Processor
{
    [Commands("commands of the shell command line processor (clp)")]
    [CommandsNamespace(CommandNamespace.kernel)]
    public class CommandLineProcessorCommands : ICommandsDeclaringType
    {
        #region shell exec

        [Command("runs a batch file [Experimental]")]
        public CommandResult<int> Batch(
            CommandEvaluationContext context,
            [Parameter(0, "path of the batch file (attempt a text file, starting or not by #orbsh!)")] FilePath path
            )
        {
            int r = (int)ReturnCode.OK;
            if (path.CheckExists(context))
            {
                context.CommandLineProcessor.CommandBatchProcessor.RunBatch(context, path.FileSystemInfo.FullName);
            }
            return new CommandResult<int>(r);
        }

        #endregion

        [Command("os (sub-shell) exec")]
        public CommandResult<int> Exec(
            CommandEvaluationContext context,
            [Parameter(0, "executable file path")] FilePath path,
            [Parameter(1, "arguments", true)] string args
            )
        {
            int ret = (int)ReturnCode.Error;
            if (path.CheckExists(context))
            {
                ret = context
                    .CommandLineProcessor
                    .ShellExec(
                        context,
                        path.FullName,
                        args
                    );
            }
            return new CommandResult<int>(ret);
        }
    }
}
using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Console.Formats;
using OrbitalShell.Component.Shell;
using OrbitalShell.Lib.FileSystem;

namespace OrbitalShell.Component.CommandLine.Processor
{
    [Commands("commands of the shell command line processor (clp)")]
    [CommandsNamespace(CommandNamespace.kernel)]
    public class CommandLineProcessorCommands : ICommandsDeclaringType
    {
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

        [Command("os shell exec")]
        public CommandResult<(int retCode, string output)> Exec(
            CommandEvaluationContext context,
            [Parameter(0, "executable file path")] FilePath path,
            [Parameter(1, "arguments", true)] string args
            )
        {
            int ret = (int)ReturnCode.Error;
            string output = null;
            if (path.CheckExists(context))
            {
                ret = context
                    .CommandLineProcessor
                    .ShellExec(
                        context,
                        path.FullName,
                        args,
                        out output,
                        redirectStandardInput: true
                    );
            }
            return new CommandResult<(int retCode, string output)>((ret, output));
        }

        [Command("find a file in shell paths, eventually limit possible file extensions of results to only registered shell path extensions")]
        public CommandResult<List<FilePath>> Which(
            CommandEvaluationContext context,
            [Parameter(0, "a file name, with or without extension")] string fileName,
            [Option("p", "path-ext", "select only results having a file extension that exists in PathExt")] bool pathExt,
            [Option("s", "short", "suppress file attributes in output")] bool @short
        )
        {
            if (CommandLineProcessor.FindInPath(
                    context,
                    fileName,
                    out var list,
                    pathExt
                ))

                foreach (var p in list)
                    p.Echo(
                        new EchoEvaluationContext(
                            context.Out,
                            context,
                            new FileSystemPathFormattingOptions(!@short, false, "", "(br)")
                            )
                    );

            return new CommandResult<List<FilePath>>(list);
        }
    }
}
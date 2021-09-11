using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using OrbitalShell.Lib.FileSystem;

using static OrbitalShell.Component.EchoDirective.Shortcuts;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Hist.cs
    /// </summary>
    public partial class ShellCommands
    {
        [Command("displays the commands history list or manipulate it")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.history)]
        [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static", Justification = "<En attente>")]
        public CommandVoidResult History(
            CommandEvaluationContext context,
            [Option("i", "invoke", "invoke the command at the entry number in the history list", true, true)] int num,
            [Option("c", "clear", "clear the loaded history list")] bool clear,
            [Option("w", "write", "write history lines to the history file (content of the file is replaced)")]
            [OptionRequireParameter("file")]  bool writeToFile,
            [Option("a", "append", "append history lines to the history file")]
            [OptionRequireParameter("file")]  bool appendToFile,
            [Option("r", "read", "read the history file and append the content to the history list")]
            [OptionRequireParameter("file")]  bool readFromFile,
            [Option("d", "read-diff", "read the history file and append the content not already in the history list to the history list")]
            [OptionRequireParameter("file")] bool appendFromFile,
            [Parameter(1, "file", true)] FilePath file
            )
        {
            var hist = context.CommandLineProcessor.CmdsHistory.History;
            var max = hist.Count.ToString().Length;
            int i = 1;
            var f = DefaultForegroundCmd;

            if (num > 0)
            {
                if (num < 1 || num > hist.Count)
                {
                    context.Errorln($"history entry number out of range (1..{hist.Count})");
                    return new CommandVoidResult(ReturnCode.Error);
                }
                var h = hist[num - 1];
                context.CommandLineProcessor.CommandLineReader.SendNextInput(h);
                return new CommandVoidResult();
            }

            if (clear)
            {
                context.CommandLineProcessor.CmdsHistory.ClearHistory(context);
                return new CommandVoidResult();
            }

            if (appendToFile || readFromFile || appendFromFile || writeToFile)
            {
                file ??= context.CommandLineProcessor.CmdsHistory.FilePath;
                if (file.CheckPathExists(context))
                {
                    if (writeToFile)
                    {
                        File.Delete(context.CommandLineProcessor.CmdsHistory.FilePath.FullName);
                        File.AppendAllLines(file.FullName, hist);
                    }
                    if (appendToFile) File.AppendAllLines(file.FullName, hist);
                    if (readFromFile)
                    {
                        var lines = File.ReadAllLines(file.FullName);
                        foreach (var line in lines) context.CommandLineProcessor.CmdsHistory.HistoryAppend(context, line);
                        context.CommandLineProcessor.CmdsHistory.HistorySetIndex(-1, false);
                    }
                    if (appendFromFile)
                    {
                        var lines = File.ReadAllLines(file.FullName);
                        foreach (var line in lines) if (!context.CommandLineProcessor.CmdsHistory.HistoryContains(line)) context.CommandLineProcessor.CmdsHistory.HistoryAppend(context, line);
                        context.CommandLineProcessor.CmdsHistory.HistorySetIndex(-1, false);
                    }
                }
                return new CommandVoidResult();
            }

            foreach (var h in hist)
            {
                if (context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested)
                    break;
                var hp = $"  {context.ShellEnv.Colors.Numeric}{i.ToString().PadRight(max + 2, ' ')}{f}";
                context.Out.Echo(hp);
                context.Out.ConsolePrint(h, true);
                i++;
            }
            return new CommandVoidResult();
        }

        [Command("repeat the previous command if there is one, else does nothing")]
        [CommandName("!!")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.history)]
        [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static", Justification = "<En attente>")]
        public CommandResult<string> HistoryPreviousCommand(
            CommandEvaluationContext context
            )
        {
            var lastCmd = context.CommandLineProcessor.CmdsHistory.History.LastOrDefault();
            context.CommandLineProcessor.AssertCommandLineProcessorHasACommandLineReader();
            if (lastCmd != null) context.CommandLineProcessor.CommandLineReader.SendNextInput(lastCmd);
            return new CommandResult<string>(lastCmd);
        }

        [Command("repeat the command specified by absolute or relative line number in command history list")]
        [CommandName("!")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.history)]
        [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static", Justification = "<En attente>")]
        public CommandResult<string> HistoryPreviousCommand(
            CommandEvaluationContext context,
            [Parameter("line number in the command history list if positive, else current command minus n if negative (! -1 equivalent to !!)")] int n
            )
        {
            var h = context.CommandLineProcessor.CmdsHistory.History;
            var index = (n < 0) ? h.Count + n : n - 1;
            string lastCmd;
            if (index < 0 || index >= h.Count)
            {
                context.Errorln($"line number out of bounds of commands history list (1..{h.Count})");
                return new CommandResult<string>(ReturnCode.Error);
            }
            else
            {
                lastCmd = h[index];
                context.CommandLineProcessor.AssertCommandLineProcessorHasACommandLineReader();
                context.CommandLineProcessor.CommandLineReader.SendNextInput(lastCmd);
            }
            return new CommandResult<string>(lastCmd);
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

using OrbitalShell.Component.CommandLine.CommandModel.Attributes;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Lib;
using OrbitalShell.Lib.FileSystem;

using static OrbitalShell.Component.EchoDirective.Shortcuts;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Hist.cs
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static", Justification = "<En attente>")]
    public partial class ShellCommands
    {
        [Command("display or execute commands from the history list")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.history)]
        public CommandVoidResult Fc(
            CommandEvaluationContext context,
            //[MinValue][MaxValue]
            [Parameter(0, "range first index (min is 1)", true, -1)] int first,
            [Parameter(1, "range last index", true, -1)] int last,

            [Option("e", "editor", "select which editor to use.  Default is $FC, edit", true, true)] string editor,

            [Option("l", "list", "omit line numbers when listing")] bool list,
            [Option("r", "reverse", "reverse the order of the lines (newest listed first)")] bool reverse,
            [Option("n", "omit", "omit line numbers when listing")] bool omit

            )
        {
            var hist = context.CommandLineProcessor.CmdsHistory.History;

            if (first == -1 && last == -1)
            {
                first = 1;
                last = hist.Count;
            }

            if ((first != -1 && last == -1) || (first == -1 && last != -1)
                || first < 1 || last > hist.Count)
                throw new InvalidDataException("history specification out of range");

            var histo = hist.Skip(first - 1).Take(last - first + 1);
            var max = hist.Count.ToString().Length;
            var f = DefaultForegroundCmd;
            string hp;

            if (list)
            {
                // list

                var i = first;
                foreach (var h in histo)
                {
                    if (!omit)
                        context.Out.Echo($"{context.ShellEnv.Colors.Numeric}{i.ToString().PadRight(max + 2, ' ')}{f}");
                    context.Out.ConsolePrint(h, true);
                    i++;
                }
            }
            else
            {
                // editor

                var tmpFile = Path.GetTempFileName();
                File.WriteAllLines(
                    tmpFile,
                    histo);

                if (string.IsNullOrWhiteSpace(editor))
                    context.ShellEnv.GetValue<string>(ShellEnvironmentVar.FC);
                if (string.IsNullOrWhiteSpace(editor))
                    editor = "edit";

                context.CommandLineProcessor.Eval(
                    context,
                    $"{editor} \"{tmpFile.Unslash()}\"{(editor == "edit" ? " -r" : "")}");

                File.Delete(tmpFile);
            }

            return CommandVoidResult.Instance;
        }

        [Command("displays the commands history list or manipulate it")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.history)]
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

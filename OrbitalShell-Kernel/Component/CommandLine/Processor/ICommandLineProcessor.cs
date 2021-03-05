using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using OrbitalShell.Component.CommandLine.Batch;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.Shell.Module;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public interface ICommandLineProcessor
    {
        string[] Args { get; }
        CancellationTokenSource CancellationTokenSource { get; set; }
        CommandsHistory CmdsHistory { get; set; }
        ICommandBatchProcessor CommandBatchProcessor { get; }
        CommandEvaluationContext CommandEvaluationContext { get; }
        IExternalParserExtension ExternalParserExtension { get; }
        CommandLineReader CommandLineReader { get; set; }
        ICommandsAlias CommandsAlias { get; }
        IConsole Console { get; }
        bool IsCancellationRequested { get; }
        bool IsInitialized { get; set; }
        IModuleManager ModuleManager { get; }

        // TODO: add IHookManager HookManager here
        // remove from module manager
        
        ICommandLineProcessorSettings Settings { get; }
        ISyntaxAnalyser SyntaxAnalyzer { get; }

        bool ExistsInPath(
            CommandEvaluationContext context,
            string cmdName,
            out string filePath);
        void AssertCommandLineProcessorHasACommandLineReader();
        void Error(string message = null, bool log = false, bool lineBreak = true, string prefix = "");
        ExpressionEvaluationResult Eval(CommandEvaluationContext context, MethodInfo commandMethodInfo, string args, int outputX = 0, string postAnalysisPreExecOutput = null);
        ExpressionEvaluationResult Eval(CommandEvaluationContext context, string expr, int outputX = 0, string postAnalysisPreExecOutput = null);
        void Init(string[] args, ICommandLineProcessorSettings settings, CommandEvaluationContext context = null);
        void PostInit();
        void PrintInfo(CommandEvaluationContext context);
        void SetArgs(string[] args);
        void SetArgs(string[] args, CommandEvaluationContext context, List<string> appliedSettings);
        int ShellExec(CommandEvaluationContext context, string comPath, string args, string workingDirectory = null, bool waitForExit = true, bool isStreamsEchoEnabled = true, bool isOutputCaptureEnabled = true, bool mergeErrorStreamIntoOutput = true);
        bool ShellExec(CommandEvaluationContext context, string com, string args, out CommandVoidResult returnCommandResult);
        int ShellExec(CommandEvaluationContext context, string comPath, string args, out string output, bool waitForExit = true, bool isStreamsEchoEnabled = true, bool isOutputCaptureEnabled = true, bool mergeErrorStreamIntoOutput = true);
        int ShellExec(CommandEvaluationContext context, string comPath, string args, string workingDirectory, out string output, bool waitForExit = true, bool isStreamsEchoEnabled = true, bool isOutputCaptureEnabled = true, bool mergeErrorStreamIntoOutput = true);
    }
}
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using OrbitalShell.Component.CommandLine.Batch;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Reader;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Shell.Module;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public interface ICommandLineProcessor
    {
        string[] Args { get; }
        CancellationTokenSource CancellationTokenSource { get; set; }
        CommandsHistory CmdsHistory { get; set; }
        CommandBatchProcessor CommandBatchProcessor { get; }
        CommandEvaluationContext CommandEvaluationContext { get; }
        CommandLineProcessorExternalParserExtension CommandLineProcessorExternalParserExtension { get; }
        CommandLineReader CommandLineReader { get; set; }
        CommandsAlias CommandsAlias { get; }
        IDotNetConsole Console { get; }
        bool IsCancellationRequested { get; }
        bool IsInitialized { get; set; }
        ModuleManager ModuleManager { get; }
        ICommandLineProcessorSettings Settings { get; }
        SyntaxAnalyser SyntaxAnalyzer { get; }

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
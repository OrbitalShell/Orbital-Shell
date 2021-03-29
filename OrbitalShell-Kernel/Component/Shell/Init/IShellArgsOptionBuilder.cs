using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Init
{
    public interface IShellArgsOptionBuilder
    {
        bool HasArg(ShellArg arg);
        ShellArgsOptionBuilder ImportSettingsFromJSon(CommandEvaluationContext context, ref List<ShellArgValue> appliedArgs);
        ShellArgsOptionBuilder ImportSettingsFromJSonFile(CommandEvaluationContext context,string path);
        void SetArgs(string[] args);
        ShellArgValue GetArg(ShellArg arg);
        ShellArgsOptionBuilder SetCommandLineProcessorOptions(CommandEvaluationContext context, ref List<ShellArgValue> appliedArgs);
        ShellArgsOptionBuilder SetShellEnvVariable(CommandEvaluationContext context, string name, string value);
        bool IsArg(ShellArg argSpec, string argName);
        ShellArgsOptionBuilder SetCommandOperationContextOptions(CommandEvaluationContext context, ref List<ShellArgValue> appliedArgs);
    }
}

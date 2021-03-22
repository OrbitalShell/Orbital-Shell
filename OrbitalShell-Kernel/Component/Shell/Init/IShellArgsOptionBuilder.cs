using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Init
{
    public interface IShellArgsOptionBuilder
    {
        bool HasArg(string name);
        ShellArgsOptionBuilder ImportSettingsFromJSon(string path);
        void SetArgs(string[] args);
        ShellArgsOptionBuilder SetCommandLineProcessorOptions(CommandEvaluationContext context, ref List<ShellArgValue> appliedArgs);
        ShellArgsOptionBuilder SetVariable(CommandEvaluationContext context, string name, string value);
    }
}
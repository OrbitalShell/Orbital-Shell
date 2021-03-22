using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;

namespace OrbitalShell.Component.Shell.Init
{
    public interface IShellBootstrap
    {
        void CreateRestoreUserAliasesFile();
        void CreateRestoreUserHistoryFile();
        ICommandLineProcessor GetCommandLineProcessor();
        void InitShellInitFolder();
        void InitUserProfileFolder();
        ShellBootstrap Run();
        void ShellInit(string[] args, IConsole console, ICommandLineProcessorSettings settings, CommandEvaluationContext context = null);
    }
}
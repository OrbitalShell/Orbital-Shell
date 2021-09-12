using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;

namespace OrbitalShell.Commands.Shell
{
    [Commands("commands of the command line processor")]
    [CommandsNamespace(CommandNamespace.shell)]
    public partial class ShellCommands : ICommandsDeclaringType
    {
        #region console app

        [Command("exit the shell")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.app)]
        public CommandResult<int> Exit(
            CommandEvaluationContext context
            )
        {
            context.CommandLineProcessor.Console.Exit();
            return new CommandResult<int>(0);
        }

        [Command("print command processor infos")]
        public CommandVoidResult Cpinfo(
            CommandEvaluationContext context
            )
        {
            context.CommandLineProcessor.PrintInfo(context);
            return new CommandVoidResult();
        }

        #endregion
    }
}

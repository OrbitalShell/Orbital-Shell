using OrbitalShell.Component.CommandLine;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Variable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using OrbitalShell.Lib.FileSystem;

namespace OrbitalShell.Commands.Shell
{
    public partial class ShellCommands : ICommandsDeclaringType
    {
        private static readonly string _rshdPathVar = "rshdPath";
        private static readonly string _rshdFilename = "orbsh-deamon.exe";
        private static readonly string _rshNSVar = Variables.Nsp(ShellEnvironmentNamespace.com + "", "rsh");

        [Command("runs the remote shell deamon")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.net)]
        [RequireOSCommand("orbsh-deamon.exe")]
        [SuppressMessage("Performance", "CA1822")]
        public CommandResult<int> Rshd(
            CommandEvaluationContext context
            )
        {
            var rshdPath = GetRshdPath(context);
            context.CommandLineProcessor.ShellExec(context, rshdPath, string.Empty, out var result);            
            return new CommandResult<int>(result.ReturnCode);
        }

        static string GetRshdPath(CommandEvaluationContext context)
        {
            var rshdPath = context.ShellEnv.GetValue<string>(_rshNSVar, _rshdPathVar, false);
            if (rshdPath==null)
            {
                context.ShellEnv.AddNew(_rshNSVar, _rshdPathVar, _rshdFilename);
                rshdPath = _rshdFilename;
            }
            if (!Path.IsPathFullyQualified(rshdPath))
            {
                rshdPath = Path.Combine(context.ShellEnv.GetValue<DirectoryPath>(ShellEnvironmentVar.shell).FullName,rshdPath);
            }
            return rshdPath;
        }
    }
}

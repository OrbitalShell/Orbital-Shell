using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine;
using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Module.PromptGitInfo
{
    /// <summary>
    /// module commands : prompt git infos
    /// </summary>
    [Commands("prompt git info module commands")]
    [CommandsNamespace(CommandNamespace.tools, "git")]
    [Hooks]
    public class PromptGitInfoCommands : ICommandsDeclaringType
    {
        /// <summary>
        /// enable or disable prompt git info
        /// </summary>
        /// <param name="isEnabled">true means enabling, false disabling</param>
        [Command("enable/disable prompt git infos")]
        public CommandVoidResult SetIsEnabled(
            CommandEvaluationContext context,
            [Parameter("if true enable the cusom prompt, else disable it")] bool isEnabled
        )
        {

            return CommandVoidResult.Instance;
        }

        [Hook(Hooks.ModuleInit)]
        public void Init(CommandEvaluationContext context)
        {

        }

        [Hook(Hooks.PromptOutputEnd)]
        public void PromptOutputEnd(CommandEvaluationContext context)
        {

        }
    }
}
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Parsing;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class CommandLineProcessorExternalParserExtension
        : ICommandLineParserExtension
    {
        public readonly CommandLineProcessor CommandLineProcessor;

        public CommandLineProcessorExternalParserExtension(CommandLineProcessor commandLineProcessor)
        {
            CommandLineProcessor = commandLineProcessor;
        }

        public bool TryGetCommandSpecificationFromExternalToken(
            ExternalParserExtensionContext externalParserExtensionContext,
            out CommandSpecification commandSpecification,
            out string commandPath)
        {
            commandSpecification = null;
            commandPath = null;

            // search in path in case of can delegate to os call to exec file or script
            if (CommandLineProcessor.ExistsInPath(
                externalParserExtensionContext.Context,
                externalParserExtensionContext.TokenName,
                out commandPath))
            {
                // get the 'exec' command
                var execComMethodInfo =
                    typeof(CommandLineProcessorCommands)
                    .GetMethod(nameof(CommandLineProcessorCommands.Exec));
                commandSpecification = CommandLineProcessor
                    .ModuleManager
                    .ModuleCommandManager
                    .GetCommandSpecification(execComMethodInfo);

                return true;
            }

            return false;
        }
    }
}
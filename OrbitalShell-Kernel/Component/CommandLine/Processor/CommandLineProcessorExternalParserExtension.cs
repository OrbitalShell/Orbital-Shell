using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Parsing.Parser;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class CommandLineProcessorExternalParserExtension
        : IExternalParserExtension
    {
        public ICommandLineProcessor CommandLineProcessor { get; set; }

        /// <summary>
        /// this instance is not initialized (no parameter commandLineProcessor) and thus can't be used before further init
        /// </summary>
        public CommandLineProcessorExternalParserExtension()
        {
        }

        /*public CommandLineProcessorExternalParserExtension(ICommandLineProcessor commandLineProcessor)
        {
            CommandLineProcessor = commandLineProcessor;
        }*/

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
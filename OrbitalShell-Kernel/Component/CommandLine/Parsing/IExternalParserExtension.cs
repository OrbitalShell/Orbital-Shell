using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    /// <summary>
    /// command line parse extension class interface
    /// </summary>
    public interface IExternalParserExtension
    {
        ICommandLineProcessor CommandLineProcessor { get; set; }

        bool TryGetCommandSpecificationFromExternalToken(
            ExternalParserExtensionContext externalParserExtensionContext,
            out CommandSpecification commandSpecification,
            out string commandPath
            );
    }
}
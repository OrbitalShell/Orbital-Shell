using OrbitalShell.Component.CommandLine.CommandModel;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    /// <summary>
    /// command line parse extension class interface
    /// </summary>
    public interface ICommandLineParserExtension
    {
        bool TryGetCommandSpecificationFromExternalToken(
            ExternalParserExtensionContext externalParserExtensionContext,
            out CommandSpecification commandSpecification,
            out string commandPath
            );
    }
}
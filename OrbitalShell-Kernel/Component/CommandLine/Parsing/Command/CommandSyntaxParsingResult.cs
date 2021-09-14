using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.Parsing.Command.Parameter;
using OrbitalShell.Component.CommandLine.Parsing.Parser.Error;

namespace OrbitalShell.Component.CommandLine.Parsing.Command
{
    public class CommandSyntaxParsingResult
    {
        public readonly CommandSyntax CommandSyntax;
        public readonly MatchingParameters MatchingParameters;
        public readonly List<ParseError> ParseErrors;

        public CommandSyntaxParsingResult(
            CommandSyntax commandSyntax,
            MatchingParameters matchingParameters,
            List<ParseError> parseErrors
            )
        {
            CommandSyntax = commandSyntax;
            MatchingParameters = matchingParameters;
            ParseErrors = parseErrors;
        }
    }
}

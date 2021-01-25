using System.Collections.Generic;

namespace OrbitalShell.Component.CommandLine.Parsing
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

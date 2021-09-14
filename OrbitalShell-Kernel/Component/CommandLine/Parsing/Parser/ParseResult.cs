using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.Parsing.Command;

namespace OrbitalShell.Component.CommandLine.Parsing.Parser
{
    public class ParseResult
    {
        public readonly ParseResultType ParseResultType;
        public readonly List<CommandSyntaxParsingResult> SyntaxParsingResults;

        public ParseResult(
            ParseResultType parseResultType,
            List<CommandSyntaxParsingResult> syntaxParsingResults
            )
        {
            ParseResultType = parseResultType;
            SyntaxParsingResults = syntaxParsingResults;
        }
    }
}

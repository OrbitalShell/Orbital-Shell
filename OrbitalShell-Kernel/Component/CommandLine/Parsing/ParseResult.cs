using System.Collections.Generic;

namespace OrbitalShell.Component.CommandLine.Parsing
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

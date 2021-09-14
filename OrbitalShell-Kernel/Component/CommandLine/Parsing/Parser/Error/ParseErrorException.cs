using System;

namespace OrbitalShell.Component.CommandLine.Parsing.Parser.Error
{
    public class ParseErrorException : Exception
    {
        public readonly ParseError ParseError;

        public ParseErrorException(ParseError parseError) : base(parseError.Description)
        {
            ParseError = parseError;
        }
    }
}

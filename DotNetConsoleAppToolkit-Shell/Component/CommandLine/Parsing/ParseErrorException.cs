using System;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
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

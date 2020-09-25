using DotNetConsoleAppToolkit.Component.CommandLine.Pipeline;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
{
    public class PipelineParseResult
    {
        public readonly ParseResult ParseResult;
        public readonly PipelineWorkUnit WorkUnit;
        public PipelineParseResult Next;

        public PipelineParseResult(PipelineWorkUnit workUnit, ParseResult parseResult)
        {
            ParseResult = parseResult;
            WorkUnit = workUnit;
        }

        public PipelineParseResult(ParseResult parseResult)
        {
            ParseResult = parseResult;
        }

        public PipelineParseResult(PipelineWorkUnit workUnit)
        {
            ParseResult = new ParseResult(ParseResultType.Empty, null);
            WorkUnit = workUnit;
        }

        public PipelineParseResult()
        {
            ParseResult = new ParseResult(ParseResultType.Empty, null);
        }
    }
}

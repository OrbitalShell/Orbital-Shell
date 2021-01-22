using DotNetConsoleAppToolkit.Component.CommandLine.Pipeline;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
{
    public class PipelineParseResult
    {
        public readonly ParseResult ParseResult;
        public readonly PipelineWorkUnit WorkUnit;
        public PipelineParseResult Next;
        public string Expr;

        public PipelineParseResult(string expr,PipelineWorkUnit workUnit, ParseResult parseResult)
        {
            ParseResult = parseResult;
            WorkUnit = workUnit;
            Expr = expr;
        }

        public PipelineParseResult(string expr,ParseResult parseResult)
        {
            ParseResult = parseResult;
            Expr = expr;
        }

        public PipelineParseResult(string expr,PipelineWorkUnit workUnit)
        {
            ParseResult = new ParseResult(ParseResultType.Empty, null);
            WorkUnit = workUnit;
            Expr = expr;
        }

        public PipelineParseResult()
        {
            ParseResult = new ParseResult(ParseResultType.Empty, null);
        }
    }
}

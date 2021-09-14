using OrbitalShell.Component.CommandLine.Parsing.Parser;
using OrbitalShell.Component.CommandLine.Pipeline;

namespace OrbitalShell.Component.CommandLine.Parsing.Pipeline
{
    public class PipelineParseResult
    {
        public readonly ParseResult ParseResult;
        public readonly PipelineWorkUnit WorkUnit;
        public PipelineParseResult Next;
        public string Expr;

        public PipelineParseResults PipelineParseResults;

        public PipelineParseResult(string expr, PipelineParseResults pipelineParseResults, PipelineWorkUnit workUnit, ParseResult parseResult)
        {
            ParseResult = parseResult;
            PipelineParseResults = pipelineParseResults;
            WorkUnit = workUnit;
            Expr = expr;
        }

        public PipelineParseResult(string expr, PipelineParseResults pipelineParseResults, ParseResult parseResult)
        {
            ParseResult = parseResult;
            PipelineParseResults = pipelineParseResults;
            Expr = expr;
        }

        public PipelineParseResult(string expr, PipelineParseResults pipelineParseResults, PipelineWorkUnit workUnit)
        {
            ParseResult = new ParseResult(ParseResultType.Empty, null);
            PipelineParseResults = pipelineParseResults;
            WorkUnit = workUnit;
            Expr = expr;
        }

        public PipelineParseResult(PipelineParseResults pipelineParseResults)
        {
            PipelineParseResults = pipelineParseResults;
            ParseResult = new ParseResult(ParseResultType.Empty, null);
        }
    }
}

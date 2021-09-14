using System.Collections.Generic;

namespace OrbitalShell.Component.CommandLine.Parsing.Pipeline
{
    public class PipelineParseResults : List<PipelineParseResult>
    {
        public PipelineParseResults() { }

        public PipelineParseResults(PipelineParseResult pipelineParseResult)
        {
            Add(pipelineParseResult);
        }
    }
}
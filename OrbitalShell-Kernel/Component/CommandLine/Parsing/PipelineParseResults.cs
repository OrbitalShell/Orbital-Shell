using System.Collections.Generic;

namespace OrbitalShell.Component.CommandLine.Parsing
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
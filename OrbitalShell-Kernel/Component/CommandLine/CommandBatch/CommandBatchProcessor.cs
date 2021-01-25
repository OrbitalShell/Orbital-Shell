using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Lib;
using static OrbitalShell.Component.CommandLine.Parsing.CommandLineSyntax;
using System;

namespace OrbitalShell.Component.CommandLine.CommandBatch
{
    public class CommandBatchProcessor
    {
        public void RunBatch(CommandEvaluationContext context,string path)
        {
            var (lines, eol, separator) = TextFileReader.ReadAllLines(path);
            RunBatch(context,lines);
        }

        public void RunBatchText(CommandEvaluationContext context, string batch)
        {
            if (string.IsNullOrWhiteSpace(batch)) return;
            var lines = batch.Split(Environment.NewLine);
            RunBatch(context,lines);
        }

        void RunBatch(CommandEvaluationContext context, string[] batchLines)
        {
            foreach ( var line in batchLines )
            {
                var s = line.Trim();
                if (!s.StartsWith(BatchCommentBegin) && !string.IsNullOrEmpty(s))
                    context.CommandLineProcessor.Eval(context, s, 0);
            }
        }
    }
}

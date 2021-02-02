using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Lib;
using static OrbitalShell.Component.CommandLine.Parsing.CommandLineSyntax;
using System;

namespace OrbitalShell.Component.CommandLine.CommandBatch
{
    public class CommandBatchProcessor
    {
        public int RunBatch(CommandEvaluationContext context, string path)
        {
            var (lines, eol, separator) = TextFileReader.ReadAllLines(path);
            return RunBatch(context, lines);
        }

        public int RunBatchText(CommandEvaluationContext context, string batch)
        {
            if (string.IsNullOrWhiteSpace(batch)) return (int)ReturnCode.OK;
            var lines = batch.Split(Environment.NewLine);
            return RunBatch(context, lines);
        }

        int RunBatch(CommandEvaluationContext context, string[] batchLines)
        {
            var ret = (int)ReturnCode.OK;
            foreach (var line in batchLines)
            {
                var s = line.Trim();
                if (!s.StartsWith(BatchCommentBegin) && !string.IsNullOrEmpty(s))
                {
                    var r = context.CommandLineProcessor.Eval(context, s, 0);
                    if (r.EvalResultCode != (int)ReturnCode.OK) ret = (int)ReturnCode.Error;
                }
            }
            return ret;
        }
    }
}

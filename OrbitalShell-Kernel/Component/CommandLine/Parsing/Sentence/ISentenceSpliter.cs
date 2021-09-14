using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;

namespace OrbitalShell.Component.CommandLine.Parsing.Sentence
{
    public interface ISentenceSpliter
    {
        char FindMetaChar(ref string expr);
        bool IsTopLevelSeparator(char c);
        StringSegment[] SplitExpr(CommandEvaluationContext _, string expr);
    }
}
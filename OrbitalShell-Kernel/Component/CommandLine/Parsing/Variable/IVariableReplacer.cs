using System.Collections.Generic;

using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.CommandLine.Parsing.Variable
{
    public interface IVariableReplacer
    {
        (string expr, Dictionary<string, object> references) SubstituteVariables(CommandEvaluationContext _, string expr);
    }
}
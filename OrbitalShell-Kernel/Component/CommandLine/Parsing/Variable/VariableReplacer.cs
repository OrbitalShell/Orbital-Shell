using System;
using System.Collections.Generic;
using System.Text;

using OrbitalShell.Component.CommandLine.Parsing.Command;
using OrbitalShell.Component.CommandLine.Parsing.Sentence;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Lib;

using static OrbitalShell.Component.CommandLine.Parsing.Sentence.CommandLineSyntax;
using static OrbitalShell.Component.Shell.Variable.Variables;

namespace OrbitalShell.Component.CommandLine.Parsing.Variable
{
    public class VariableReplacer : IVariableReplacer
    {
        /// <summary>
        /// substitue any var ($..) found in expr (expr is a word from the command line)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public (string expr, Dictionary<string, object> references)
            SubstituteVariables(
                CommandEvaluationContext _,
                string expr
            )
        {
            Dictionary<string, object> references = new Dictionary<string, object>();

            /*
             * if expr is equal to a single var ref :
             *  - var (propagate object to the command...)
             * if expr contains one or several var refs :
             *  - var.ToString()
             */
            var origStr = new string(expr);
            char[] t;
            int totalRemaining;
            int searchScopeEndIndex = expr.Length;

            do
            {
                totalRemaining = 0;
                var i = 0;
                var vars = new List<VariableStringSegment>();
                t = expr.ToCharArray();
                int nextSearchScopeEndIndex = searchScopeEndIndex;
                searchScopeEndIndex = Math.Min(t.Length, searchScopeEndIndex);

                while (i < searchScopeEndIndex && i < t.Length)
                {
                    var c = t[i];
                    char? previousChar = i > 0 ? t[i - 1] : null;

                    if (c == CommandLineSyntax.VariablePrefix
                        && previousChar != NeutralizerSymbol)
                    {
                        (string varName,
                            int beginIndex,
                            int lastIndex,
                            bool isNameCaptured,
                            int remaining,
                            bool _) =
                            VariableSyntax.ReadVariableName(
                                ref t,
                                i + 1,
                                searchScopeEndIndex);
                        totalRemaining += remaining;
                        vars.Add(
                            new VariableStringSegment(
                                varName,
                                beginIndex,
                                lastIndex,
                                lastIndex - beginIndex + 1,
                                isNameCaptured
                                ));
                        i = lastIndex;
                    }
                    i++;
                }
                searchScopeEndIndex = nextSearchScopeEndIndex;

                if (vars.Count > 0)
                {
                    var nexpr = new StringBuilder();
                    int x = 0;
                    VariableStringSegment lastvr = null;
                    VariableStringSegment currentVr = null;
                    foreach (var vr in vars)
                    {
                        currentVr = vr;
                        lastvr = vr;
                        nexpr.Append(expr[x..vr.X]);
                        try
                        {
                            _.Variables.Get(vr.Text, out var value);

                            // here: value is transformed by his ToString method
                            // (var is substituted by its text)

                            if (vars.Count == 1 &&
                                (vr.FullSyntax == origStr))
                            {
                                // single var (no conversion)
                                // keep var ref in place (arbitrary convention)
                                var varName = origStr;
                                nexpr.Append(varName);
                                references.AddOrReplace(varName, value);
                            }
                            else
                            {
                                var o = (value is DataValue dv) ? dv?.Value : value;
                                if (o == null)
                                    nexpr.Append(o);
                                else
                                {
                                    CommandSyntax.TryCastToString(_, o, out var strValue);
                                    nexpr.Append(strValue);
                                }
                            }
                        }
                        catch (VariablePathNotFoundException ex)
                        {
                            _.Errorln(ex.Message);
                            // Bash convention: replace name by empty
                            nexpr.Append("");
                        }
                        x = vr.Y + 1;
                        totalRemaining--;
                    }

                    if (lastvr != null)
                        nexpr.Append(
                            (x < expr.Length) ?
                                expr[x..]
                                : "");

                    expr = nexpr.ToString();
                }

            } while (totalRemaining > 0);

            return (expr, references);
        }

        // TODO: check this
#if experiment
        // TODO: remove or explain

        public static void StoreReference(
            CommandEvaluationContext context,
            string reference,
            object obj
            )
        {
            context.Variables.Set(VariableNamespace.Local, reference, obj);
        }

        public static object GetReference(
            CommandEvaluationContext context,
            string reference
            )
        {
            context.Variables.Get(VariableNamespace.Local, reference, out var value, false);
            return value;
        }
#endif
    }
}

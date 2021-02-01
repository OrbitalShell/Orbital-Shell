//#define debugParser

using OrbitalShell.Component.CommandLine.Data;
using OrbitalShell.Component.CommandLine.Pipeline;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.CommandLine.Variable;
using OrbitalShell.Console;
using OrbitalShell.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using static OrbitalShell.Component.CommandLine.Parsing.CommandLineSyntax;
using static OrbitalShell.Component.CommandLine.Pipeline.PipelineParser;
using static OrbitalShell.Component.CommandLine.Variable.Variables;
using static OrbitalShell.DotNetConsole;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public static class CommandLineParser
    {
        public static StringComparison SyntaxMatchingRule = StringComparison.InvariantCultureIgnoreCase;

        /// <summary>
        /// parse top level syntax of a command line
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static StringSegment[] SplitExpr(
            CommandEvaluationContext context,
            string expr
            )
        {
            if (expr == null) return new StringSegment[] { };
            var splits = new List<StringSegment>();
            var t = expr.Trim().ToCharArray();

            char prevc = SpaceSeparator;
            int i = 0;

            var inString = false;
            var inSingleQuoteString = false;
            var inDoubleQuoteString = false;
            int beginCurStrIndex = 0;
            var prevStr = "";
            var k = t.Length;

            while (i < k)
            {
                var c = t[i];
                var neutralizedBySymbol = prevc == NeutralizerSymbol;
                var isSeparator = c == SpaceSeparator;
                var isStringSeparator = !neutralizedBySymbol && (c == SingleQuote || c == DoubleQuote);
                var isSingleQuoteStringSeparator = c == SingleQuote && !neutralizedBySymbol;
                var isDoubleQuoteStringSeparator = c == DoubleQuote && !neutralizedBySymbol;

                if (!inString)
                {
                    if (isStringSeparator)
                    {
                        prevStr = ((i - 1) >= beginCurStrIndex) ? new string(t[beginCurStrIndex..i]) : "";
                        inSingleQuoteString = isSingleQuoteStringSeparator;
                        inDoubleQuoteString = isDoubleQuoteStringSeparator;
                        inString = true;
                        beginCurStrIndex = i + 1;
                    }

                    if (isSeparator && (i - 1) >= beginCurStrIndex)
                    {
                        if ((i - 1) >= beginCurStrIndex) splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]), beginCurStrIndex, i - 1));
                        prevStr = "";
                        beginCurStrIndex = i + 1;
                    }
                }
                else
                {
                    if ((inSingleQuoteString && isSingleQuoteStringSeparator)
                        || (inDoubleQuoteString && isDoubleQuoteStringSeparator))
                    {
                        if (i == t.Length - 1)
                        {
                            splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]), beginCurStrIndex, i - 1));
                            prevStr = "";
                            beginCurStrIndex = t.Length;
                        }
                        else
                        {
                            var t2 = t[0..(beginCurStrIndex - 1)].ToList();
                            t2.AddRange(t[beginCurStrIndex..i].ToList());
                            if (i < t.Length - 1)
                                t2.AddRange(t[(i + 1)..t.Length].ToList());
                            t = t2.ToArray();
                            k = t.Length;
                            beginCurStrIndex--;
                            i -= 2;
                        }
                        inSingleQuoteString = inDoubleQuoteString = false;
                        inString = false;
                    }
                }
                prevc = c;
                i++;
            }

            if ((t.Length - 1) >= beginCurStrIndex)
            {
                splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..(t.Length)]), beginCurStrIndex, i - 1));
                prevStr = "";
            }
#if debugParser
            for (i = 0; i < splits.Count; i++)
            {
                context.Out.Echoln($"(f=darkcyan){i} : {splits[i]}");
            }
#endif
            splits = splits.Where(x => !string.IsNullOrEmpty(x.Text)).ToArray().ToList();
            return splits.ToArray();
        }

#if NO
        /// <summary>
        /// split an expression to be evaluted at top level syntax level
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expr"></param>
        /// <returns>segments of syntaxes to be evaluated</returns>
        public static string[] SplitExpr0(
            CommandEvaluationContext context,
            string expr)
        {
            // @TODO: - add pipes syntaxs as spliters syntax + manage ' and " neutralization (even on variable syntaxe)
            // @TODO: - add streams syntaxes as splitters syntax
            // must returns splits in a pipeline execution model instance
            if (expr == null) return new string[] { };
            var splits = new List<string>();
            var t = expr.Trim().ToCharArray();
            var inQuotedStr = false;
            int i = 0;
            var curStr = "";
            char prevc = ' ';
            while (i < t.Length)
            {
                var c = t[i];
                if (!inQuotedStr)
                {
                    if (c == ' ')
                    {
                        splits.Add(curStr);
                        curStr = "";
                    }
                    else
                    {
                        if (c == '"')
                            inQuotedStr = true;
                        else
                            curStr += c;
                    }
                }
                else
                {
                    if (c == '"' && prevc != '\\')
                        inQuotedStr = false;
                    else
                        curStr += c;
                }
                prevc = c;
                i++;
            }
            if (!string.IsNullOrWhiteSpace(curStr))
                splits.Add(curStr);
            return splits.ToArray();
        }
#endif

        public static int GetIndex(
            CommandEvaluationContext context,
            int position,
            string expr)
        {
            var splits = SplitExpr(context, expr);
            var n = 0;
            for (int i = 0; i <= position && i < splits.Length; i++)
                n += splits[i].Length + ((i > 0) ? 1 : 0);
            return n;
        }

#if experiment

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

        /// <summary>
        /// substitue any var ($..) found in expr (expr is a word from the command line)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static (string expr, Dictionary<string, object> references) SubstituteVariables(
            CommandEvaluationContext context,
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

            var t = expr.ToCharArray();
            var i = 0;
            var vars = new List<StringSegment>();

            while (i < t.Length)
            {
                var c = t[i];
                if (c == CommandLineSyntax.VariablePrefix && (i == 0 || t[i - 1] != '\\'))
                {
                    var j = VariableSyntax.FindEndOfVariableName(t, i + 1);
                    var varName = expr.Substring(i + 1, j - i);

                    // accept terminator symbol (if any) as var name if var name is empty
                    if (varName == "" && (j + 1) < t.Length)
                    {
                        j++;
                        varName = t[j] + "";
                    }

                    vars.Add(new StringSegment(varName, i, j, j - i + 1));
                    i = j;
                }
                i++;
            }

            if (vars.Count > 0)
            {
                var nexpr = new StringBuilder();
                int x = 0;
                StringSegment lastvr = null;
                foreach (var vr in vars)
                {
                    lastvr = vr;
                    nexpr.Append(expr.Substring(x, vr.X - x));
                    try
                    {
                        context.Variables.Get(vr.Text, out var value);

                        // here: value is transformed by his ToString method
                        // (var is substituted by its text)

                        if (vars.Count == 1 &&
                            ((CommandLineSyntax.VariablePrefix + vr.Text) == origStr))
                        {
                            // single var (no conversion)
                            // keep var ref in place (arbitrary convention)
                            var varName = CommandLineSyntax.VariablePrefix + vr.Text;
                            nexpr.Append(varName);
                            references.AddOrReplace(varName, value);
                        }
                        else
                        {
                            var o = (value is DataValue dv) ? dv.Value : value;
                            if (o == null)
                                nexpr.Append(o);
                            else
                            {
                                var (success, strValue) = CommandSyntax.TryCastToString(context, o);
                                nexpr.Append(strValue);
                            }
                        }
                    }
                    catch (VariablePathNotFoundException ex)
                    {
                        context.Errorln(ex.Message);
                        // keep bad var name in place (? can be option of the shell. Bash let it blank)
                        nexpr.Append(CommandLineSyntax.VariablePrefix + vr.Text);
                    }
                    x = vr.Y + 1;
                }
                if (lastvr != null)
                {
                    nexpr.Append(expr.Substring(x));
                }
                expr = nexpr.ToString();
            }

            return (expr, references);
        }

        public static PipelineParseResults Parse(
            CommandEvaluationContext context,
            SyntaxAnalyser syntaxAnalyzer,
            string expr,
            ICommandLineParserExtension commandLineParserExtension
            )
        {
            var parseResults = new PipelineParseResults();

            if (expr == null) return new PipelineParseResults(new PipelineParseResult(parseResults));

            expr = expr.Trim();
            if (string.IsNullOrWhiteSpace(expr)) return new PipelineParseResults(new PipelineParseResult(parseResults));

            var splits0 = SplitExpr(context, expr);

            try
            {
                var pipeline = GetPipeline(context, splits0);
                var workUnit = pipeline;
                var splits = new List<StringSegment>();
                var references = new Dictionary<string, object>();

                var token = workUnit.Segments.First()?.Text;
                if (token != null && context
                        .CommandLineProcessor
                        .CommandsAlias
                        .Aliases
                        .TryGetValue(token, out var alias)
                        && workUnit.Segments.Count == 1)
                {
                    expr = alias;
                    splits0 = SplitExpr(context, expr);
                    pipeline = GetPipeline(context, splits0);
                    workUnit = pipeline;
                }

                while (workUnit != null)
                {
                    splits.Clear();
                    foreach (var split in workUnit.Segments)
                    {
                        (string argExpr2, Dictionary<string, object> refs) = SubstituteVariables(context, split.Text);
                        foreach (var kv in refs) references.AddOrReplace(kv.Key, kv.Value);
                        splits.Add(new StringSegment(argExpr2, split.X, split.Y, refs));
                    }

                    parseResults.Add(
                        new PipelineParseResult(
                            expr,
                            parseResults,
                            workUnit,
                            ParseCmdSplits(             // parse the command line unit : /!\ May change the segments
                                context,
                                syntaxAnalyzer,
                                commandLineParserExtension,
                                expr,
                                workUnit,
                                splits)));

                    workUnit = workUnit.NextUnit;
                }
            }
            catch (ParseErrorException parseErrorEx)
            {
                // get pipeline parse error
                parseResults.Add(
                    new PipelineParseResult(
                        expr,
                        parseResults,
                        new ParseResult(
                            ParseResultType.SyntaxError,
                            new List<CommandSyntaxParsingResult>
                            {
                                new CommandSyntaxParsingResult(null,null,new List<ParseError>{ parseErrorEx.ParseError })
                            })));
            }
            PipelineParseResult ppr = null;
            foreach (var pr in parseResults)
            {
                if (ppr != null)
                    ppr.Next = pr;
                ppr = pr;
            }
            return parseResults;
        }

        /// <summary>
        /// parse a command line unit (find syntax from token)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syntaxAnalyzer"></param>
        /// <param name="expr"></param>
        /// <param name="splits"></param>
        /// <returns></returns>
        public static ParseResult ParseCmdSplits(
            CommandEvaluationContext context,
            SyntaxAnalyser syntaxAnalyzer,
            ICommandLineParserExtension commandLineParserExtension,
            string expr,
            PipelineWorkUnit workUnit,
            List<StringSegment> splits)
        {
            var segments = splits.Skip(1).ToArray();
            var token = splits.First().Text;

            // get potential syntaxes
            var ctokens = syntaxAnalyzer.FindSyntaxesFromToken(token, false, SyntaxMatchingRule);

            if (ctokens.Count == 0)
            {
                // check cmdspec from external token
                if (commandLineParserExtension != null &&
                    commandLineParserExtension
                        .TryGetCommandSpecificationFromExternalToken(
                            new ExternalParserExtensionContext(
                                context,
                                token,
                                segments.Select(x => x.Text).ToArray(),
                                expr),
                            out var externalCommand,
                            out var commandPath
                            ))
                {
                    if (externalCommand == null) throw new Exception($"external parser extension returns a null command specificaiton for external token: {token}");

                    var extValidSyntaxParsingResults = new List<CommandSyntaxParsingResult>();
                    var extCmdTokenSyntax = syntaxAnalyzer.FindSyntaxesFromToken(externalCommand.Name, false, SyntaxMatchingRule).FirstOrDefault();
                    if (extCmdTokenSyntax == null) throw new Exception($"missing kernel command: {externalCommand.Name}");

                    var args = string.Join(" ", segments.Select(x => /*"\"" +*/ x.Text /*+ "\""*/));
                    var map = new Dictionary<string, object>();
                    foreach (var mp in segments.Select(x => x.Map))
                        if (mp != null) map.Merge(mp);

                    var newSegments = new StringSegment[]
                    {
                        new StringSegment(commandPath,0,commandPath.Length-1,commandPath.Length),
                        new StringSegment(args,commandPath.Length+1,commandPath.Length+1+args.Length-1,args.Length,map)
                    };
                    splits.Clear();
                    splits.AddRange(newSegments);
                    workUnit.Segments = splits;

                    var (extMatchingParameters, extParseErrors) =
                        extCmdTokenSyntax.Match(
                            context,
                            SyntaxMatchingRule,
                            newSegments,
                            token.Length + 1);
                    var cspr = new CommandSyntaxParsingResult(
                        extCmdTokenSyntax,
                        extMatchingParameters,
                        extParseErrors
                    );
                    extValidSyntaxParsingResults.Add(cspr);
                    return new ParseResult(ParseResultType.Valid, extValidSyntaxParsingResults);
                }

                var cmdnotfound = new CommandSyntaxParsingResult(
                    null,
                    null,
                    new List<ParseError> {
                        new ParseError(
                            $"unknown command: {token}",
                            0,
                            0,
                            null) });
                return new ParseResult(ParseResultType.NotIdentified, new List<CommandSyntaxParsingResult> { cmdnotfound });
            }

            if (ctokens.Count > 0)
            {
                int nbValid = 0;
                var syntaxParsingResults = new List<CommandSyntaxParsingResult>();
                var validSyntaxParsingResults = new List<CommandSyntaxParsingResult>();

                foreach (var syntax in ctokens)
                {
                    var (matchingParameters, parseErrors) =
                        syntax.Match(
                            context,
                            SyntaxMatchingRule,
                            segments,
                            token.Length + 1);
                    if (parseErrors.Count == 0)
                    {
                        nbValid++;
                        validSyntaxParsingResults.Add(new CommandSyntaxParsingResult(syntax, matchingParameters, parseErrors));
                    }
                    else
                        syntaxParsingResults.Add(new CommandSyntaxParsingResult(syntax, matchingParameters, parseErrors));
                }

                if (nbValid > 1)
                {
                    // try disambiguization : priority to com with the maximum of options
                    validSyntaxParsingResults.Sort(
                        new Comparison<CommandSyntaxParsingResult>((x, y)
                            => x.CommandSyntax.CommandSpecification.OptionsCount.CompareTo(
                                y.CommandSyntax.CommandSpecification.OptionsCount
                                )
                        ));
                    validSyntaxParsingResults.Reverse();
                    if (validSyntaxParsingResults[0].CommandSyntax.CommandSpecification.OptionsCount >
                        validSyntaxParsingResults[1].CommandSyntax.CommandSpecification.OptionsCount)
                    {
                        validSyntaxParsingResults = new List<CommandSyntaxParsingResult>
                    {
                        validSyntaxParsingResults.First()
                    };
                        nbValid = 1;
                    }
                    else
                        return new ParseResult(ParseResultType.Ambiguous, validSyntaxParsingResults);
                }

                if (nbValid == 0)
                {
                    return new ParseResult(ParseResultType.NotValid, syntaxParsingResults);
                }

                if (nbValid == 1)
                {
                    return new ParseResult(ParseResultType.Valid, validSyntaxParsingResults);
                }

                throw new InvalidOperationException();
            }

            return null;
        }
    }
}

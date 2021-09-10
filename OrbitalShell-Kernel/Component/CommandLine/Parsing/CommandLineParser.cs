//#define debugParser

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OrbitalShell.Component.CommandLine.Pipeline;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Lib;

using static OrbitalShell.Component.CommandLine.Parsing.CommandLineSyntax;
using static OrbitalShell.Component.CommandLine.Pipeline.PipelineParser;
using static OrbitalShell.Component.Shell.Variable.Variables;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public static class CommandLineParser
    {
        public static StringComparison SyntaxMatchingRule { get; set; } = StringComparison.InvariantCultureIgnoreCase;

        /// <summary>
        /// split on SPACE " ' 
        /// parse top level syntax of a command line
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static StringSegment[] SplitExpr(
            CommandEvaluationContext _,
            string expr
            )
        {
            if (expr == null) return Array.Empty<StringSegment>();

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

            // find a meta char for private coding that is not in the string - try char 0 or a random in 10000-20000
            var metachar = FindMetaChar(ref expr);

            while (i < k)
            {
                var c = t[i];
                var isNeutralizer = c == NeutralizerSymbol;
                var neutralizedBySymbol = prevc == NeutralizerSymbol;
                bool isSeparator = IsTopLevelSeparator(c);

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

                    if (isSeparator)
                    {
                        // end of non separator pattern + begin of separator pattern

                        splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]), beginCurStrIndex, i - 1));

                        // skip * separators
                        beginCurStrIndex = i;
                        while (IsTopLevelSeparator(t[++i])) ;

                        splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]), beginCurStrIndex, i - 1));

                        prevStr = "";
                        beginCurStrIndex = i;
                        i--;
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

                if (isNeutralizer)
                {
                    // neutralizer symbol: should be removed from parsed split
                    t[i] = metachar;
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
            // remove splits that fit the separator pattern (length>=1) - keep any defined empty value: '' "" (length=0)

            splits = splits.Where(x => !(string.IsNullOrWhiteSpace(x.Text) && x.Length >= 1)).ToList();     // TODO: ⚠️⚠️⚠️ must perform a real separator pattern matching check ⚠️⚠️⚠️

            // clean up meta chars in splits

            foreach (var split in splits)
            {
                split.SetText(split.Text.Replace($"{metachar}", ""));
            }

            return splits.ToArray();
        }

        /// <summary>
        /// indicates if the char is a separator at top input stream level
        /// <para>// TODO: ⚠️⚠️⚠️ handle here more separators: eol,cr,... ⚠️⚠️⚠️</para>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsTopLevelSeparator(char c)
        {
            return c == SpaceSeparator;
        }

        private static char FindMetaChar(ref string expr)
        {
            char metachar = (char)0;
            var badmetachar = expr.Contains(metachar);
            if (badmetachar)
            {
                var r = new Random();
                while (badmetachar)
                {
                    metachar = (char)r.Next(10000, 20000);
                    badmetachar = expr.Contains(metachar);
                }
            }

            return metachar;
        }

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

        /// <summary>
        /// substitue any var ($..) found in expr (expr is a word from the command line)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static
            (string expr, Dictionary<string, object> references)
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

        public static PipelineParseResults ParseCommandLine(
            CommandEvaluationContext context,
            ISyntaxAnalyser syntaxAnalyzer,
            string expr,
            IExternalParserExtension commandLineParserExtension
            )
        {
            var parseResults = new PipelineParseResults();

            if (expr == null) return new PipelineParseResults(new PipelineParseResult(parseResults));

            expr = expr.Trim();
            if (string.IsNullOrWhiteSpace(expr)) return new PipelineParseResults(new PipelineParseResult(parseResults));

            var splits0 = SplitExpr(context, expr);

            // TODO: add splits to debug var in debug mode

            try
            {
                var pipeline = GetPipeline(context, splits0);
                var workUnit = pipeline;
                var splits = new List<StringSegment>();
                var references = new Dictionary<string, object>();

                // check and substitute alias

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

                // TODO: add segments to debug var in debug mode
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
            ISyntaxAnalyser syntaxAnalyzer,
            IExternalParserExtension commandLineParserExtension,
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

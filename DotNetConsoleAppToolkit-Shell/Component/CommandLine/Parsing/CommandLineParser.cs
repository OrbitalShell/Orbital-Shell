﻿//#define debugParser
#define splitFromPipeline

using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Component.CommandLine.Variable;
using DotNetConsoleAppToolkit.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DotNetConsoleAppToolkit.Component.CommandLine.Parsing.CommandLineSyntax;
using static DotNetConsoleAppToolkit.Component.CommandLine.Pipeline.PipelineParser;
using static DotNetConsoleAppToolkit.Component.CommandLine.Variable.Variables;
using static DotNetConsoleAppToolkit.DotNetConsole;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Parsing
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
                        prevStr = ((i-1) >= beginCurStrIndex) ? new string(t[beginCurStrIndex..i]) : "";
                        inSingleQuoteString = isSingleQuoteStringSeparator;
                        inDoubleQuoteString = isDoubleQuoteStringSeparator;
                        inString = true;
                        beginCurStrIndex = i + 1;
                    }
                   
                    if (isSeparator && (i - 1)>=beginCurStrIndex)
                    {
                        if ((i-1) >= beginCurStrIndex) splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]),beginCurStrIndex,i-1));
                        prevStr = "";
                        beginCurStrIndex = i + 1;
                    }
                } 
                else
                {
                    if ((inSingleQuoteString && isSingleQuoteStringSeparator)
                        ||(inDoubleQuoteString && isDoubleQuoteStringSeparator))
                    {
                        if (i==t.Length-1)
                        {
                            splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..i]),beginCurStrIndex,i-1));
                            prevStr = "";
                            beginCurStrIndex = t.Length;
                        } 
                        else
                        {
                            var t2 = t[0..(beginCurStrIndex-1)].ToList();
                            t2.AddRange(t[beginCurStrIndex..i].ToList());
                            if (i < t.Length - 1)
                                t2.AddRange( t[(i + 1)..t.Length].ToList() );
                            t = t2.ToArray();
                            k = t.Length;
                            beginCurStrIndex--;
                            i-=2;
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
                splits.Add(new StringSegment(prevStr + new string(t[beginCurStrIndex..(t.Length)]),beginCurStrIndex,i-1));
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

        public static int GetIndex(
            CommandEvaluationContext context,
            int position,
            string expr)
        {
            var splits = SplitExpr(context,expr);
            var n = 0;
            for (int i = 0; i <= position && i<splits.Length; i++)
                n += splits[i].Length + ((i>0)?1:0);
            return n;
        }

        public static string SubstituteVariables(
            CommandEvaluationContext context,
            string expr
            )
        {
            var t = expr.ToCharArray();
            var i = 0;
            var vars = new List<StringSegment>();
            
            while (i<t.Length)
            {
                var c = t[i];
                if (c== CommandLineSyntax.VariablePrefix && (i==0 || t[i-1]!='\\' ))
                {
                    var j = VariableSyntax.FindEndOfVariableName(t, i+1);
                    var variable = expr.Substring(i+1, j - i);
                    vars.Add(new StringSegment(variable, i, j, j - i + 1));
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
                    nexpr.Append(expr.Substring(x, vr.X-x));
                    try
                    {
                        context.Variables.GetValue(vr.Text,out var value);
                        nexpr.Append(value.Value);
                    }
                    catch (VariableNotFoundException ex)
                    {
                        Errorln(ex.Message);
                        // keep bad var name in place
                        nexpr.Append("$" + vr.Text);
                    }
                    x = vr.Y + 1;
                }
                if (lastvr!=null)
                {
                    nexpr.Append(expr.Substring(x));
                }
                expr = nexpr.ToString();
            }

            return expr;
        }

        public static List<PipelineParseResult> Parse(
            CommandEvaluationContext context,
            SyntaxAnalyser syntaxAnalyzer,
            string expr)
        {
            if (expr == null) return new List<PipelineParseResult> { new PipelineParseResult() };

            expr = expr.Trim();
            if (string.IsNullOrWhiteSpace(expr)) return new List<PipelineParseResult> { new PipelineParseResult() };

            var splits0 = SplitExpr(context, expr);
            var parseResults = new List<PipelineParseResult>();

            try
            {
                var pipeline = GetPipeline(context, splits0);
                var workUnit = pipeline;
                var splits = new List<StringSegment>();

#if !splitFromPipeline
                foreach (var split in splits0)
                {
                    var argExpr2 = SubstituteVariables(context, split.Text);
                    splits.Add(new StringSegment(argExpr2, split.X, split.Y));
                }
                parseResults.Add(
                    ParseCmdSplits(
                        context,
                        syntaxAnalyzer,
                        expr,
                        splits));
#else
                while (workUnit != null)
                {
                    splits.Clear();
                    foreach (var split in workUnit.Segments)
                    {
                        var argExpr2 = SubstituteVariables(context, split.Text);
                        splits.Add(new StringSegment(argExpr2, split.X, split.Y));
                    }
                                        
                    parseResults.Add(
                        new PipelineParseResult(
                            workUnit,
                            ParseCmdSplits(
                                context,
                                syntaxAnalyzer,
                                expr,
                                splits)));

                    workUnit = workUnit.NextUnit;
                }
#endif
                
            }
            catch (ParseErrorException parseErrorEx)
            {
                // get pipeline parse error
                parseResults.Add(
                    new PipelineParseResult(
                        new ParseResult(
                            ParseResultType.SyntaxError,
                            new List<CommandSyntaxParsingResult>
                            {
                                new CommandSyntaxParsingResult(null,null,new List<ParseError>{ parseErrorEx.ParseError })
                            })));
            }
            catch (Exception)
            {
                throw;
            }
            PipelineParseResult ppr = null;
            foreach ( var pr in parseResults )
            {
                if (ppr != null)
                    ppr.Next = pr;
                ppr = pr;
            }
            return parseResults;
        }

        public static ParseResult ParseCmdSplits(
            CommandEvaluationContext context,
            SyntaxAnalyser syntaxAnalyzer,
            string expr,
            List<StringSegment> splits)
        { 
            // <----------------------

            var segments = splits.Skip(1).ToArray();
            var token = splits.First().Text;

            // get potential syntaxes
            var ctokens = syntaxAnalyzer.FindSyntaxesFromToken(token, false, SyntaxMatchingRule);

            if (ctokens.Count == 0)
            {
                var cmdnotfound = new CommandSyntaxParsingResult(null, null, new List<ParseError> { new ParseError($"unknown command: {token}", 0, 0, null) });
                return new ParseResult(ParseResultType.NotIdentified, new List<CommandSyntaxParsingResult> { cmdnotfound });
            }

            if (ctokens.Count > 0)
            {
                int nbValid = 0;
                var syntaxParsingResults = new List<CommandSyntaxParsingResult>();
                var validSyntaxParsingResults = new List<CommandSyntaxParsingResult>();

                foreach (var syntax in ctokens)
                {
                    var (matchingParameters, parseErrors) = syntax.Match(SyntaxMatchingRule, segments.Select(x => x.Text).ToArray(), token.Length + 1);
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

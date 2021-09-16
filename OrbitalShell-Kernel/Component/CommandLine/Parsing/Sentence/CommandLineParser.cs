//#define debugParser

using System;
using System.Collections.Generic;
using System.Linq;

using OrbitalShell.Component.CommandLine.Parsing.Command;
using OrbitalShell.Component.CommandLine.Parsing.Parser;
using OrbitalShell.Component.CommandLine.Parsing.Parser.Error;
using OrbitalShell.Component.CommandLine.Parsing.Pipeline;
using OrbitalShell.Component.CommandLine.Parsing.Variable;
using OrbitalShell.Component.CommandLine.Pipeline;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Lib;

using static OrbitalShell.Component.CommandLine.Parsing.Pipeline.PipelineParser;

namespace OrbitalShell.Component.CommandLine.Parsing.Sentence
{
    public static class CommandLineParser
    {
        public static StringComparison SyntaxMatchingRule { get; set; } = StringComparison.InvariantCultureIgnoreCase;

        static VariableReplacer _variableReplacer = new();

        static SentenceSpliter _sentenceSpliter = new();

        public static int GetIndex(
            CommandEvaluationContext context,
            int position,
            string expr)
        {
            var splits = _sentenceSpliter.SplitExpr(context, expr);
            var n = 0;
            for (int i = 0; i <= position && i < splits.Length; i++)
                n += splits[i].Length + ((i > 0) ? 1 : 0);
            return n;
        }

        public static PipelineParseResults ParseCommandLine(
            CommandEvaluationContext context,
            ICommandSyntaxAnalyzer syntaxAnalyzer,
            string expr,
            IExternalParserExtension commandLineParserExtension
            )
        {
            var parseResults = new PipelineParseResults();

            if (expr == null) return new PipelineParseResults(new PipelineParseResult(parseResults));

            expr = expr.Trim();
            if (string.IsNullOrWhiteSpace(expr)) return new PipelineParseResults(new PipelineParseResult(parseResults));

            var splits0 = _sentenceSpliter.SplitExpr(context, expr);

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
                    splits0 = _sentenceSpliter.SplitExpr(context, expr);
                    pipeline = GetPipeline(context, splits0);
                    workUnit = pipeline;
                }

                while (workUnit != null)
                {
                    splits.Clear();
                    foreach (var split in workUnit.Segments)
                    {
                        (string argExpr2, Dictionary<string, object> refs) = _variableReplacer.SubstituteVariables(context, split.Text);
                        foreach (var kv in refs)
                            references.AddOrReplace(kv.Key, kv.Value);
                        splits
                            .Add(
                            new StringSegment(argExpr2, split.X, split.Y, refs));
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
            ICommandSyntaxAnalyzer syntaxAnalyzer,
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

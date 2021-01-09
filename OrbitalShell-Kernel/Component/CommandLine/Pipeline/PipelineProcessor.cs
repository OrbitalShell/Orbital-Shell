using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using static DotNetConsoleAppToolkit.DotNetConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Pipeline
{
    public class PipelineProcessor
    {
        public static ExpressionEvaluationResult RunPipeline(
            CommandEvaluationContext context,
            PipelineParseResult pipelineParseResult
            )
        {
            ExpressionEvaluationResult r = null;
            var workUnitParseResult = pipelineParseResult;
            while (workUnitParseResult != null)
            {
                r = RunWorkUnit(context, workUnitParseResult);
                workUnitParseResult = workUnitParseResult.Next;
            }
            return r;
        }
        
        public static ExpressionEvaluationResult RunWorkUnit(
            CommandEvaluationContext context,
            PipelineParseResult pipelineParseResult
            )
        {
            var syntaxParsingResult = pipelineParseResult.ParseResult.SyntaxParsingResults.First();
            try
            {
                var outputData = InvokeCommand(context, syntaxParsingResult.CommandSyntax.CommandSpecification, syntaxParsingResult.MatchingParameters);

                return new ExpressionEvaluationResult(null, ParseResultType.Valid, outputData, (int)ReturnCode.OK, null);
            }
            catch (Exception commandInvokeError)
            {
                var commandError = commandInvokeError.InnerException ?? commandInvokeError;
                Errorln(commandError.Message);
                return new ExpressionEvaluationResult(null, pipelineParseResult.ParseResult.ParseResultType, null, (int)ReturnCode.Error, commandError);
            }
        }

        static object InvokeCommand(
            CommandEvaluationContext context,
            CommandSpecification commandSpecification,
            MatchingParameters matchingParameters)
        {
            var parameters = new List<object>() { context };
            var pindex = 0;
            foreach (var parameter in commandSpecification.MethodInfo.GetParameters())
            {
                if (pindex > 0)
                {
                    if (matchingParameters.TryGet(parameter.Name, out var matchingParameter))
                        parameters.Add(matchingParameter.GetValue());
                    else
                        throw new InvalidOperationException($"parameter not found: '{parameter.Name}' when invoking command: {commandSpecification}");
                }
                pindex++;
            }
            var r = commandSpecification.MethodInfo
                .Invoke(commandSpecification.MethodOwner, parameters.ToArray());
            return r;
        }
    }
}

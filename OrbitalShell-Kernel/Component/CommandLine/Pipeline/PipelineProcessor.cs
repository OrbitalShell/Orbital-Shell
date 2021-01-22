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
                // capture the err output
                context.Err.StartRecording();
                
                var r = InvokeCommand(context, syntaxParsingResult.CommandSyntax.CommandSpecification, syntaxParsingResult.MatchingParameters);
                var res = r as ICommandResult;
                var err_record = context.Err.StopRecording();
                var err = res.ExecErrorText;

                // auto assign from Err stream if no error text provided
                if (string.IsNullOrEmpty(res.ExecErrorText) && !string.IsNullOrEmpty(err_record)) err = err_record;

                return 
                    (res==null)?
                        new ExpressionEvaluationResult(pipelineParseResult.Expr, null, ParseResultType.Valid, null, (int)ReturnCode.Error, null , "the command has returned a null result" ) :
                        new ExpressionEvaluationResult(pipelineParseResult.Expr, null, ParseResultType.Valid, res.GetOuputData(), res.ReturnCode , null , err ) ;
            }
            catch (Exception commandInvokeError)
            {
                // error is catched at shell level
                var commandError = commandInvokeError.InnerException ?? commandInvokeError;
                context.Errorln(commandError.Message);
                return new ExpressionEvaluationResult(pipelineParseResult.Expr, null, pipelineParseResult.ParseResult.ParseResultType, null, (int)ReturnCode.Error, commandError);
            }
            finally {
                context.Err.StopRecording();
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

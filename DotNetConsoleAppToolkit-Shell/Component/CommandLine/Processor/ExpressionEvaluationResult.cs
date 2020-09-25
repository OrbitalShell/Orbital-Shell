using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
using System;

namespace DotNetConsoleAppToolkit.Component.CommandLine.Processor
{
    public class ExpressionEvaluationResult
    {
        public readonly string SyntaxError;
        public readonly object Result;      // output data
        public readonly int EvalResultCode;
        public readonly ParseResultType ParseResult;
        public readonly Exception EvalError;

        public ExpressionEvaluationResult(
            string syntaxError,
            ParseResultType parseResult, 
            object result, 
            int evalResultCode,
            Exception evalError)
        {
            SyntaxError = syntaxError;
            ParseResult = parseResult;
            Result = result;
            EvalResultCode = evalResultCode;
            EvalError = evalError;
        }
    }
}

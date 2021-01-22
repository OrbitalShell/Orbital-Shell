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
        public readonly string EvalErrorText;

        /// <summary>
        /// cmd line expr. eval result
        /// <para></para>
        /// </summary>
        /// <param name="syntaxError">text of error if concerns syntax analysis</param>
        /// <param name="parseResult">parse result state</param>
        /// <param name="result">command evaluation result</param>
        /// <param name="evalResultCode">analysis return code or execution return code</param>
        /// <param name="evalError">eval exception error</param>
        /// <param name="evalErrorText">eval error text (if no exception)</param>
        public ExpressionEvaluationResult(
            string syntaxError,
            ParseResultType parseResult, 
            object result, 
            int evalResultCode,
            Exception evalError = null,
            string evalErrorText = null)
        {
            SyntaxError = syntaxError;
            ParseResult = parseResult;
            Result = result;
            EvalResultCode = evalResultCode;
            EvalError = evalError;
            EvalErrorText = evalErrorText;
        }
    }
}

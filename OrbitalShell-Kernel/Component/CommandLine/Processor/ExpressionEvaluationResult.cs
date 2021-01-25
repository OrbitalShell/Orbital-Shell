using OrbitalShell.Component.CommandLine.Parsing;
using System;

namespace OrbitalShell.Component.CommandLine.Processor
{
    public class ExpressionEvaluationResult
    {
        /// <summary>
        /// the analysed command text
        /// </summary>
        public readonly string Com;

        /// <summary>
        /// analysis error syntax error (if any)
        /// </summary>
        public readonly string SyntaxError;

        /// <summary>
        /// com output data or null
        /// </summary>
        public readonly object Result;

        /// <summary>
        /// analysis result code or com result code
        /// </summary>
        public readonly int EvalResultCode;

        /// <summary>
        /// analysis result
        /// </summary>
        public readonly ParseResultType ParseResult;

        /// <summary>
        /// com exception (if any)
        /// </summary>
        public readonly Exception EvalError;

        /// <summary>
        /// com eval error text (analysis or err stream, if any)
        /// </summary>
        public readonly string EvalErrorText;

        /// <summary>
        /// cmd line expr. eval result
        /// </summary>
        /// <param name="com">the analyzed command text</param>
        /// <param name="syntaxError">text of error if concerns syntax analysis</param>
        /// <param name="parseResult">parse result state</param>
        /// <param name="result">command evaluation result</param>
        /// <param name="evalResultCode">analysis return code or execution return code</param>
        /// <param name="evalError">eval exception error</param>
        /// <param name="evalErrorText">eval error text (if no exception)</param>
        public ExpressionEvaluationResult(
            string com,
            string syntaxError,
            ParseResultType parseResult, 
            object result, 
            int evalResultCode,
            Exception evalError = null,
            string evalErrorText = null)
        {
            Com = com;
            SyntaxError = syntaxError;
            ParseResult = parseResult;
            Result = result;
            EvalResultCode = evalResultCode;
            EvalError = evalError;
            EvalErrorText = evalErrorText;
        }
    }
}

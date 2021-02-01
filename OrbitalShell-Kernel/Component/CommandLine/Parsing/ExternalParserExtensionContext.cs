using System;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public class ExternalParserExtensionContext
    {
        public readonly CommandEvaluationContext Context;
        public readonly string TokenName;
        public readonly string[] Args;
        public readonly string Expr;

        public ExternalParserExtensionContext(
            CommandEvaluationContext context,
            string tokenName,
            string[] args,
            string expr
        )
        {
            Context = context;
            TokenName = tokenName;
            Args = args;
            Expr = expr;
        }

    }
}
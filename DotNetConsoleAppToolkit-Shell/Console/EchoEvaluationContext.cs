using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using System;

namespace DotNetConsoleAppToolkit.Console
{
    public class EchoEvaluationContext
    {
        public ConsoleTextWriterWrapper Out;
        public CommandEvaluationContext CmdContext;
        public FormattingOptions Options;

        public EchoEvaluationContext(
            ConsoleTextWriterWrapper @out, 
            CommandEvaluationContext cmdContext, 
            FormattingOptions options=null)
        {
            Out = @out;
            CmdContext = cmdContext;
            Options = options;
        }

        internal void Deconstruct(out ConsoleTextWriterWrapper @out, out CommandEvaluationContext context, out FormattingOptions options)
        {
            @out = Out;
            context = CmdContext;
            options = Options;
        }
    }
}

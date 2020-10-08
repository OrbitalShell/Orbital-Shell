using DotNetConsoleAppToolkit.Component.CommandLine.Processor;

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

        public void Deconstruct(
            out ConsoleTextWriterWrapper @out, 
            out CommandEvaluationContext context, 
            out FormattingOptions options)
        {
            @out = Out;
            context = CmdContext;
            options = Options;
        }

        public void Echo(object o)
        {
            if (o==null)
            {
                Out.Echo(EchoPrimitives.DumpAsText(CmdContext,null));
            } else
            {
                o.Echo(this);
            }
        }
    }
}

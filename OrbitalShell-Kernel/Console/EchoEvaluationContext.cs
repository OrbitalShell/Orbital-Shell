using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Console
{
    public class EchoEvaluationContext
    {
        public ConsoleTextWriterWrapper Out;
        public CommandEvaluationContext CommandEvaluationContext;
        public FormattingOptions Options;

        public EchoEvaluationContext(
            ConsoleTextWriterWrapper @out, 
            CommandEvaluationContext cmdContext, 
            FormattingOptions options=null)
        {
            Out = @out;
            CommandEvaluationContext = cmdContext;
            Options = options;
        }

        public void Deconstruct(
            out ConsoleTextWriterWrapper @out, 
            out CommandEvaluationContext context, 
            out FormattingOptions options)
        {
            @out = Out;
            context = CommandEvaluationContext;
            options = Options;
        }

        /*public void Echo(object o)
        {
            if (o==null)
            {
                Out.Echo(EchoPrimitives.DumpAsText(CommandEvaluationContext,null));
            } else
            {
                o.Echo(this);
            }
        }*/
    }
}

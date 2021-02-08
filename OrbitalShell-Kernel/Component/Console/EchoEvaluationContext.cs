using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Console
{
    /// <summary>
    /// contextual data of an echo operation context
    /// </summary>
    public class EchoEvaluationContext
    {
        /// <summary>
        /// target stream
        /// </summary>
        public ConsoleTextWriterWrapper Out;

        /// <summary>
        /// command evaluation context
        /// </summary>
        public CommandEvaluationContext CommandEvaluationContext;

        /// <summary>
        /// formatting options
        /// </summary>
        public FormattingOptions Options;

        public EchoEvaluationContext(
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext cmdContext,
            FormattingOptions options = null)
        {
            Out = @out;
            CommandEvaluationContext = cmdContext;
            Options = options;
        }

        public EchoEvaluationContext(
            EchoEvaluationContext ctx
            )
        {
            Out = ctx.Out;
            CommandEvaluationContext = ctx.CommandEvaluationContext;
            Options = ctx.Options;
        }

        public EchoEvaluationContext(
            EchoEvaluationContext ctx,
            FormattingOptions options
            )
        {
            Out = ctx.Out;
            CommandEvaluationContext = ctx.CommandEvaluationContext;
            Options = options;
        }

        /// <summary>
        ///  for Lib.TypeExt.Clone() method purpose
        /// </summary>
        public EchoEvaluationContext() { }

        public void Deconstruct(
            out ConsoleTextWriterWrapper @out,
            out CommandEvaluationContext context,
            out FormattingOptions options)
        {
            @out = Out;
            context = CommandEvaluationContext;
            options = Options;
        }
    }
}

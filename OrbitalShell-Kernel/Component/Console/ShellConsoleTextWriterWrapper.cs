using System.IO;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console.Formats;
using OrbitalShell.Component.Script;

namespace OrbitalShell.Component.Console
{
    /// <summary>
    /// wraps the console text writer wrapper for the needs of the shell
    /// </summary>
    public class ShellConsoleTextWriterWrapper : ConsoleTextWriterWrapper
    {
        public CommandEvaluationContext CommandEvaluationContext;

        public override string ToString() => $"[shell console text writer wrapper - {base.ToString()}]";

        public ShellConsoleTextWriterWrapper(
            CommandEvaluationContext commandEvaluationContext,
            TextWriter textWriter,
            CSharpScriptEngine cSharpScriptEngine = null
            ) : base(
                commandEvaluationContext.CommandLineProcessor.Console,
                textWriter,
                cSharpScriptEngine
                )
        {
            CommandEvaluationContext = commandEvaluationContext;
            ColorSettings = commandEvaluationContext.ShellEnv.Colors;
        }

        public override void Echo(
            object o,
            bool lineBreak = false,
            bool preserveColors = false,        // TODO: remove this parameter + SaveColors property
            bool parseCommands = true,
            bool doNotEvalutatePrintDirectives = false,         // TODO: explain this
            EchoSequenceList printSequences = null,
            bool avoidANSISequencesAndNonPrintableCharacters = true,
            bool getNonPrintablesASCIICodesAsLabel = true
            // TODO: add the formatting options for the decorators (see ConsoleCommands.Echo)            
            )
        {
            if (IsMute) return;
            if (o == null)
            {
                // handle null

                base.Echo(
                    EchoPrimitives.DumpNull(CommandEvaluationContext),
                    lineBreak,
                        preserveColors,
                        parseCommands,
                        doNotEvalutatePrintDirectives,
                        printSequences,
                        avoidANSISequencesAndNonPrintableCharacters,
                        getNonPrintablesASCIICodesAsLabel
                    );
            }
            else
            {
                if (o is string)
                {
                    // handle string

                    base.Echo(
                        o,
                        lineBreak,
                        preserveColors,
                        parseCommands,
                        doNotEvalutatePrintDirectives,
                        printSequences,
                        avoidANSISequencesAndNonPrintableCharacters,
                        getNonPrintablesASCIICodesAsLabel
                    );
                }
                else
                {
                    // handle object

                    o.Echo(
                        new EchoEvaluationContext(
                            (ConsoleTextWriterWrapper)CommandEvaluationContext.Out,
                            CommandEvaluationContext,
                            new FormatingOptions(lineBreak, parseCommands, true)
                        )
                    );
                    if (lineBreak) base.Echo("", true);  // TODO: formatting option not implemented ?
                }
            }
        }

    }
}
using System.IO;
using System;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Console
{
    /// <summary>
    /// wraps the console text writer wrapper for the needs of the shell
    /// </summary>
    public class ShellConsoleTextWriterWrapper : ConsoleTextWriterWrapper
    {
        public CommandEvaluationContext CommandEvaluationContext;

        public ShellConsoleTextWriterWrapper(
            CommandEvaluationContext commandEvaluationContext,
            TextWriter textWriter
            ) : base(textWriter)
        {
            CommandEvaluationContext = commandEvaluationContext;
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
            if (o == null)
            {
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
                    o.Echo(
                        new EchoEvaluationContext(
                            (ConsoleTextWriterWrapper)CommandEvaluationContext.Out,
                            CommandEvaluationContext,
                            new FormattingOptions(lineBreak, parseCommands)
                        )
                    );
                    if (lineBreak) base.Echo("", true);  // TODO: formatting option not implemented ?
                }
            }
        }



    }
}
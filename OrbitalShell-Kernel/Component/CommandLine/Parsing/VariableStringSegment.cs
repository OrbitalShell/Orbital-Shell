using OrbitalShell.Component.Console;
using OrbitalShell.Lib;

using static OrbitalShell.Component.CommandLine.Parsing.CommandLineSyntax;

namespace OrbitalShell.Component.CommandLine.Parsing
{
    public class VariableStringSegment : StringSegment
    {
        public bool IsNameCaptured { get; protected set; }

        public VariableStringSegment(
            string text,
            int x,
            int y,
            int length,
            bool isNameCaptured)
            : base(text, x, y, length)
        {
            IsNameCaptured = isNameCaptured;
        }

        public string FullSyntax
            => CommandLineSyntax.VariablePrefix +
                Text.WrapIf(
                    IsNameCaptured
                    , VariableNameOpenCapture + ""
                    , VariableNameEndCapture + "");
    }
}

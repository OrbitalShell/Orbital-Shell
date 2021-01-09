using System.Collections.Generic;

namespace DotNetConsoleAppToolkit.Console
{
    public class LineSplits
    {
        public readonly List<StringSegment> Splits;

        public readonly EchoSequences PrintSequences;

        public readonly int CursorIndex;

        public readonly int CursorLineIndex;

        public LineSplits(
            List<StringSegment> splits,
            EchoSequences printSequences,
            int cursorIndex=-1,
            int cursorLineIndex=-1)
        {
            Splits = splits;
            PrintSequences = printSequences;
            CursorIndex = cursorIndex;
            CursorLineIndex = cursorLineIndex;
        }

    }
}

using OrbitalShell.Console;
using OrbitalShell.Lib.FileSystem;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace OrbitalShell.Shell.Commands.TextEditor
{
    public class EditorBackup
    {
        public readonly bool RawMode;
        public readonly FilePath FilePath;
        public readonly string EOLSeparator;
        public readonly bool ReadOnly;
        public readonly bool FileModified;
        public readonly long FileSize = 0;
        public readonly int FirstLine = 0;
        public readonly int CurrentLine = 0;
        public readonly int X = 0;
        public readonly int Y = 0;
        public readonly List<string> Text;
        public readonly List<List<StringSegment>> LinesSplits;
        public readonly Encoding FileEncoding;
        public readonly OSPlatform? FileEOL;
        public readonly Point BeginOfLineCurPos;
        public readonly int LastVisibleLineIndex;
        public readonly int SplitedLastVisibleLineIndex;

        public EditorBackup(
            bool rawMode,
            FilePath filePath,
            string eolSeparator,
            bool readOnly,
            bool fileModified,
            long fileSize,
            int firstLine,
            int currentLine,
            int x,
            int y,
            List<string> text,
            List<List<StringSegment>> linesSplits,
            Encoding fileEncoding,
            OSPlatform? fileEOL,
            Point beginOfLineCurPos,
            int lastVisibleLineIndex,
            int splitedLastVisibleLineIndex)
        {
            RawMode = false;
            FilePath = filePath;
            EOLSeparator = eolSeparator;
            ReadOnly = readOnly;
            FileModified = fileModified;
            FileSize = fileSize;
            FirstLine = firstLine;
            CurrentLine = currentLine;
            X = x;
            Y = y;
            Text = new List<string>();
            Text.AddRange(text);
            LinesSplits = new List<List<StringSegment>>();
            LinesSplits.AddRange(linesSplits);
            FileEncoding = fileEncoding;
            FileEOL = fileEOL;
            BeginOfLineCurPos = beginOfLineCurPos;
            LastVisibleLineIndex = lastVisibleLineIndex;
            SplitedLastVisibleLineIndex = splitedLastVisibleLineIndex;
        }
    }
}

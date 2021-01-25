using DotNetConsoleAppToolkit.Lib.FileSystem;
using System.Runtime.InteropServices;

namespace DotNetConsoleAppToolkit.Shell.Commands.TextFile
{
    public class TextFileInfo
    {
        public readonly FilePath File;
        public readonly string[] TextLines;
        public readonly OSPlatform OSPlatform;
        public readonly string Eol;

        public TextFileInfo(FilePath file,string[] textLines, OSPlatform oSPlatform, string eol)
        {
            File = file;
            TextLines = textLines;
            OSPlatform = oSPlatform;
            Eol = eol;
        }
    }
}

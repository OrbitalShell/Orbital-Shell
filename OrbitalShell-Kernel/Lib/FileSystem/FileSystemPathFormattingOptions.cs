using OrbitalShell.Component.Console;
namespace OrbitalShell.Lib.FileSystem
{
    public class FileSystemPathFormattingOptions : FormatingOptions
    {
        public bool PrintAttributes = true;
        public bool ShortPath = false;
        public string Prefix = "";
        public string Postfix = "";
        public int PaddingRight = -1;
        public string LinePrefix = "";

        public FileSystemPathFormattingOptions() { }

        public FileSystemPathFormattingOptions(FormatingOptions o) => InitFrom(o);

        public FileSystemPathFormattingOptions(
            bool printAttributes = true,
            bool shortPath = false,
            string prefix = "",
            string postfix = "",
            int paddingRight = -1,
            string linePrefix = "",
            
            bool lineBreak = false,
            bool isRawModeEnabled = false
            )
            : base(lineBreak,isRawModeEnabled)
        {
            PrintAttributes = printAttributes;
            ShortPath = shortPath;
            Prefix = prefix;
            Postfix = postfix;
            PaddingRight = paddingRight;
            LinePrefix = linePrefix;
        }
    }
}

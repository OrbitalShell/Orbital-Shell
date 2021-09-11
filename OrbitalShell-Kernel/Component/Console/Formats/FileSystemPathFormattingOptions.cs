using OrbitalShell.Lib.Sys;

namespace OrbitalShell.Component.Console.Formats
{
    public class FileSystemPathFormattingOptions :
        FormatingOptions,
        IClonable<FileSystemPathFormattingOptions>
    {
        public bool PrintAttributes = true;
        public bool ShortPath = false;
        public string Prefix = "";
        public string Postfix = "";
        public int PaddingRight = -1;
        public string LinePrefix = "";

        public FileSystemPathFormattingOptions() { }

        public FileSystemPathFormattingOptions(FormatingOptions o)
            => InitFrom(o);

        public FileSystemPathFormattingOptions(
            bool printAttributes = true,
            bool shortPath = false,
            string prefix = "",
            string postfix = "",
            int paddingRight = -1,
            string linePrefix = "",

            bool lineBreak = false,
            bool isRawModeEnabled = false,
            bool isObjectDumpEnabled = true
            )
            : base(lineBreak, isRawModeEnabled, isObjectDumpEnabled)
        {
            PrintAttributes = printAttributes;
            ShortPath = shortPath;
            Prefix = prefix;
            Postfix = postfix;
            PaddingRight = paddingRight;
            LinePrefix = linePrefix;
        }

        public FileSystemPathFormattingOptions InitFrom(FileSystemPathFormattingOptions o)
        {
            PrintAttributes = o.PrintAttributes;
            ShortPath = o.ShortPath;
            Prefix = o.Prefix;
            Postfix = o.Postfix;
            PaddingRight = o.PaddingRight;
            LinePrefix = o.LinePrefix;
            return this;
        }

        public override FileSystemPathFormattingOptions Clone()
            => new(this);
    }
}

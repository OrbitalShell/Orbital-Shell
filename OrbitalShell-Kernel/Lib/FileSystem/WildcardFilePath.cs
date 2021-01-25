using System.IO;
using static OrbitalShell.DotNetConsole;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Lib.FileSystem
{
    public class WildcardFilePath : DirectoryPath
    {
        public readonly string WildCardFileName;
        public readonly string OriginalPath;

        public WildcardFilePath(string path) : base(path) {
            OriginalPath = path;
            if (ContainsWildcardFileName(path))
            {
                var basepath = Path.GetDirectoryName(path);
                if (string.IsNullOrWhiteSpace(basepath)) basepath = System.Environment.CurrentDirectory;
                WildCardFileName = Path.GetFileName(path);
                FileSystemInfo = new DirectoryInfo(basepath);
            }
            else 
            { 
                if (File.Exists(path))
                {
                    var basepath = Path.GetDirectoryName(path);
                    if (string.IsNullOrWhiteSpace(basepath)) basepath = System.Environment.CurrentDirectory;
                    WildCardFileName = Path.GetFileName(path);
                    FileSystemInfo = new DirectoryInfo(basepath);
                } else
                {
                    if (Directory.Exists(path))
                    {
                        FileSystemInfo = new DirectoryInfo(path);
                    }
                }
            }
            DirectoryInfo = (DirectoryInfo)FileSystemInfo;
        }

        public override bool CheckExists(CommandEvaluationContext context,bool dumpError = true)
        {
            if (!DirectoryInfo.Exists)
            {
                if (dumpError) context.Errorln($"file or directory doesn't exists: {this}");
                return false;
            }
            return true;
        }

        public static bool ContainsWildcardFileName(string path) {
            var ext = Path.GetFileName(path);
            return ext.Contains('*') || ext.Contains('?');
        }

        public string NameWithWildcard => WildCardFileName ?? Name;
        public string FullNameWithWildcard => Path.Combine(FileSystemInfo.FullName,WildCardFileName);
        public string PrintableFullNameWithWildCard
        {
            get
            {
                var quote = FullNameWithWildcard.Contains(' ') ? "\"" : "";
                return $"{quote}{FullNameWithWildcard}{quote}";
            }
        }
        public string PrintableNameWithWildCard
        {
            get
            {
                var quote = NameWithWildcard.Contains(' ') ? "\"" : "";
                return $"{quote}{NameWithWildcard}{quote}";
            }
        }
        public string GetPrintableNameWithWlidCard(bool fullname = false) => fullname ? PrintableFullNameWithWildCard : PrintableNameWithWildCard;
    }
}


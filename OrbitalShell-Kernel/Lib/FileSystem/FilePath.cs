using System.IO;
using System.Text;
using static OrbitalShell.DotNetConsole;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Lib.FileSystem
{
    public class FilePath : FileSystemPath
    {
        public readonly FileInfo FileInfo;

        public FilePath(string path) : base(new FileInfo(path))
        {
            FileInfo = (FileInfo)FileSystemInfo;
        }

        public override bool CheckExists(CommandEvaluationContext context,bool dumpError = true)
        {
            if (!FileInfo.Exists)
            {
                if (dumpError) context.Errorln($"file doesn't exists: {this}");
                return false;
            }
            return true;
        }

        public bool CheckPathExists(bool dumpError = true)
        {
            if (!Directory.Exists(Path.GetDirectoryName(FullName)))
            {
                if (dumpError)
                    Errorln($"the directory of the file doesn't exists: {this}");
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return FileInfo.FullName;
        }

        public Encoding GetEncoding(Encoding defaultEncoding=null)
        {
            if (defaultEncoding != null)
            {
                using var reader = new StreamReader(FullName, defaultEncoding, true);
                if (reader.Peek() >= 0)
                    reader.Read();
                return reader.CurrentEncoding;
            } 
            else
            {
                using var reader = new StreamReader(FullName, true);
                if (reader.Peek() >= 0)
                    reader.Read();
                return reader.CurrentEncoding;
            }
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Text;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Lib.FileSystem
{
    public class FilePath : FileSystemPath
    {
        public readonly FileInfo FileInfo;

        public override long Length => FileInfo.Length;

        public FilePath(string path) : base(new FileInfo(path))
        {
            FileInfo = (FileInfo)FileSystemInfo;
        }

        public override bool CheckExists(CommandEvaluationContext context, bool dumpError = true)
        {
            if (!FileInfo.Exists)
            {
                if (dumpError) context.Errorln($"file doesn't exists: {this}");
                return false;
            }
            return true;
        }

        public bool CheckPathExists(CommandEvaluationContext context, bool dumpError = true)
        {
            if (!Directory.Exists(Path.GetDirectoryName(FullName)))
            {
                if (dumpError)
                    context.Errorln($"the directory of the file doesn't exists: {this}");
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return UnescapePathSeparators(FileInfo.FullName);
        }

        public Encoding GetEncoding(Encoding defaultEncoding = null) => GetEncoding(FullName, defaultEncoding);

        public static Encoding GetEncoding(string filePath,Encoding defaultEncoding = null)
        {
            if (defaultEncoding != null)
            {
                using var reader = new StreamReader(filePath, defaultEncoding, true);
                if (reader.Peek() >= 0)
                    reader.Read();
                return reader.CurrentEncoding;
            }
            else
            {
                using var reader = new StreamReader(filePath, true);
                if (reader.Peek() >= 0)
                {
                    for (int i=0;i<10;i++) if (reader.Peek() >= 0) reader.Read();
                }
                return reader.CurrentEncoding;
            }
        }

        public const double NonPrintableCharactersMaxRatio = 10;
        public const int CheckSequenceMinimumLength = 1024;
        public const int CheckSequenceMaximumLength = 2048;

        public static bool IsTextFile(
            string filePath, 
            double maxRatio = NonPrintableCharactersMaxRatio, 
            int minSeqLength = CheckSequenceMinimumLength, 
            int maxSeqLength = CheckSequenceMaximumLength)
            => !IsBinaryFile(filePath, maxRatio, minSeqLength, maxSeqLength);

        public bool IsTextFile(
            double maxRatio = NonPrintableCharactersMaxRatio, 
            int minSeqLength = CheckSequenceMinimumLength, 
            int maxSeqLength = CheckSequenceMaximumLength)
            => !IsBinaryFile(FullName, maxRatio, minSeqLength, maxSeqLength);

        public bool IsBinaryFile(
            double maxRatio = NonPrintableCharactersMaxRatio, 
            int minSeqLength = CheckSequenceMinimumLength, 
            int maxSeqLength = CheckSequenceMaximumLength) =>
            IsBinaryFile(FullName, maxRatio, minSeqLength, maxSeqLength);

        public static bool IsBinaryFile(
            string filePath, 
            double maxRatio = NonPrintableCharactersMaxRatio, 
            int minSeqLength = CheckSequenceMinimumLength, 
            int maxSeqLength = CheckSequenceMaximumLength)
        {
            using var fs = new StreamReader(new FileStream(filePath, FileMode.Open),null,true, CheckSequenceMaximumLength);
            var r = false;
            double nonPrintableCount = 0;
            double rt = 0;
            int i = 0;
            int c;

            while (i < maxSeqLength)
            {
                if (fs.Peek() >= 0)
                    c = fs.Read();
                else
                    break;
                if ((c != 10 && c != 13 && c < 32) || c=='?') nonPrintableCount++;
                rt = nonPrintableCount / (i + 1) * 100d;
                if (rt > maxRatio && i > minSeqLength)
                {
                    r = true;
                    break;
                }
                i++;
            }
            r |= rt > maxRatio;

            return r;
        }
    }
}

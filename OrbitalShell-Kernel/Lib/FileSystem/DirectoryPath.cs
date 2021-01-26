using System.IO;
using System.Linq;
using static OrbitalShell.DotNetConsole;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Lib.FileSystem
{
    /// <summary>
    /// directory path wrapper<br/>
    /// supports path syntaxes :<br/>
    /// c:\MyDir\MyFile.txt<br/>
    /// c:/MyDir/MyFile.txt<br/>
    /// c:\MyDir\<br/>
    /// c:\MyDir<br/>
    /// c:/MyDir<br/>
    /// c:/MyDir/<br/>
    /// MyDir\MySubdir<br/>
    /// MyDir/MySubdir<br/>
    /// \\MyServer\MyShare\<br/>
    /// \\MyServer/MyShare/<br/>
    /// ~\...<br/>
    /// ~/...<br/>
    /// </summary>
    public class DirectoryPath : FileSystemPath
    {
        public DirectoryInfo DirectoryInfo { get; protected set; }

        public DirectoryPath(string path) : base(new DirectoryInfo(path))
        {
            DirectoryInfo = (DirectoryInfo)FileSystemInfo;
        }

        public bool IsEmpty => DirectoryInfo.EnumerateFileSystemInfos().Count() == 0;        

        public override bool CheckExists(CommandEvaluationContext context,bool dumpError = true)
        {
            if (!DirectoryInfo.Exists)
            {
                if (dumpError) context.Errorln($"directory doesn't exists: {this}");
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return DirectoryInfo.FullName;
        }
    }
}

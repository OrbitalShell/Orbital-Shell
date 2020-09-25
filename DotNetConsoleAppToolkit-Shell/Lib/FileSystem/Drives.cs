using System;
using System.IO;
using System.Linq;
using static DotNetConsoleAppToolkit.Lib.Str;

namespace DotNetConsoleAppToolkit.Lib.FileSystem
{
    public class Drives
    {
        public static string GetCurrentDriveInfo() => GetDriveInfo(Environment.CurrentDirectory,true);

        public static string GetDriveInfo(string path, bool printFileSystemInfo = false, string prefix = "", string postfix = "", string sep = "",int digits=0)
        {
            var rootDirectory = Path.GetPathRoot(path.ToLower());
            var di = DriveInfo.GetDrives().Where(x => x.RootDirectory.FullName.ToLower() == rootDirectory).FirstOrDefault();
            return (di == null) ? "?" : $"{(printFileSystemInfo?(rootDirectory+" "):"")}{HumanFormatOfSize(di.AvailableFreeSpace, digits, sep,prefix,postfix)}/{HumanFormatOfSize(di.TotalSize, digits, sep,prefix,postfix)}{(printFileSystemInfo?$"({di.DriveFormat})":"")}";
        }
    }
}

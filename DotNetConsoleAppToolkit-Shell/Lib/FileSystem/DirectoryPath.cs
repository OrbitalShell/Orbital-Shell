﻿using System.IO;
using System.Linq;
using static DotNetConsoleAppToolkit.DotNetConsole;

namespace DotNetConsoleAppToolkit.Lib.FileSystem
{
    public class DirectoryPath : FileSystemPath
    {
        public DirectoryInfo DirectoryInfo { get; protected set; }

        public DirectoryPath(string path) : base(new DirectoryInfo(path))
        {
            DirectoryInfo = (DirectoryInfo)FileSystemInfo;
        }

        public bool IsEmpty => DirectoryInfo.EnumerateFileSystemInfos().Count() == 0;        

        public override bool CheckExists(bool dumpError=true)
        {
            if (!DirectoryInfo.Exists)
            {
                if (dumpError)
                    Errorln($"directory doesn't exists: {this}");
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

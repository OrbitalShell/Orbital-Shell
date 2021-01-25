using System;
using System.Collections.Generic;
using System.Text;

namespace OrbitalShell.Lib.Data
{
    public class FindCounts
    {
        public int FoldersCount;
        public int FilesCount;
        public int ScannedFoldersCount;
        public int ScannedFilesCount;
        public DateTime BeginDateTime;
        public TimeSpan Elapsed;

        public FindCounts()
        {
            BeginDateTime = DateTime.Now;
        }
    }
}

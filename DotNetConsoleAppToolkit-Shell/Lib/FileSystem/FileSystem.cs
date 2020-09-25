﻿using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Lib.Data;
using System;
using System.Collections.Generic;
using System.IO;
using static DotNetConsoleAppToolkit.DotNetConsole;
using static DotNetConsoleAppToolkit.Lib.Str;

namespace DotNetConsoleAppToolkit.Lib.FileSystem
{
    public static partial class FileSystem
    {
        public static List<FileSystemPath> FindItems(
            CommandEvaluationContext context,
            string path,
            string pattern,
            bool top,
            bool all,
            bool dirs,
            bool attributes,
            bool shortPathes,
            string contains,
            bool checkPatternOnFullName,
            FindCounts counts,
            bool print,
            bool alwaysSelectDirs = false,
            bool ignoreCase = false)
        {
            var dinf = new DirectoryInfo(path);
            List<FileSystemPath> items = new List<FileSystemPath>();
            bool hasPattern = !string.IsNullOrWhiteSpace(pattern);
            bool hasContains = !string.IsNullOrWhiteSpace(contains);

            if (context.CommandLineProcessor.CancellationTokenSource.Token.IsCancellationRequested)
                return items;

            try
            {
                counts.ScannedFoldersCount++;
                var scan = dinf.GetFileSystemInfos();

                foreach (var fsinf in scan)
                {
                    var sitem = FileSystemPath.Get(fsinf);

                    if (sitem.IsDirectory)
                    {
                        if ((dirs || all) && (alwaysSelectDirs || (!hasPattern || MatchWildcard(pattern, checkPatternOnFullName ? sitem.FileSystemInfo.FullName : sitem.FileSystemInfo.Name, ignoreCase))))
                        {
                            items.Add(sitem);
                            if (print) sitem.Print(attributes, shortPathes, "", Br);
                            counts.FoldersCount++;
                        }
                        else
                            sitem = null;

                        if (!top)
                            items.AddRange(FindItems(context,fsinf.FullName, pattern, top, all, dirs, attributes, shortPathes, contains, checkPatternOnFullName, counts, print, alwaysSelectDirs, ignoreCase));
                    }
                    else
                    {
                        counts.ScannedFilesCount++;
                        if (!dirs && (!hasPattern || MatchWildcard(pattern, checkPatternOnFullName ? sitem.FileSystemInfo.FullName : sitem.FileSystemInfo.Name, ignoreCase)))
                        {
                            if (hasContains)
                            {
                                try
                                {
                                    var str = File.ReadAllText(sitem.FileSystemInfo.FullName);
                                    if (!str.Contains(contains))
                                        sitem = null;
                                }
                                catch (Exception ex)
                                {
                                    Errorln($"file read error: {ex.Message} when accessing file: {sitem.PrintableFullName}");
                                }
                            }
                            if (sitem != null)
                            {
                                counts.FilesCount++;
                                items.Add(sitem);
                                if (print) sitem.Print(attributes, shortPathes, "", Br);
                            }
                        }
                        else
                            sitem = null;
                    }

                    if (context.CommandLineProcessor.CancellationTokenSource.Token.IsCancellationRequested)
                        return items;
                }
                return items;
            }
            catch (UnauthorizedAccessException)
            {
                Errorln($"unauthorized access to {new DirectoryPath(path).PrintableFullName}");
                return items;
            }
        }

    }
}

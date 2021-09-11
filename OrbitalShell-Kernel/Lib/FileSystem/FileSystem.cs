using System;
using System.Collections.Generic;
using System.IO;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Console.Formats;
using OrbitalShell.Lib.Data;

using static OrbitalShell.Component.EchoDirective.Shortcuts;
using static OrbitalShell.Lib.Str;

namespace OrbitalShell.Lib.FileSystem
{
    /// <summary>
    /// os file system editing and helper operations
    /// </summary>
    public static partial class FileSystem
    {
        /// <summary>
        /// search items in file system
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <param name="pattern"></param>
        /// <param name="top"></param>
        /// <param name="all"></param>
        /// <param name="dirs"></param>
        /// <param name="attributes"></param>
        /// <param name="shortPathes"></param>
        /// <param name="contains"></param>
        /// <param name="checkPatternOnFullName"></param>
        /// <param name="counts"></param>
        /// <param name="print"></param>
        /// <param name="alwaysSelectDirs"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="printMatches"></param>
        /// <returns></returns>
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
            bool ignoreCase = false,
            bool printMatches = false)
        {
            bool isFile = File.Exists(path);
            var dinf = isFile ? null : new DirectoryInfo(path);
            List<FileSystemPath> items = new List<FileSystemPath>();
            bool hasPattern = !string.IsNullOrWhiteSpace(pattern);
            bool hasContains = !string.IsNullOrWhiteSpace(contains);

            if (context.CommandLineProcessor.IsCancellationRequested)
                return items;

            try
            {
                if (!isFile) counts.ScannedFoldersCount++;
                var scan = isFile ? new FileSystemInfo[] { new FileInfo(path) }
                    : dinf.GetFileSystemInfos();

                foreach (var fsinf in scan)
                {
                    var sitem = FileSystemPath.Get(fsinf);

                    if (sitem.IsDirectory)
                    {
                        if ((dirs || all) && (alwaysSelectDirs || (!hasPattern || MatchWildcard(pattern, checkPatternOnFullName ? sitem.FileSystemInfo.FullName : sitem.FileSystemInfo.Name, ignoreCase))))
                        {
                            items.Add(sitem);
                            if (print) sitem.Echo(new EchoEvaluationContext(context.Out, context, new FileSystemPathFormattingOptions(attributes, shortPathes, "", Br)));
                            counts.FoldersCount++;
                        }
                        else
                            sitem = null;

                        if (!top)
                            items.AddRange(FindItems(context, fsinf.FullName, pattern, top, all, dirs, attributes, shortPathes, contains, checkPatternOnFullName, counts, print, alwaysSelectDirs, ignoreCase, printMatches));
                    }
                    else
                    {
                        counts.ScannedFilesCount++;
                        if (!dirs && (!hasPattern || MatchWildcard(pattern, checkPatternOnFullName ? sitem.FileSystemInfo.FullName : sitem.FileSystemInfo.Name, ignoreCase)))
                        {
                            var matches = new List<string>();
                            if (hasContains)
                            {
                                try
                                {
                                    if (!FilePath.IsBinaryFile(sitem.FileSystemInfo.FullName))
                                    {
                                        // skip non text files
                                        var (lines, platform, eol) = TextFileReader.ReadAllLines(sitem.FileSystemInfo.FullName);
                                        bool match = false;
                                        for (int i = 0; i < lines.Length; i++)
                                        {
                                            if (lines[i].Contains(contains))
                                            {
                                                match |= true;
                                                if (printMatches)
                                                {
                                                    int j = lines[i].IndexOf(contains);
                                                    var loc = $"\t{context.ShellEnv.Colors.MarginText} {$"line {i},col {j}".PadRight(24)} ";
                                                    var txt = lines[i].Replace(contains, context.ShellEnv.Colors.TextExtractSelectionBlock + contains + Rdc + context.ShellEnv.Colors.TextExtract);
                                                    matches.Add(loc + context.ShellEnv.Colors.TextExtract + txt + Rdc);
                                                }
                                            }
                                        }
                                        if (!match) sitem = null;
                                    }
                                    else sitem = null;
                                }
                                catch (Exception ex)
                                {
                                    context.Errorln($"file read error: {ex.Message} when accessing file: {sitem.PrintableFullName}");
                                    sitem = null;
                                }
                            }
                            if (sitem != null)
                            {
                                counts.FilesCount++;
                                items.Add(sitem);
                                if (print && context.Out.IsModified && matches.Count > 0) context.Out.Echoln("");
                                if (print) sitem.Echo(new EchoEvaluationContext(context.Out, context, new FileSystemPathFormattingOptions(attributes, shortPathes, "", Br)));
                            }
                            if (matches.Count > 0) matches.ForEach(x => context.Out.Echoln(x)); matches.Clear();
                        }
                        else
                            sitem = null;
                    }

                    if (context.CommandLineProcessor.IsCancellationRequested)
                        return items;
                }
                return items;
            }
            catch (UnauthorizedAccessException)
            {
                context.Errorln($"unauthorized access to {new DirectoryPath(path).PrintableFullName}");
                return items;
            }
        }

    }
}

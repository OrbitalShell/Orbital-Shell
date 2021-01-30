using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Parsing;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.CommandLine.Variable;
using OrbitalShell.Console;
using OrbitalShell.Lib;
using OrbitalShell.Lib.Data;
using OrbitalShell.Lib.FileSystem;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static OrbitalShell.Component.CommandLine.CommandLineReader.Interaction;
using static OrbitalShell.Lib.FileSystem.FileSystem;
using static OrbitalShell.Lib.Str;
using sc = System.Console;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component;
using OrbitalShell.Component.CommandLine;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace OrbitalShell.Commands.FileSystem
{
    [Commands("commands related to files,directories,mounts/filesystems and disks")]
    [CommandsNamespace(CommandNamespace.fs)]
    public class FileSystemCommands : ICommandsDeclaringType
    {
        [Command("search for files and/or folders")]
        public CommandResult<(List<FileSystemPath> items, FindCounts counts)> Find(
            CommandEvaluationContext context,
            [Parameter("search path")] DirectoryPath path,
            [Option("p", "pattern", "select names that matches the pattern", true, true)] string pattern,
            [Option("i", "non-sensitive", "if set and p is set, perform a non case sensisitive search")] bool ignoreCase,
            [Option("f", "fullname", "check pattern on fullname instead of name")] bool checkPatternOnFullName,
            [Option("c", "contains", "files that contains the string", true, true)] string contains,
            [Option("a", "attr", "print file system attributes")] bool attributes,
            [Option("s", "short", "print short pathes")] bool shortPathes,
            [Option("l", "all", "select both files and directories")] bool all,
            [Option("d", "dir", "select only directories")] bool dirs,
            [Option("t", "top", "search in top directory only")] bool top
            )
        {
            if (path.CheckExists(context))
            {
                var sp = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;
                var counts = new FindCounts();
                var items = FindItems(context, path.FullName, sp, top, all, dirs, attributes, shortPathes, contains, checkPatternOnFullName, counts, true, false, ignoreCase);
                var f = DefaultForegroundCmd;
                counts.Elapsed = DateTime.Now - counts.BeginDateTime;
                if (items.Count > 0) context.Out.Echoln();
                context.Out.Echoln($"found {context.ShellEnv.Colors.Numeric}{Plur("file", counts.FilesCount, f)} and {context.ShellEnv.Colors.Numeric}{Plur("folder", counts.FoldersCount, f)}. scanned {context.ShellEnv.Colors.Numeric}{Plur("file", counts.ScannedFilesCount, f)} in {context.ShellEnv.Colors.Numeric}{Plur("folder", counts.ScannedFoldersCount, f)} during {TimeSpanDescription(counts.Elapsed, context.ShellEnv.Colors.Numeric.ToString(), f)}");
                return new CommandResult<(List<FileSystemPath>, FindCounts)>((items, counts));
            }
            return new CommandResult<(List<FileSystemPath>, FindCounts)>((new List<FileSystemPath>(), new FindCounts()), ReturnCode.Error);
        }

        [Flags]
        public enum DirSort
        {
            /// <summary>
            /// sort not specified
            /// </summary>
            not_specified = 0,

            /// <summary>
            /// sort by name or fullname
            /// </summary>
            name = 1,

            /// <summary>
            /// sort by size
            /// </summary>
            size = 2,

            /// <summary>
            /// sort by extension
            /// </summary>
            ext = 4,

            /// <summary>
            /// files on top
            /// </summary>
            file = 8,

            /// <summary>
            /// dirs on top
            /// </summary>
            dir = 16,

            /// <summary>
            /// reverse sort
            /// </summary>
            rev = 32
        }

        // TODO: --sort=... or -s= and no short name should be possible (symbol =)
        [Command("list files and folders in a path. eventually recurse in sub paths")]
        public CommandResult<(List<FileSystemPath> items, FindCounts counts)> Dir(
            CommandEvaluationContext context,
            [Parameter("path where to list files and folders. if not specified is set to the current directory. use wildcards * and ? to filter files and folders names", true)] WildcardFilePath path,
            [Option("n", "name", "names only: do not print file system attributes")] bool noattributes,
            [Option("r", "recurse", "also list files and folders in sub directories. force display files full path")] bool recurse,
            [Option("w", "wide", "displays file names on several columns so output fills console width (only if not recurse mode). disable print of attributes")] bool wide,
            [Option("f", "file", "filter list on files (exclude directories). avoid -d")] bool filesOnly,
            [Option("d", "dir", "filter list on directories (exclude files)")] bool dirsOnly,
            [Option("s", "sort", "sort list of files and folders. defaults is alphabetic, mixing files and folders", true, true)] DirSort sort = DirSort.name
            )
        {
            var r = new List<FileSystemPath>();
            path ??= new WildcardFilePath(Environment.CurrentDirectory);
            if (path.CheckExists(context))
            {
                var counts = new FindCounts();

                var items = FindItems(context, path.FullName, path.WildCardFileName ?? "*", !recurse, true, false, !noattributes, !recurse, null, false, counts, false, false);

                if (filesOnly) dirsOnly = false;
                if (filesOnly) items = items.Where(x => x.IsFile).ToList();
                if (dirsOnly) items = items.Where(x => x.IsDirectory).ToList();

                // apply sorts - default (.net) is name (==DirSort.name==1) - possible DirSort.not_specified==0 == default sort by name
                if (sort != DirSort.not_specified && sort != DirSort.name)
                {
                    IEnumerable<IGrouping<string, FileSystemPath>> grp = null;

                    if (sort.HasFlag(DirSort.ext))
                        grp = items.GroupBy((x) => Path.GetExtension(x.FullName));
                    else if (sort.HasFlag(DirSort.file))
                        grp = items.GroupBy((x) => x.IsDirectory + "");
                    else if (sort.HasFlag(DirSort.dir))
                        grp = items.GroupBy((x) => x.IsFile + "");
                    else if (sort.HasFlag(DirSort.size))
                        grp = items.GroupBy((x) => !x.IsFile + "");

                    var lst = grp.ToList();
                    lst.Sort((x, y) => x.Key.CompareTo(y.Key)); // sort alpha
                    if (!sort.HasFlag(DirSort.file) && !sort.HasFlag(DirSort.dir) && !sort.HasFlag(DirSort.size) && sort.HasFlag(DirSort.rev)) lst.Reverse();

                    items.Clear();
                    foreach (var g in lst)
                    {
                        var glst = g.ToList();
                        if (!sort.HasFlag(DirSort.size))
                            glst.Sort((x, y) => x.FullName.CompareTo(y.FullName));    // sort alpha
                        else
                            glst.Sort((x, y) => x.Length.CompareTo(y.Length));    // sort size
                        if (sort.HasFlag(DirSort.rev)) glst.Reverse();
                        items.AddRange(glst);
                    }
                }

                var f = DefaultForegroundCmd;
                long totFileSize = 0;
                var cancellationTokenSource = new CancellationTokenSource();
                if (wide) noattributes = true;

                // here we self handle the console Cancel event so we can output a partial result even if the seek phasis has been broke. print can also be broke.
                // TODO: seems there is now a simple way to do this. Just use the context cancellation token (as in findItems) : 
                // TODO: if (context.CommandLineProcessor.CancellationTokenSource.Token.IsCancellationRequested) // then cancel com

                void postCmd(object o, EventArgs e)
                {
                    sc.CancelKeyPress -= cancelCmd;
                    context.Out.Echoln($"{Tab}{context.ShellEnv.Colors.Numeric}{Plur("file", counts.FilesCount, f),-30}{HumanFormatOfSize(totFileSize, 2, " ", context.ShellEnv.Colors.Numeric.ToString(), f)}");
                    context.Out.Echoln($"{Tab}{context.ShellEnv.Colors.Numeric}{Plur("folder", counts.FoldersCount, f),-30}{Drives.GetDriveInfo(path.FileSystemInfo.FullName, false, context.ShellEnv.Colors.Numeric.ToString(), f, " ", 2)}");
                }
                void cancelCmd(object o, ConsoleCancelEventArgs e)
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                }
                int printResult()
                {
                    var i = 0;

                    int maxitlength = 0;
                    counts.FilesCount = counts.FoldersCount = 0;
                    foreach (var item in items)
                    {
                        if (item.IsFile)
                        {
                            totFileSize += ((FileInfo)item.FileSystemInfo).Length;
                            counts.FilesCount++;
                        }
                        else
                            counts.FoldersCount++;
                        maxitlength = Math.Max(item.Name.Length, maxitlength);
                    }
                    maxitlength += 4;
                    var (id, left, top, right, bottom) = DotNetConsole.ActualWorkArea();
                    var nbcols = Math.Floor((double)(right - left + 1) / (double)maxitlength);

                    int nocol = 0;
                    foreach (var item in items)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                            return i;

                        item.Echo(
                            new EchoEvaluationContext(
                                context.Out, context,
                                new FileSystemPathFormattingOptions(
                                    !noattributes,
                                    !recurse,
                                    "",
                                    ((!wide || recurse || nocol == nbcols - 1) ? Br : ""),
                                    (wide && !recurse) ? maxitlength : -1, "")));
                        i++;
                        nocol++;
                        if (nocol == nbcols)
                            nocol = 0;
                    }
                    if (!recurse && wide && nocol < nbcols && nocol > 0) context.Out.Echoln();
                    return i;
                }
                sc.CancelKeyPress += cancelCmd;
                var task = Task.Run<int>(() => printResult(),
                    cancellationTokenSource.Token);
                try
                {
                    task.Wait(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    var res = task.Result;
                }
                postCmd(null, null);
                return new CommandResult<(List<FileSystemPath>, FindCounts)>((items, counts));
            }
            return new CommandResult<(List<FileSystemPath>, FindCounts)>((r, new FindCounts()), ReturnCode.Error);
        }

        [Command("sets the path of the working directory")]
        public CommandResult<DirectoryPath> Cd(
            CommandEvaluationContext context,
            [Parameter("path where to list files and folders. if not specified is equal to the current directory", true)] DirectoryPath path
            )
        {
            path ??= new DirectoryPath(Path.GetPathRoot(Environment.CurrentDirectory));
            if (path.CheckExists(context))
            {
                var bkpath = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = path.FullName;
                }
                catch (UnauthorizedAccessException)
                {
                    context.Errorln($"unauthorized access to {path.PrintableFullName}");
                    Environment.CurrentDirectory = bkpath;
                    return new CommandResult<DirectoryPath>(path, ReturnCode.Error);
                }
                return new CommandResult<DirectoryPath>(path, ReturnCode.OK);
            }
            else
                return new CommandResult<DirectoryPath>(path, ReturnCode.Error);
        }

        [Command("print the path of the current working directory")]
        public CommandResult<DirectoryPath> Pwd(
            CommandEvaluationContext context,
            [Option("s", "short", "short display: do not print file system attributes")] bool noattributes
            )
        {
            var path = new DirectoryPath(Environment.CurrentDirectory);
            if (path.CheckExists(context))
            {
                path.Echo(new EchoEvaluationContext(context.Out, context, new FileSystemPathFormattingOptions(!noattributes, false, "", Br)));
                return new CommandResult<DirectoryPath>(path);
            }
            else
                return new CommandResult<DirectoryPath>();
        }

        [Command("print informations about drives/mount points")]
        [CommandName("DriveInfo")]
        public CommandResult<List<DriveInfo>> Driveinfo(
            CommandEvaluationContext context,
            [Parameter("drive name for which informations must be printed. if no drive specified, list all drives", true)] string drive,
            [Option("b", "borders", "if set add table borders")] bool borders
            )
        {
            var drives = DriveInfo.GetDrives().AsQueryable();
            if (drive != null)
            {
                drives = drives.Where(x => x.Name.Equals(drive, CommandLineParser.SyntaxMatchingRule));
                if (drives.Count() == 0)
                {
                    context.Errorln($"drive \"{drive}\" not found");
                }
            }
            var table = new DataTable();
            table.AddColumns("name", "label", "type", "format", "bytes");
            foreach (var di in drives)
            {
                var f = DefaultForegroundCmd;
                var row = table.NewRow();
                try
                {
                    row["name"] = $"{context.ShellEnv.Colors.Highlight}{di.Name}{f}";
                    row["label"] = $"{context.ShellEnv.Colors.Highlight}{di.VolumeLabel}{f}";
                    row["type"] = $"{context.ShellEnv.Colors.Name}{di.DriveType}{f}";
                    row["format"] = $"{context.ShellEnv.Colors.Name}{di.DriveFormat}{f}";
                    row["bytes"] = (di.TotalSize == 0) ? "" : $"{HumanFormatOfSize(di.TotalFreeSpace, 2, " ", context.ShellEnv.Colors.Numeric.ToString(), f)}{f}/{context.ShellEnv.Colors.Numeric}{HumanFormatOfSize(di.TotalSize, 2, " ", context.ShellEnv.Colors.Numeric.ToString(), f)} {f}({context.ShellEnv.Colors.Highlight}{Math.Round((double)di.TotalFreeSpace / (double)di.TotalSize * 100d, 2)}{f} %)";
                }
                catch (UnauthorizedAccessException)
                {
                    context.Errorln($"unauthorized access to drive {di.Name}");
                    row["name"] = $"{context.ShellEnv.Colors.Highlight}{di.Name}{f}";
                    row["label"] = "?";
                    row["type"] = "?";
                    row["format"] = "?";
                    row["bytes"] = "?";
                }
                catch (Exception ex)
                {
                    context.Errorln($"error when accessing drive {di.Name}: {ex.Message}");
                    row["name"] = $"{context.ShellEnv.Colors.Highlight}{di.Name}{f}";
                    row["label"] = "?";
                    row["type"] = "?";
                    row["format"] = "?";
                    row["bytes"] = "?";
                }
                table.Rows.Add(row);
            }
            table.Echo(
                new EchoEvaluationContext(context.Out, context,
                    new TableFormattingOptions(context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.display_tableFormattingOptions))
                    { NoBorders = !borders }));

            return new CommandResult<List<DriveInfo>>(drives.ToList());
        }

        [Command("remove file(s) and/or the directory(ies)")]
        public CommandResult<(List<FileSystemPath> items, FindCounts counts)> Rm(
            CommandEvaluationContext context,
            [Parameter("file or folder path")] WildcardFilePath path,
            [Option("r", "recurse", "also remove files and folders in sub directories")] bool recurse,
            [Option("i", "interactive", "prompt before any removal")] bool interactive,
            [Option("v", "verbose", "explain what is being done")] bool verbose,
            [Option("e", "delete-empty", "remove empty directories")] bool rmEmptyDirs,
            [Option("s", "short", "short display: do not print file system attributes when verbose")] bool noattributes,
            [Option("m", "simulate", "don't remove any file/or folder, just simulate the operation (enable verbose)")] bool simulate
        )
        {
            var r = new List<FileSystemPath>();
            var counts = new FindCounts();
            if (path.CheckExists(context))
            {
                var items = FindItems(context, path.FullName, path.WildCardFileName ?? "*", !recurse, true, false, !noattributes, !recurse, null, false, counts, false, false);
                var cancellationTokenSource = new CancellationTokenSource();
                verbose |= simulate;
                void cancelCmd(object o, ConsoleCancelEventArgs e)
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                };
                void postCmd(object o, EventArgs e)
                {
                    sc.CancelKeyPress -= cancelCmd;
                }
                List<FileSystemPath> processRemove()
                {
                    var r = new List<FileSystemPath>();
                    foreach (var item in items)
                    {
                        if (cancellationTokenSource.IsCancellationRequested) return r;
                        bool deleted = false;
                        if (item.IsFile)
                        {
                            if (item.FileSystemInfo.Exists && !r.Contains(item))
                            {
                                if (interactive)
                                {
                                    if (Confirm("rm: remove file " + item.GetPrintableName(recurse)) && !simulate)
                                    {
                                        if (!simulate) item.FileSystemInfo.Delete();
                                        deleted = true;
                                    }
                                }
                                else
                                {
                                    if (!simulate) item.FileSystemInfo.Delete();
                                    deleted = true;
                                }
                            }
                        }
                        else
                        {
                            var dp = (DirectoryPath)item;
                            if ((rmEmptyDirs && dp.IsEmpty) || recurse)
                            {
                                if (dp.DirectoryInfo.Exists && !r.Contains(dp))
                                {
                                    if (interactive)
                                        r.Merge(RecurseInteractiveDeleteDir(context, dp, simulate, noattributes, verbose, cancellationTokenSource));
                                    else
                                    {
                                        if (!simulate) dp.DirectoryInfo.Delete(recurse);
                                        deleted = true;
                                    }
                                }
                            }
                        }
                        if (deleted)
                        {
                            if (verbose) item.Echo(new EchoEvaluationContext(context.Out, context, new FileSystemPathFormattingOptions(!noattributes, !recurse, "", Br, -1, "removed ")));
                            r.Add(item);
                        }
                    }
                    return r;
                };
                sc.CancelKeyPress += cancelCmd;
                var task = Task.Run(() => processRemove(), cancellationTokenSource.Token);
                try
                {
                    task.Wait(cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    r = task.Result;
                }
                postCmd(null, null);
                return new CommandResult<(List<FileSystemPath>, FindCounts)>((r, counts), ReturnCode.OK);
            }
            else
                return new CommandResult<(List<FileSystemPath>, FindCounts)>((r, counts), ReturnCode.Error);
        }

        [Command("move or rename files and directories",
@"- if multiple source, move to a directory that must exists
- if source is a file or a directory and dest is an existing directory move the source
- if source and target are a file that exists remame the source and replace the dest
- if dest doesn't exists rename the source that must be a file or a directory")]
        public CommandResult<(List<(FileSystemPath source, FileSystemPath target)> items, FindCounts counts)> Mv(
            CommandEvaluationContext context,
            [Parameter("source: file/directory or several corresponding to a wildcarded path")] WildcardFilePath source,
            [Parameter(1, "target: a file or a directory")] FileSystemPath dest,
            [Option("i", "interactive", "prompt before overwrite")] bool interactive,
            [Option("v", "verbose", "explain what is being done, but actually does nothing")] bool verbose
            )
        {
            if (source.CheckExists(context))
            {
                var counts = new FindCounts();
                var items = FindItems(context, source.FullName, source.WildCardFileName ?? "*", true, true, false, true, false, null, false, counts, false, false);
                var sourceCount = items.Count;
                List<(FileSystemPath src, FileSystemPath tgt)> r = new List<(FileSystemPath src, FileSystemPath tgt)>();
                if (sourceCount > 1)
                {
                    if (dest.CheckExists(context))
                    {
                        if (!dest.IsDirectory)
                        {
                            context.Errorln("target must be a directory");
                            return new CommandResult<(List<(FileSystemPath, FileSystemPath)> items, FindCounts counts)>(
                                 (new List<(FileSystemPath, FileSystemPath)> { (source, dest) }, counts), ReturnCode.Error
                                );
                        }
                        else
                        {
                            // move multiple source to dest
                            foreach (var item in items)
                            {
                                var msg = $"move {item.GetPrintableName()} to {dest.GetPrintableName()}";
                                if (!interactive || Confirm("mv: " + msg))
                                {
                                    if (source.IsFile)
                                    {
                                        var newdest = Path.Combine(dest.FullName, item.Name);
                                        r.Add((item, new FileSystemPath(newdest)));
                                        File.Move(item.FullName, newdest);
                                    }
                                    else
                                    {
                                        var newdest = Path.Combine(dest.FullName, item.Name);
                                        Directory.Move(item.FullName, newdest);
                                        r.Add((item, new DirectoryPath(newdest)));
                                    }
                                    if (verbose) context.Out.Echoln(msg.Replace("move ", "moved "));
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (dest.CheckExists(context))
                    {
                        if (dest.IsDirectory)
                        {
                            // move one source to dest
                            var msg = $"move {source.GetPrintableNameWithWlidCard()} to {dest.GetPrintableName()}";
                            if (!interactive || Confirm("mv: " + msg))
                            {
                                if (source.IsFile)
                                {
                                    var newdest = Path.Combine(dest.FullName, source.NameWithWildcard);
                                    File.Move(source.FullNameWithWildcard, newdest);
                                    r.Add((new FilePath(source.FullNameWithWildcard), new FilePath(newdest)));
                                }
                                else
                                {
                                    var newdest = Path.Combine(dest.FullName, source.NameWithWildcard);
                                    Directory.Move(source.FullName, newdest);
                                    r.Add((source, new DirectoryPath(newdest)));
                                }
                                if (verbose) context.Out.Echoln(msg.Replace("move ", "moved "));
                            }
                        }
                        else
                        {
                            // rename source (file) to dest (overwrite dest)
                            var msg = $"rename {source.GetPrintableNameWithWlidCard()} to {dest.GetPrintableName()}";
                            if (!interactive || Confirm("mv: " + msg))
                            {
                                dest.FileSystemInfo.Delete();
                                File.Move(source.FullNameWithWildcard, dest.FullName);
                                r.Add((new FilePath(source.FullNameWithWildcard), dest));
                                if (verbose) context.Out.Echoln(msg.Replace("rename ", "renamed "));
                            }
                        }
                    }
                    else
                    {
                        // rename source to dest
                        var msg = $"rename {source.GetPrintableNameWithWlidCard()} to {dest.GetPrintableName()}";
                        if (!interactive || Confirm("mv: " + msg))
                        {
                            if (source.IsFile)
                            {
                                File.Move(source.FullNameWithWildcard, dest.FullName);
                                r.Add((new FilePath(source.FullNameWithWildcard), dest));
                            }
                            else
                            {
                                Directory.Move(source.FullName, dest.FullName);
                                r.Add((source, dest));
                            }
                            if (verbose) context.Out.Echoln(msg.Replace("rename ", "renamed "));
                        }
                    }
                }
                return new CommandResult<(List<(FileSystemPath source, FileSystemPath target)> items, FindCounts counts)>
                    ((r, counts));
            }
            else
                return new CommandResult<(List<(FileSystemPath, FileSystemPath)> items, FindCounts counts)>
                    ((new List<(FileSystemPath, FileSystemPath)> { (source, null) }, new FindCounts()));
        }

        List<FileSystemPath> RecurseInteractiveDeleteDir(
            CommandEvaluationContext context,
            DirectoryPath dir,
            bool simulate,
            bool noattributes,
            bool verbose,
            CancellationTokenSource cancellationTokenSource)
        {
            var fullname = true;
            var r = new List<FileSystemPath>();
            verbose |= simulate;
            if (cancellationTokenSource.IsCancellationRequested) return r;
            if (dir.IsEmpty || Confirm("rm: descend " + dir.GetPrintableName(fullname)))
            {
                foreach (var subdir in dir.DirectoryInfo.EnumerateDirectories())
                    r.Merge(RecurseInteractiveDeleteDir(context, new DirectoryPath(subdir.FullName), simulate, noattributes, verbose, cancellationTokenSource));

                var options = new FileSystemPathFormattingOptions(!noattributes, false, "", Br, -1, "removed ");
                var echoContext = new EchoEvaluationContext(context.Out, context, options);

                foreach (var subfile in dir.DirectoryInfo.EnumerateFiles())
                {
                    var subfi = new FilePath(subfile.FullName);
                    if (Confirm("rm: remove file " + subfi.GetPrintableName(fullname)))
                    {
                        if (!simulate) subfi.FileSystemInfo.Delete();
                        if (verbose) subfi.Echo(echoContext);
                        r.Add(subfi);
                    }
                }
                if (Confirm("rm: remove directory " + dir.GetPrintableName(fullname)))
                {
                    if (!simulate) dir.DirectoryInfo.Delete(true);
                    if (verbose) dir.Echo(echoContext);
                    r.Add(dir);
                }
            }
            return r;
        }
    }
}

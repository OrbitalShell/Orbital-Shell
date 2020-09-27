using DotNetConsoleAppToolkit.Shell.Commands.FileSystem;
using DotNetConsoleAppToolkit.Component.CommandLine;
using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using DotNetConsoleAppToolkit.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using static DotNetConsoleAppToolkit.Component.CommandLine.CommandLineReader.Interaction;
using static DotNetConsoleAppToolkit.DotNetConsole;
using static DotNetConsoleAppToolkit.Lib.TextFileReader;
using static DotNetConsoleAppToolkit.Lib.Str;
using sc = System.Console;
using static DotNetConsoleAppToolkit.Lib.FileSystem.FileSystem;
using System.IO;
using System.Runtime.InteropServices;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Lib.FileSystem;
using DotNetConsoleAppToolkit.Lib.Data;

namespace DotNetConsoleAppToolkit.Shell.Commands.TextFile
{
    [Commands("commands related to text files")]
    public class TextFileCommands : ICommandsDeclaringType
    {
        [Command("output the content of one or several text files")]
        public CommandResult<List<TextFileInfo>> More(
            CommandEvaluationContext context,
            [Parameter("file or folder path")] WildcardFilePath path,
            [Option("h", "hide line numbers")] bool hideLineNumbers
            )
        {
            if (path.CheckExists())
            {
                var counts = new FindCounts();
                var items = FindItems(context, path.FullName, path.WildCardFileName ?? "*", true, false, false, true, false, null, false, counts, false, false);
                var r = new List<TextFileInfo>();
                foreach (var item in items)
                {
                    PrintFile(context, (FilePath)item, hideLineNumbers);
                    r.Add( new TextFileInfo((FilePath)item, null,OSPlatform.Create("?"),null));
                }
                if (items.Count == 0)
                {
                    Errorln($"more: no such file: {path.OriginalPath}");
                    return new CommandResult<List<TextFileInfo>>( new List<TextFileInfo> { new TextFileInfo( new FilePath(path.OriginalPath),null,OSPlatform.Create("?"),null) }, ReturnCode.Error);
                }
                context.Out.ShowCur();
                return new CommandResult<List<TextFileInfo>>( r );
            }
            else
                return new CommandResult<List<TextFileInfo>>( new List<TextFileInfo> { new TextFileInfo( new FilePath(path.FullName), null, OSPlatform.Create("?"),null) }, ReturnCode.Error);
        }

        [SuppressMessage("Style", "IDE0071:Simplifier l’interpolation", Justification = "<En attente>")]
        [SuppressMessage("Style", "IDE0071WithoutSuggestion:Simplifier l’interpolation", Justification = "<En attente>")]
        TextFileInfo PrintFile(
            CommandEvaluationContext context, 
            FilePath file, 
            bool hideLineNumbers)
        {
            const int cl = -14;
            string quit = $"{context.ShellEnv.Colors.ParameterName}{$"q|Q",cl}{context.ShellEnv.Colors.Default}quit";
            string help = $"{context.ShellEnv.Colors.ParameterName}{$"h|H",cl}{context.ShellEnv.Colors.Default}print this help";
            string scrollnext = $"{context.ShellEnv.Colors.ParameterName}{$"space",cl}{context.ShellEnv.Colors.Default}display next lines of text, according to current screen size";
            string scrolllinedown = $"{context.ShellEnv.Colors.ParameterName}{$"down arrow",cl}{context.ShellEnv.Colors.Default}scroll one line down";
            string scrolllineup = $"{context.ShellEnv.Colors.ParameterName}{$"up arrow",cl}{context.ShellEnv.Colors.Default}scroll one line up";
            string pagedown = $"{context.ShellEnv.Colors.ParameterName}{$"right arrow",cl}{context.ShellEnv.Colors.Default}jump one page down, according to current screen size";
            string pageup = $"{context.ShellEnv.Colors.ParameterName}{$"left arrow",cl}{context.ShellEnv.Colors.Default}jump one page up, according to current screen size";
            string totop = $"{context.ShellEnv.Colors.ParameterName}{$"t|T",cl}{context.ShellEnv.Colors.Default}jump to the top of the file";
            string toend = $"{context.ShellEnv.Colors.ParameterName}{$"e|E",cl}{context.ShellEnv.Colors.Default}jump to the end of the file";

            var inputMaps = new List<InputMap>
            {
                new InputMap("q",quit),
                new InputMap("h",help),
                new InputMap(" ",scrollnext),
                new InputMap((str,key)=>key.Key==ConsoleKey.DownArrow?InputMap.ExactMatch:InputMap.NoMatch,scrolllinedown),
                new InputMap((str,key)=>key.Key==ConsoleKey.UpArrow?InputMap.ExactMatch:InputMap.NoMatch,scrolllineup),
                new InputMap((str,key)=>key.Key==ConsoleKey.RightArrow?InputMap.ExactMatch:InputMap.NoMatch,pagedown),
                new InputMap((str,key)=>key.Key==ConsoleKey.LeftArrow?InputMap.ExactMatch:InputMap.NoMatch,pageup),
                new InputMap("t",totop),
                new InputMap("e",toend)
            };

            var fileEncoding = file.GetEncoding(Encoding.Default);
            //var lines = fileEncoding == null ? File.ReadAllLines(file.FullName, fileEncoding).ToArray() : File.ReadAllLines(file.FullName).ToArray();
            var (rlines, filePlatform, eol) = ReadAllLines(file.FullName);
            var lines = rlines.ToArray();
            var nblines = lines.Length;

            var infos = $"    ({Plur("line", nblines)},encoding={(fileEncoding != null ? fileEncoding.EncodingName : "?")},eol={filePlatform})";
            var n = file.Name.Length + TabLength + infos.Length;
            var sep = "".PadRight(n + 1, '-');
            context.Out.Echoln($"{context.ShellEnv.Colors.TitleBar}{sep}");
            context.Out.Echoln($"{context.ShellEnv.Colors.TitleBar} {file.Name}{context.ShellEnv.Colors.TitleDarkText}{infos.PadRight(n - file.Name.Length, ' ')}");
            context.Out.Echoln($"{context.ShellEnv.Colors.TitleBar}{sep}{context.ShellEnv.Colors.Default}");

            var preambleHeight = 3;
            var linecollength = nblines.ToString().Length;
            var pos = 0;
            bool end = false;
            int y = 0, x = 0;
            var actualWorkArea = DotNetConsole.ActualWorkArea();
            int maxk = actualWorkArea.Bottom - actualWorkArea.Top + 1;
            int k = maxk;
            bool endReached = false;
            bool topReached = true;
            bool skipPrint = false;
            bool scroll1down = false;
            bool forcePrintInputBar = false;
            int decpos = 0;

            while (!end)
            {
                var h = k - 1 - preambleHeight;
                var curNbLines = Math.Min(nblines, h);
                var percent = nblines == 0 ? 100 : Math.Ceiling((double)(Math.Min(curNbLines + pos + decpos, nblines)) / (double)nblines * 100d);
                int i = 0;
                if (!skipPrint)
                    lock (ConsoleLock)
                    {
                        context.Out.HideCur();
                        while (i < curNbLines && pos + decpos + i < nblines)
                        {
                            if (context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested) 
                                return new TextFileInfo(file, rlines, filePlatform, eol);
                            var prefix = hideLineNumbers ? "" : (context.ShellEnv.Colors.Dark + "  " + (pos + decpos + i + 1).ToString().PadRight(linecollength, ' ') + "  ");
                            context.Out.Echoln(prefix + context.ShellEnv.Colors.Default + lines[pos + decpos + i]);
                            i++;
                        }
                        context.Out.ShowCur();
                        y = sc.CursorTop;
                        x = sc.CursorLeft;
                        endReached = pos + i >= nblines;
                        topReached = pos == 0;
                    }
                var inputText = $"--more--({percent}%)";

                var action = end ? quit : InputBar(inputText, inputMaps);
                end = (string)action == quit;

                var oldpos = pos;

                if ((string)action == scrollnext) { k = maxk; pos += k - 1 - preambleHeight; }
                if ((string)action == scrolllinedown && !endReached)
                {
                    if (!scroll1down)
                    {
                        scroll1down = true;
                        decpos = k - 1 - preambleHeight - 1;
                    }
                    pos++;
                    k = 2;
                }
                else
                {
                    scroll1down = false;
                    decpos = 0;
                }

                if ((string)action == totop) { k = maxk; pos = 0; if (pos != oldpos) context.Out.ClearScreen(); }
                if ((string)action == toend) { k = maxk; pos = Math.Max(0, nblines - maxk + 1); if (pos != oldpos) context.Out.ClearScreen(); }

                if ((string)action == scrolllineup && !topReached)
                {
                    context.Out.ClearScreen(); k = maxk; pos = Math.Max(0, pos - 1);
                }
                if ((string)action == pagedown && !endReached) { context.Out.ClearScreen(); k = maxk; pos += k - 1 - preambleHeight; }
                if ((string)action == pageup && !topReached) { context.Out.ClearScreen(); k = maxk; pos = Math.Max(0, pos - k + 1); }

                if ((string)action == help)
                {
                    var sepw = inputMaps.Select(x => ((string)x.Code).Length).Max();
                    var hsep = "".PadRight(sepw + 10, '-');
                    context.Out.Echoln(Br + hsep + Br);
                    inputMaps.ForEach(x => context.Out.Echoln((string)x.Code + Br));
                    context.Out.Echoln(hsep);
                    forcePrintInputBar = true;
                }

                preambleHeight = 0;
                skipPrint = oldpos == pos;

                lock (ConsoleLock)
                {
                    sc.CursorLeft = x;
                    if (forcePrintInputBar || !skipPrint || end)
                    {
                        context.Out.Echo("".PadLeft(inputText.Length, ' '));
                        sc.CursorLeft = x;
                        forcePrintInputBar = false;
                    }
                }
            }

            return new TextFileInfo(file,rlines, filePlatform, eol);
        }

        [Command("check integrity of one or several text files","output a message for each corrupted file.\nThese command will declares a text file to be not integre as soon that it detects than the ratio of non printable caracters (excepted CR,LF) is geater than a fixed amount when reading the file")]
        public CommandResult<List<FilePath>> FckIntegrity(
            CommandEvaluationContext context,
            [Parameter( "path of a file to be checked or path from where find files to to be checked")] FileSystemPath fileOrDir,
            [Option("p", "select names that matches the pattern", true, true)] string pattern,
            [Option("i", "if set and p is set, perform a non case sensisitive search")] bool ignoreCase,
            [Option("a", "print file system attributes")] bool printAttr,
            [Option("t", "search in top directory only")] bool top,
            [Option("q", "quiet mode: do not print error message below corrupted file name" )] bool quiet,
            [Option("r", "acceptable ratio of non printable characters",true,true)] double ratio = 30,
            [Option("s", "minimum size of analysed part of the text",true,true)] int minSeqLength = 1024
            )
        {
            var r = new List<FilePath>();
            if (fileOrDir.CheckExists())
            {
                if (fileOrDir.IsFile)
                {
                    var (isValid,filePath) = CheckIntegrity(context, new FilePath(fileOrDir.FullName), ratio, printAttr, minSeqLength, quiet);
                    if (!isValid) r.Add(filePath);
                    return new CommandResult<List<FilePath>>( r);
                }
                else
                {
                    var sp = string.IsNullOrWhiteSpace(pattern) ? "*" : pattern;
                    var counts = new FindCounts();
                    var items = FindItems(context, fileOrDir.FullName, sp, top, false, false, printAttr, false, null, false, counts, false, false, ignoreCase);
                    var f = context.ShellEnv.Colors.Default.ToString();
                    var elapsed = DateTime.Now - counts.BeginDateTime;
                    context.Out.Echoln($"found {context.ShellEnv.Colors.Numeric}{Plur("file", counts.FilesCount, f)} and {context.ShellEnv.Colors.Numeric}{Plur("folder", counts.FoldersCount, f)}. scanned {context.ShellEnv.Colors.Numeric}{Plur("file", counts.ScannedFilesCount, f)} in {context.ShellEnv.Colors.Numeric}{Plur("folder", counts.ScannedFoldersCount, f)} during {TimeSpanDescription(elapsed, context.ShellEnv.Colors.Numeric.ToString(), f)}");
                    if (items.Count > 0)
                    {
                        context.Out.Echoln($"analyzing files ({counts.FilesCount})...");
                        int corruptedFilesCount = 0;
                        foreach (var item in items)
                        {
                            if (context.CommandLineProcessor.CancellationTokenSource.Token.IsCancellationRequested)
                                break;
                            if (item.IsFile)
                                if (!CheckIntegrity(context, (FilePath)item, ratio, printAttr, minSeqLength, quiet).isValid)
                                {
                                    corruptedFilesCount++;
                                    r.Add((FilePath)item);
                                }
                        }
                        if (corruptedFilesCount > 0) context.Out.Echoln();
                        var crprt = (double)corruptedFilesCount / (double)counts.FilesCount * 100d;
                        context.Out.Echoln($"found {context.ShellEnv.Colors.Numeric}{Plur("corrupted file", corruptedFilesCount, f)} in {context.ShellEnv.Colors.Numeric}{Plur("file", counts.FilesCount, f)} corruption ratio={Cyan}{crprt}%");
                        return new CommandResult<List<FilePath>>( r);
                    } else
                        return new CommandResult<List<FilePath>>(r);
                }
            } else
                return new CommandResult<List<FilePath>>( new List<FilePath> { new FilePath(fileOrDir.FullName) }, ReturnCode.Error);
        }

        (bool isValid,FilePath filePath) CheckIntegrity(
            CommandEvaluationContext context,
            FilePath filePath,
            double maxRatio,
            bool printAttr,
            int minSeqLength,
            bool quiet
            )
        {
            var str = File.ReadAllText(filePath.FullName);
            var arr = str.ToCharArray();
            var r = true;
            double nonPrintableCount = 0;
            double rt = 0;
            var cti = arr.Length-1;

            for (int i=0;i<arr.Length;i++)
            {
                if (arr[i]!=10 && arr[i]!=13 && ( arr[i]<32 || arr[i]>255) ) nonPrintableCount++;
                rt = nonPrintableCount / (i+1) * 100d;
                if (rt>maxRatio && i>minSeqLength)
                {
                    cti = i;
                    r = false;
                    break;
                }
            }
            r &= rt <= maxRatio;
            if (!r)
            {
                filePath.Echo(context.Out, context, new FileSystemPathFormattingOptions(printAttr, false, "", !quiet ? $"{Red} seems corrupted from index {cti}: bad chars ratio={rt}%":""));
                context.Out.LineBreak();
            }
            return (r,filePath);
        }
    }
}

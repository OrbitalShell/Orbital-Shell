using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Console;
using OrbitalShell.Lib.FileSystem;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static OrbitalShell.Console.ANSI;
using static OrbitalShell.Lib.TextFileReader;
using sc = System.Console;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component;
using OrbitalShell.Component.CommandLine;

namespace OrbitalShell.Commands.Test
{
    [Commands("tests commands")]
    [CommandsNamespace(CommandNamespace.test)]
    public class TestCommands : ICommandsDeclaringType
    {
        [Command("print cursor info")]
        public CommandResult<Point> CursorInfo(CommandEvaluationContext context)
        {
            int x = sc.CursorLeft, y = sc.CursorTop;
            context.Out.Echoln($"crx={x} cry={y}");
            //context.Out.Echoln($"{(char)27}[6n"); // test query cursor - no return in vscode conPty integrated terminal , return ok in windows terminal
            return new CommandResult<Point>(new Point(x, y));
        }

        [Command("backup ansi cursor pos")]
        public CommandResult<Point> CursorPosBackup(CommandEvaluationContext context)
        {
            int x = sc.CursorLeft, y = sc.CursorTop;
            context.Out.Echo(ANSI.DECSC);
            return new CommandResult<Point>(new Point(x, y));
        }

        [Command("restore ansi cursor pos")]
        public CommandResult<Point> CursorPosRestore(CommandEvaluationContext context)
        {
            int x = sc.CursorLeft, y = sc.CursorTop;
            context.Out.Echo(ANSI.DECRC);
            return new CommandResult<Point>(new Point(x, y));
        }

        [Command("set console windows size")]
        public CommandVoidResult ConsoleSetWindowSize(
            CommandEvaluationContext context,
            [Parameter(0, "width")] int w,
            [Parameter(1, "height")] int h)
        {
            System.Console.WindowWidth = w;
            System.Console.WindowHeight = h;
            return CommandVoidResult.Instance;
        }

        [Command("set console buffer size")]
        public CommandVoidResult ConsoleSetBufferSize(
            CommandEvaluationContext context,
            [Parameter(0, "width")] int w,
            [Parameter(1, "height")] int h)
        {
            System.Console.WindowWidth = w;
            System.Console.WindowHeight = h;
            return CommandVoidResult.Instance;
        }

        [Command("check end of line symbols of a file")]
        public CommandResult<List<string>> FileEol(
            CommandEvaluationContext context,
            [Parameter("file path")] FilePath file)
        {
            var r = new List<string>();
            if (file.CheckExists(context))
            {
                var (_, eolCounts, _) = GetEOLCounts(File.ReadAllText(file.FullName));
                foreach (var eol in eolCounts)
                {
                    var s = $"{eol.eol}={eol.count}";
                    r.Add(s);
                    context.Out.Echoln(s);
                }
                return new CommandResult<List<string>>(r);
            }
            else return new CommandResult<List<string>>(r, ReturnCode.Error);
        }

        [Command("echo an ANSI / VT-100 sequence")]
        public CommandVoidResult AnsiSeq(
            CommandEvaluationContext context,
            [Parameter(0, "esc sequence (text behind ESC). replace character @ by ESC (\\x1b) to allow write new sequences in the string parameter")] string seq,
            [Option("c", "char", "character to be used for ESC", true, true, (char)27)] char c
        )
        {
            seq = seq.Replace(/*"@"*/"" + c, ESC);
            context.Out.Echoln(ANSI.ESC + seq);
            return CommandVoidResult.Instance;
        }

        [Command("echo an unicode character")]
        public CommandVoidResult Unicode(
            CommandEvaluationContext context,
            [Parameter(0, "decimal unicode character index")] int n
        )
        {
            context.Out.Echoln("" + (char)n);
            return CommandVoidResult.Instance;
        }

        [Command("echo unicode characters from Console.Unicode (Codepage - 850)")]
        public CommandVoidResult UnicodeTest(
            CommandEvaluationContext context
        )
        {
            var t = typeof(Console.Unicode);
            foreach (var fi in t.GetFields())
            {
                context.Out.Echo(fi.GetValue(null) + " ");
            }
            context.Out.Echoln();
            return CommandVoidResult.Instance;
        }

        [Command("show current colors support and current colors map using ANSI escape codes")]
        public CommandVoidResult AnsiColorTest(
            CommandEvaluationContext context
            )
        {
            // 3 bits colors (standard)
            var colw = 8;
            var totw = colw * 8 + 3 + 10;
            var hsep = "".PadLeft(totw, '-');
            var esc = (char)27;
            string r;
            int x2 = 0;

            context.Out.Echoln("3 bits (8 color mode)");
            context.Out.Echoln();
            context.Out.Echoln("Background | Foreground colors");
            context.Out.Echoln(hsep);
            for (int j = 0; j <= 7; j++)
            {
                var str1 = $" ESC[4{j}m   | {esc}[4{j}m";
                var str2 = $" ESC[10{j}m  | {esc}[10{j}m";
                for (int i = 0; i <= 7; i++)
                {
                    str1 += Set4BitsColors(i, j | 0b1000) + $" [9{i}m   ";
                    str2 += Set4BitsColors(i | 0b1000, j) + $" [3{i}m   ";
                }

                context.Out.Echoln(str1 + context.ShellEnv.Colors.Default);
                context.Out.Echoln(str2 + context.ShellEnv.Colors.Default);
                context.Out.Echoln(hsep);
            }
            context.Out.Echoln(context.ShellEnv.Colors.Default + "");

            // 8 bits colors
            context.Out.Echoln("8 bits (256 color mode)");
            context.Out.Echoln();
            context.Out.Echoln("216 colors: 16 + 36 × r + 6 × g + b (0 <= r, g, b <= 5)(br)");
            int n = 16;
            for (int y = 0; y < 6; y++)
            {
                r = "";
                for (int x = 16; x <= 51; x++)
                {
                    if (x >= 34)
                        r += Black;
                    else
                        r += White;
                    r += $"{esc}[48;5;{n}m" + ((n + "").PadLeft(4, ' '));
                    n++;
                    x2++;
                    if (x2 >= 6) { r += Br; x2 = 0; }
                }
                context.Out.Echo(r);
            }

            context.Out.Echoln(context.ShellEnv.Colors.Default + "");
            context.Out.Echoln("grayscale colors (24 colors) : 232 + l (0 <= l <= 24)(br)");
            r = White;
            x2 = 0;
            for (int x = 232; x <= 255; x++)
            {
                if (x >= 244)
                    r += Black;
                r += $"{esc}[48;5;{x}m" + ((x + "").PadLeft(4, ' '));
                x2++;
                if (x2 >= 6) { r += context.Out.LNBRK; x2 = 0; }
            }
            context.Out.Echo(r);

            context.Out.Echoln(context.ShellEnv.Colors.Default + "");
            context.Out.Echoln("24 bits (16777216 colors): 0 <= r,g,b <= 255 (br) ");

            string cl(int r, int v, int b) =>
                esc + "[48;2;" + r + ";" + v + ";" + b + "m ";

            var stp = 4;
            r = "";
            int cr, cb = 0, cv = 0;
            for (cr = 0; cr < 255; cr += stp)
                r += cl(cr, cv, cb);
            context.Out.Echoln(r);

            r = "";
            cr = 0;
            for (cv = 0; cv < 255; cv += stp)
                r += cl(cr, cv, cb);
            context.Out.Echoln(r);

            cv = 0;
            r = "";
            for (cb = 0; cb < 255; cb += stp)
                r += cl(cr, cv, cb);
            context.Out.Echoln(r);

            r = "";
            for (cb = 0; cb < 255; cb += stp)
                r += cl(cb, cb, 0);
            context.Out.Echoln(r);

            r = "";
            for (cb = 0; cb < 255; cb += stp)
                r += cl(cb, 0, cb);
            context.Out.Echoln(r);

            r = "";
            for (cb = 0; cb < 255; cb += stp)
                r += cl(0, cb, cb);
            context.Out.Echoln(r);

            r = "";
            for (cb = 0; cb < 255; cb += stp)
                r += cl(cb, cb, cb);
            context.Out.Echoln(r);

            return new CommandVoidResult();
        }
    }
}

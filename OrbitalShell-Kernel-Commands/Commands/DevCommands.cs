using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using static OrbitalShell.Component.Console.ANSI;
using OrbitalShell.Component.Parser.ANSI;
using System;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.CommandLine;
using OrbitalShell.Commands.FileSystem;
using System.Collections.Generic;

namespace OrbitalShell.Commands.Dev
{
    [Commands("for shell development")]
    [CommandsNamespace(CommandNamespace.dev)]
    public class DevCommands : ICommandsDeclaringType
    {
        [Command("generate EchoDirectives from Console.Unicode (19/1/21)")]
        public CommandVoidResult GenDirectivesFromUnicode(CommandEvaluationContext context)
        {
            var t = typeof(Unicode);
            var echodirs = "";
            var textwrapper = "";
            var commentdoc = @"    
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; Unicode characters<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>";
            commentdoc += CRLF;
            foreach (var fi in t.GetFields())
            {
                commentdoc += $"    /// {fi.Name}<br/>{CRLF}";
                echodirs += $"        {fi.Name},{CRLF}";
                textwrapper += $"                {{ EchoDirectives.{fi.Name}+\"\" , (null,_Unicode,Unicode.{fi.Name}) }},{CRLF}";
            }
            context.Out.Echoln(textwrapper);
            context.Out.Echoln(echodirs);
            context.Out.Echoln(commentdoc);
            return CommandVoidResult.Instance;
        }

        [Command("ansi parser test (25/1/21)")]
        public CommandVoidResult EchoRawMeasureTest(CommandEvaluationContext context)
        {
            var str = ANSI.ESC + "[38;2;50;100;150m" + "HELLO(f=red)YOU"; // 5 printables cars

            context.Out.Echoln("parsed echo:");
            context.Out.Echoln(str);

            context.Out.Echoln("raw mode from echo:");
            context.Out.Echoln(str, true);

            string s;

            // good arg is: ignorePrintDirectives
            // ansi pass throught

            // ignorePrintDirectives = false , doNotEvaluatePrintPrimitives = false
            context.Out.Echo("text from echo printable chars only mode 1 (GetText,GetPrint defaults)=" + (s = context.Out.GetText(str)));
            context.Out.Echoln("(rdc)  length = " + s.Length);
            context.Out.Echoln("raw =" + s, true);

            // ignorePrintDirectives = false , doNotEvaluatePrintPrimitives = true
            context.Out.Echo("text from echo printable chars only mode 2=" + (s = context.Out.GetPrint(str, false, false, true)));
            context.Out.Echoln("(rdc)  length = " + s.Length);
            context.Out.Echoln("raw =" + s, true);

            // ignorePrintDirectives = true , doNotEvaluatePrintPrimitives = true
            context.Out.Echo("text from echo printable chars only mode 3=" + (s = context.Out.GetPrint(str, false, true, true)));
            context.Out.Echoln("(rdc)  length = " + s.Length);
            context.Out.Echoln("raw =" + s, true);

            // ignorePrintDirectives = true , doNotEvaluatePrintPrimitives = false
            context.Out.Echo("text from echo printable chars only mode 4=" + (s = context.Out.GetPrint(str, false, true, false)));
            context.Out.Echoln("(rdc)  length = " + s.Length);
            context.Out.Echoln("raw =" + s, true);

            return CommandVoidResult.Instance;
        }

        [Command("ansi parser test (22/1/21)")]
        public CommandVoidResult AnsiParseTest(CommandEvaluationContext context)
        {
            var str = ANSI.CSI + "1;2Z" + ANSI.RIS + ANSI.CHA(10) + "hello" + ANSI.CRLF + ANSI.SGRF24("50:100:150") + "WORLD(f=red) RED";
            //var str = ANSI.ESC;

            context.Out.Echoln(str, true);
            context.Out.Echoln();
            var paths = ANSIParser.Parse(str);
            foreach (var p in paths)
                context.Out.Echoln(ASCII.GetNonPrintablesCodesAsLabel(p.ToString(), false), true);

            context.Out.Echoln();
            context.Out.Echoln($"text={paths.GetText()}", true);
            context.Out.Echoln($"text length={paths.GetTextLength()}", true);

            return CommandVoidResult.Instance;
        }

        [Command("set a and b anc c variables to play with (22/1/21)")]
        public CommandVoidResult SetAbc(CommandEvaluationContext context)
        {
            context.Variables.Set(VariableNamespace.local, "a", "i am a");
            context.Variables.Set(VariableNamespace.local, "b", "i am b");
            context.Variables.Set(VariableNamespace.local, "c", "a");
            return CommandVoidResult.Instance;
        }

        [Command("command crash test (21/1/21)")]
        [CommandAlias("crash", "crash-test")]
        public CommandVoidResult CrashTest(CommandEvaluationContext context) => throw new Exception("command crash test (throws exception)");

        [Command("command with several option names types (short/long/both/none) (27/1/21)")]

        public CommandVoidResult ComOptNameTest(
            CommandEvaluationContext context,
            [Option(null, "long-name", "an option without a short name", true)] bool noShortNameOption,
            [Option("s", null, "an option without a long name", true)] bool noLongNameOption,
            [Option("b", "both", "an option with both", true)] bool withBothOption
            )
        {
            context.Out.Echo("noShortNameOption="); context.Out.Echo(noShortNameOption, true);
            context.Out.Echo("noLongNameOption="); context.Out.Echo(noLongNameOption, true);
            context.Out.Echo("withBothOption="); context.Out.Echo(withBothOption, true);
            return CommandVoidResult.Instance;
        }

        [Command("command with several option names types (short/long/both/none) and values (27/1/21)")]
        public CommandVoidResult ComOptNameWithValueTest(
            CommandEvaluationContext context,
            [Option(null, "long-name", "an option without a short name and a value", true, true)] int noShortNameOption = -32,
            [Option("s", null, "an option without a long name and a value", true, true)] object noLongNameOption = null,
            [Option("b", "both", "an option with both and a value", true, true)] string withBothOption = "a string"
            )
        {
            context.Out.Echo("noShortNameOption=");
            context.Out.Echo(noShortNameOption, true);
            context.Out.Echo("noLongNameOption=");
            context.Out.Echo(noLongNameOption, true);
            context.Out.Echo("withBothOption=");
            context.Out.Echo(withBothOption, true);
            return CommandVoidResult.Instance;
        }

        [Command("command with parameters type and value (27/1/21)")]
        public CommandVoidResult ComParamWithValueTest(
            CommandEvaluationContext context,
            [Parameter(0, "a parameter with a value", true)] object param
            )
        {
            context.Out.Echo("noShortNameOption="); context.Out.Echo(param, true);
            return CommandVoidResult.Instance;
        }

        [Command("command with collection parameters types (29/1/21)")]
        public CommandVoidResult ComParamColTest(
            CommandEvaluationContext context,
            [Option("A", "optiona", "option a", true, true)] FileSystemCommands.DirSort A,
            [Option("b", "b", "option b", true, true)] List<string> b,
            [Option("c", "c", "option c", true, true)] List<int> c,
            [Parameter(1, "x", true)] FileSystemCommands.DirSort B,
            [Parameter(2, "y", true)] List<FileSystemCommands.DirSort> C
            )
        {
            int i = 0;
            var v = Enum.GetValues(typeof(FileSystemCommands.DirSort));
            foreach (var s in Enum.GetNames(typeof(FileSystemCommands.DirSort)))
            {
                context.Out.Echo($"s={s} n={(int)v.GetValue(i)}", true);
                i++;
            }

            context.Out.Echo("A="); context.Out.Echo(A, true);
            context.Out.Echo("b=");
            context.Out.Echo(b, true);
            context.Out.Echo("c="); context.Out.Echo(c, true);
            context.Out.Echo("B="); context.Out.Echo(B, true);
            context.Out.Echo("C="); context.Out.Echo(C, true);
            return CommandVoidResult.Instance;
        }
    }
}

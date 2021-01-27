using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Console;
using static OrbitalShell.Console.ANSI;
using OrbitalShell.Component.Parser.ANSI;
using System;
using OrbitalShell.Component;
using OrbitalShell.Component.CommandLine.Variable;
using OrbitalShell.Component.CommandLine;

namespace OrbitalShell.Commands.Dev
{
    [Commands("for shell development")]
    [CommandsNamespace(CommandNamespace.dev)]
    public class DevCommands : ICommandsDeclaringType
    {
        [Command("generate EchoDirectives from Console.Unicode (19/1/21)")]
        public CommandVoidResult GenDirectivesFromUnicode(CommandEvaluationContext context)
        {
            var t = typeof(Console.Unicode);
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
        public CommandVoidResult CrashTest(CommandEvaluationContext context) => throw new Exception("command crash test (throws exception)");
    }
}

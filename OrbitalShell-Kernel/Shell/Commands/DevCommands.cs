using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Console;
using DotNetConsoleAppToolkit.Lib.FileSystem;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static DotNetConsoleAppToolkit.Console.ANSI;
using cons = DotNetConsoleAppToolkit.DotNetConsole;
using static DotNetConsoleAppToolkit.Lib.TextFileReader;
using sc = System.Console;
using static DotNetConsoleAppToolkit.Component.EchoDirective.Shortcuts;
using DotNetConsoleAppToolkit.Component.EchoDirective;
using DotNetConsoleAppToolkit.Component.Parser.ANSI;
using System;

namespace DotNetConsoleAppToolkit.Shell.Commands.Dev
{
    [Commands("for shell development")]
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
            commentdoc+=CRLF;
            foreach (var fi in  t.GetFields()) {
                commentdoc += $"    /// {fi.Name}<br/>{CRLF}";
                echodirs += $"        {fi.Name},{CRLF}";
                textwrapper += $"                {{ EchoDirectives.{fi.Name}+\"\" , (null,_Unicode,Unicode.{fi.Name}) }},{CRLF}";
            }
            context.Out.Echoln(textwrapper);
            context.Out.Echoln(echodirs);
            context.Out.Echoln(commentdoc);
            return CommandVoidResult.Instance;
        }

        [Command("ansi parser test (22/1/21)")]
        public CommandVoidResult Ansiparsetest(CommandEvaluationContext context) 
        {
            var str = ANSI.CSI+"1;2Z"+ANSI.RIS+ANSI.CHA(10)+"hello"+ANSI.CRLF+ANSI.SGRF24("50:100:150")+"WORLD(f=red) RED";
            //var str = ANSI.ESC;
            
            //context.Out.Echoln(str);
            context.Out.Echoln(str,true);
            context.Out.Echoln();
            var paths = ANSIParser.Parse(str);
            foreach ( var p in paths )
                context.Out.Echoln(ASCII.GetNonPrintablesCodesAsLabel( p.ToString() , false ),true);

            context.Out.Echoln();
            context.Out.Echoln($"text= {paths.GetText()}",true);

            return CommandVoidResult.Instance;
        }

        [Command("set a and b anc c variables to play with (22/1/21)")]
        public CommandVoidResult Setabc(CommandEvaluationContext context) 
        {
            context.Variables.Set("a","i am a");
            context.Variables.Set("b","i am b");
            context.Variables.Set("c","a");
            return CommandVoidResult.Instance;
        }

        [Command("command crash test (21/1/21)")]
        public CommandVoidResult CrashTest(CommandEvaluationContext context) => throw new Exception("command crash test (throws exception)");
    }
}

using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Console;
using DotNetConsoleAppToolkit.Lib.FileSystem;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static DotNetConsoleAppToolkit.Console.ANSI;
using static DotNetConsoleAppToolkit.DotNetConsole;
using static DotNetConsoleAppToolkit.Lib.TextFileReader;
using sc = System.Console;
using static DotNetConsoleAppToolkit.Component.EchoDirective.Shortcuts;
using DotNetConsoleAppToolkit.Component.EchoDirective;
using System;

namespace DotNetConsoleAppToolkit.Shell.Commands.Test
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
            Out.Echoln(textwrapper);
            Out.Echoln(echodirs);
            Out.Echoln(commentdoc);
            return CommandVoidResult.Instance;
        }

        [Command("command crash test (19/1/21)")]
        public CommandVoidResult CrashTest(CommandEvaluationContext context) => throw new Exception("command crash test (throws exception)");
    }
}

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
using OrbitalShell.Lib.FileSystem;
using System.IO;

namespace OrbitalShell.Commands.Tools
{
    [Commands("tools dedicated to development of tools built upon the shell")]
    [CommandsNamespace(CommandNamespace.tools,CommandNamespace.shell)]
    public class ToolsShellCommands : ICommandsDeclaringType
    {
        public const string moduleProjectTemplateRepositoryUrl = "https://github.com/OrbitalShell/OrbitalShell-Module-Template.git";

        string Title(string text)
        {
            var p = "".PadLeft(text.Length+2,' ');
            var spc = $"(b=darkgreen,f=black){p}(br)";
            var r = spc;
            r += $"(b=darkgreen,f=black) {text} (rdc)(br)";
            return r + spc;
        }

        string Action(string text, string info) => $"(br,f=green){text}: (b=darkgreen,f=yellow){info}(rdc)";

        string _(string text) => $"(f=green){text}(rdc)";

        string Input(CommandEvaluationContext context,string prompt)
        {
            context.Out.Echo(prompt);
            return context.In.ReadLine();
        }

        [Command("generates a new .net project for developing a shell module from the module project template","requires git command available in os")] 
        public CommandVoidResult NewModule( 
            CommandEvaluationContext context,
            [Parameter(0, "module ID")] string id,
            [Option("o","out","output path",true,true)] DirectoryPath output = null,
            [Option("f","force","delete target if already exists")] bool force = false,
            [Option("project template url", "modproj-tpl-url", "module project template url", true, true)] string url = moduleProjectTemplateRepositoryUrl
            )
        {
            var o = context.Out;
            var c = context.ShellEnv.Colors;
            output ??= new DirectoryPath(Environment.CurrentDirectory);
            output = new DirectoryPath(Path.Combine(output.FullName, id));
            var ectx = new EchoEvaluationContext(context);

            o.Echo(Title("shell module dotnet project generator"));

            if (force && output.CheckExists()) Directory.Delete(output.FullName, true);
            //if (!output.CheckExists()) Directory.CreateDirectory(output.FullName);            
            //if (!output.CheckExists(context)) return CommandVoidResult.Instance;            

            o.Echoln(Action("cloning module project repository",url));
            o.Echo(_("into: ")); 
            output.Echo(ectx); o.Echoln();

            o.Echoln($"(br){c.Information}> git clone {url} {output.FullName}");

            var r0 = context.CommandLineProcessor.ShellExec(
                context,
                "git",
                $"clone {url} {output.FullName}",
                out var cmd0Out);

            if (r0 != (int)ReturnCode.OK)
                return new CommandVoidResult(r0, cmd0Out);

            return CommandVoidResult.Instance;
        }
    }
}

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
using Newtonsoft.Json;

namespace OrbitalShell.Commands.Tools.Shell
{
    [Commands("tools dedicated to development of tools built upon the shell")]
    [CommandsNamespace(CommandNamespace.tools,CommandNamespace.shell)]
    public class ToolsShellCommands : ICommandsDeclaringType
    {
        public const string moduleProjectTemplateRepositoryUrl = "https://github.com/OrbitalShell/OrbitalShell-Module-Template.git";
        public const string defaultSettingsFileName = "module-settings.json";

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
            [Option("i","in","take parameters from json input file",true,true)] string inputFile = defaultSettingsFileName,
            [Option("f","force","delete target if already exists")] bool force = false,
            [Option("s","skip-errors","skip ant error if possible")] bool skipErrors = false,
            [Option("project template url", "modproj-tpl-url", "module project template url", true, true)] string url = moduleProjectTemplateRepositoryUrl
            )
        {
            var o = context.Out;
            var c = context.ShellEnv.Colors;
            output ??= new DirectoryPath(Environment.CurrentDirectory);
            output = new DirectoryPath(Path.Combine(output.FullName, id));
            var input = new FilePath(inputFile);
            var ectx = new EchoEvaluationContext(context);

            o.Echo(Title("shell module dotnet project generator"));

            if (!input.CheckExists(context)) return new CommandVoidResult(ReturnCode.Error);
            if (force && output.CheckExists()) Directory.Delete(output.FullName, true);
            
            o.Echoln(Action("cloning module project repository",url));
            o.Echo(_("into: ")); 
            output.Echo(ectx); o.Echoln();

            o.Echoln($"(br){c.Information}> git clone {url} {output.FullName}");

            var r0 = context.CommandLineProcessor.ShellExec(
                context,
                "git",
                $"clone {url} {output.FullName}",
                out var cmd0Out);

            if (!skipErrors && r0 != (int)ReturnCode.OK)
                return new CommandVoidResult(r0, cmd0Out);

            o.Echoln(Action($"set parameters in new module ", $"'{id}'"));

            var settingsText = File.ReadAllText(input.FullName);
            var settings = JsonConvert.DeserializeObject<ModuleSettings>(settingsText);

            /*              
            cd MyModule
            git remote remove origin
            git remote add origin {repositoryUrl}
             */

            return CommandVoidResult.Instance;
        }
    }
}

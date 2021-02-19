using System;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell;
using OrbitalShell.Lib.Data;
using OrbitalShell.Lib.FileSystem;

using fs = OrbitalShell.Lib.FileSystem.FileSystem;

namespace OrbitalShell.Commands.Tools.Shell
{
    [Commands("tools dedicated to development of tools built upon the shell")]
    [CommandsNamespace(CommandNamespace.tools,CommandNamespace.shell)]
    public class ToolsShellCommands : ICommandsDeclaringType
    {
        public const string moduleProjectTemplateRepositoryUrl = "https://github.com/OrbitalShell/OrbitalShell-Module-Template.git";
        public const string defaultSettingsFileName = "module-settings.json";

        string _title(string text)
        {
            var p = "".PadLeft(text.Length+2,' ');
            var spc = $"(b=darkgreen,f=black){p}(br)";
            var r = spc;
            r += $"(b=darkgreen,f=black) {text} (rdc)(br)";
            return r + spc;
        }

        string _subTitle(string text, string info) => $"(br,f=green){text}: (b=darkgreen,f=yellow){info}(rdc)";

        string _(string text) => $"(f=green){text}(rdc)";

        string Input(CommandEvaluationContext context,string prompt)
        {
            context.Out.Echo(prompt);
            return context.In.ReadLine();
        }

        [Command("generates a new .net project for developing a shell module from the module project template","requires git command available in os")] 
        [RequireOSCommand("git")]
        [RequireOSCommand("dotnet")]
        public CommandVoidResult NewModule( 
            CommandEvaluationContext context,
            [Parameter(0, "module ID")] string id,
            [Parameter(1, "module project repository url (if not set do net set project repository properties, thus the project repo is not connected to any remote repository)",true)] string repoUrl = null,
            [Option("o","out","output path",true,true)] DirectoryPath output = null,
            [Option("i","in","take parameters from json input file",true,true)] string inputFile = defaultSettingsFileName,
            [Option("f","force","delete target if already exists")] bool force = false,
            [Option("s","skip-errors","skip ant error if possible")] bool skipErrors = false,
            [Option(null, "no-init", "don't perform an initial init of remote repo")] bool noInitRemote = false,
            [Option("project template url", "modproj-tpl-url", "module project template url", true, true)] string url = moduleProjectTemplateRepositoryUrl,
            [Option(null,"preserve-curdir","preserve current dir")] bool preserveCurrentDir = false
            )
        {
            var o = context.Out;
            var c = context.ShellEnv.Colors;
            string packageId = !string.IsNullOrWhiteSpace(repoUrl) ?
                Path.GetFileNameWithoutExtension(repoUrl) : null;
            string repoOwner = !string.IsNullOrWhiteSpace(repoUrl) ?
                Path.GetFileName(Path.GetDirectoryName(repoUrl)) : null;
            output ??= new DirectoryPath(Environment.CurrentDirectory);
            output = new DirectoryPath(Path.Combine(output.FullName, packageId ?? id));
            var input = new FilePath(inputFile);
            var ectx = new EchoEvaluationContext(context);
            ectx.Options.LineBreak = true;
            CommandVoidResult result = CommandVoidResult.Instance;

            o.Echo(_title("shell module dotnet project generator"));

            #region download_pattern_project

            if (force && output.CheckExists()) Directory.Delete(output.FullName, true);
            
            o.Echoln(_subTitle("cloning module project repository",url));
            o.Echo(_("into: ")); output.Echo(ectx); o.Echoln("(br)");
           
            if (!ShellExec(context, skipErrors, "git", $"clone {url} {output.FullName}", out result)) return result;

            #endregion

            #region setup project template properties

            if (!input.CheckExists(context)) return new CommandVoidResult(ReturnCode.Error);
            o.Echoln(_subTitle($"set project template properties", $"'{id}'"));

            

            var settingsText = File.ReadAllText(input.FullName);
            var settings = JsonConvert
                .DeserializeObject<ModuleSettings>(settingsText)
                .AutoFill(id,packageId);
            settings.ModuleRepositoryUrl = repoUrl;
            settings.ModuleRepositoryOwner = repoOwner;

            o.Echoln(_subTitle("using settings", input.FullName));
            settings.Echo(ectx); o.Echoln("(br)");

            var items = fs.FindItems(
                context,
                output.FullName,
                "*",
                false,
                true,
                false,
                true,
                false,
                null,
                false,
                new FindCounts(),
                false);

            var fields = typeof(ModuleSettings).GetFields();
            foreach ( var item in items )
            {
                var path = item.FullName;
                if (item.IsFile && fs.IsTextFile(path))
                {
                    var tpl = path;
                    _templateReplace(fields, settings, ref tpl);
                    
                    var txt = File.ReadAllText(path);

                    if (tpl != path)
                    {
                        File.Delete(path);
                        path = tpl;
                    }

                    tpl = txt+"";
                    _templateReplace(fields, settings, ref tpl);
                    if (tpl!=txt)
                        File.WriteAllText(path, tpl);                    
                }
            }

            void _deleteIfExists(string n) { if (File.Exists(n)) File.Delete(n); }

            _deleteIfExists(Path.Combine(output.FullName,"templateInfo.txt"));
            _deleteIfExists(Path.Combine(output.FullName, "./module-settings.json"));

            #endregion

            #region setup project repository

            o.Echoln(_subTitle("setup project repository", url)); o.Echoln();   

            var curDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = output.FullName;

            if (!ShellExec(context, skipErrors, "git", "remote remove origin", out result)) return result;

            if (!string.IsNullOrWhiteSpace(settings.ModuleRepositoryUrl))
            {
                if (!ShellExec(context, skipErrors, "git", $"remote add origin {settings.ModuleRepositoryUrl}", out result)) return result;
                // check this
                //if (!noPush && !ShellExec(context, skipErrors, "git", "push --set-upstream origin main", out result)) return result;
                if (!noInitRemote && !ShellExec(context, skipErrors, "git", "fetch", out result)) return result;
                if (!noInitRemote && !ShellExec(context, skipErrors, "git", "branch --set-upstream-to=origin/main main", out result)) return result;
                if (!noInitRemote && !ShellExec(context, skipErrors, "git", "pull --allow-unrelated-histories", out result)) return result;
                if (!noInitRemote && !ShellExec(context, skipErrors, "git", "add .", out result)) return result;
                if (!noInitRemote && !ShellExec(context, skipErrors, "git", "commit -a -m \"initial commit\"", out result)) return result;
                if (!noInitRemote && !ShellExec(context, skipErrors, "git", "push", out result)) return result;
            }

            #endregion

            #region restore & build project

            o.Echoln(_subTitle("restore & build module project", "")); o.Echoln();

            if (!ShellExec(context, skipErrors, "dotnet", "build", out result)) return result;

            #endregion

            if (preserveCurrentDir) Environment.CurrentDirectory = curDir;

            o.Echoln(_subTitle($"module project has been generated",id));

            return result ?? CommandVoidResult.Instance;
        }

        void _templateReplace(FieldInfo[] fields,ModuleSettings settings,ref string tpl)
        {
            foreach (var field in fields)
            {
                var propn = $"{{{field.Name}}}";
                tpl = tpl.Replace(propn, field.GetValue(settings) + "");
            }
        }

        bool ShellExec(
           CommandEvaluationContext context,
           bool skipErrors,
           string com,
           string args,
           out CommandVoidResult returnCommandResult)
        {
            context.Out.Echoln($"{context.ShellEnv.Colors.Information}> {com} {args}(br)");
            var @return = skipErrors
                | context.CommandLineProcessor
                    .ShellExec(context, com, args, out CommandVoidResult result);
            returnCommandResult = skipErrors? null : result;
            return @return;
        }

    }
}


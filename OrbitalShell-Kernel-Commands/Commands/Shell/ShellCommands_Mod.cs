using OrbitalShell.Component.CommandLine;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Lib.FileSystem;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.Shell;
using System.IO;
using OrbitalShell.Lib;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Component.Console;
using System;
using System.Net.Http;
using System.Net;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Help.cs
    /// </summary>
    public partial class ShellCommands
    {
        [Command("list modules if no option specified, else load or unload modules")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.module)]
        [CommandAlias("mod", "module")]
        [CommandAlias("mods", "module -s")]
        public CommandResult<List<ModuleSpecification>> Module(
            CommandEvaluationContext context,
            [Option("l", "load", "load a module from the given path", true, true)] FilePath loadModulePath = null,
            [Option("u", "unload", "unload the module having the given name ", true, true)] string unloadModuleName = null,
            [Option("i", "install", "install a module from the nuget source", true, true)] string installModuleName = null,
            [Option("f", "fetch", "fetch list of modules from modules repositories", true)] bool getList = false,
            [Option("v", "version", "version to be installed in case of", true, true)] string version = null,
            [Option("s", "short", "output less informations", true)] bool @short = false
            )
        {
            var f = context.ShellEnv.Colors.Default.ToString();
            if (loadModulePath == null && unloadModuleName == null && !getList)
            {
                var col1length = context.CommandLineProcessor.ModuleManager.Modules.Values.Select(x => x.Name.Length).Max() + 1;
                int n = 1;
                foreach (var kvp in context.CommandLineProcessor.ModuleManager.Modules)
                {
                    var ver = kvp.Value.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                    var af = new FileInfo(kvp.Value.Assembly.Location);
                    var dat = af.CreationTimeUtc.ToString() + " UTC";
                    var comp = kvp.Value.Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
                    var aut_attr = kvp.Value.Assembly.GetCustomAttribute<ModuleAuthorsAttribute>();
                    var aut = (aut_attr == null) ? "" : string.Join(",", aut_attr.Auhors);
                    var target = kvp.Value.Assembly.GetCustomAttribute<ModuleTargetPlateformAttribute>()?.TargetPlateform;
                    target ??= TargetPlatform.Unspecified;
                    var deps_attr = kvp.Value.Assembly.GetCustomAttributes<ModuleDependencyAttribute>();
                    var deps = (deps_attr.Count() == 0) ? "" : string.Join(",", deps_attr.Select(x => x.ModuleName + " " + x.ModuleMinVersion));
                    var sminv = kvp.Value.Assembly.GetCustomAttribute<ModuleShellMinVersionAttribute>()?.ShellMinVersion;

                    context.Out.Echoln($"{Darkcyan}{kvp.Value.Name.PadRight(col1length, ' ')}{f}{kvp.Value.Description}");

                    if (!@short)
                    {
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}assembly     : (rdc){kvp.Value.Assembly.FullName}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}path         : (rdc){kvp.Value.Assembly.Location}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}version      : (rdc){ver}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}date         : (rdc){dat}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}target       : (rdc){target}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}dependencies : (rdc){deps}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}shell min ver: (rdc){sminv}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}company      : (rdc){comp}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}authors      : (rdc){aut}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{kvp.Value.Info.GetDescriptor(context)}");
                    }
                    else
                    {
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}version : {context.ShellEnv.Colors.HalfDark}{ver} (target {((target == null) ? "" : target + ", ")}created {dat}) {context.ShellEnv.Colors.Label}Company : (rdc){comp} ({aut}) ");
                    }
                    if (n < context.CommandLineProcessor.ModuleManager.Modules.Count) context.Out.Echoln();
                    n++;
                }
                return new CommandResult<List<ModuleSpecification>>(context.CommandLineProcessor.ModuleManager.Modules.Values.ToList());
            }

            ModuleSpecification moduleSpecification = null;

            if (!getList)
            {
                // load/init module
                if (loadModulePath != null)
                {
                    if (loadModulePath.CheckExists(context))
                    {
                        var a = Assembly.LoadFrom(loadModulePath.FileSystemInfo.FullName);
                        moduleSpecification = context.CommandLineProcessor.ModuleManager.RegisterModule(context, a);
                        if (moduleSpecification != null && moduleSpecification.Info != null) context.Out.Echoln($" Done : {moduleSpecification.Info.GetDescriptor(context)}");
                    }
                    else
                        return new CommandResult<List<ModuleSpecification>>(ReturnCode.Error);
                }

                // unload module
                if (unloadModuleName != null)
                {
                    if (context.CommandLineProcessor.ModuleManager.Modules.Values.Any(x => x.Name == unloadModuleName))
                    {
                        moduleSpecification = context.CommandLineProcessor.ModuleManager.UnregisterModule(context, unloadModuleName);
                        if (moduleSpecification != null && moduleSpecification.Info != null) context.Out.Echoln($"unloaded: {moduleSpecification.Info.GetDescriptor(context)}");
                    }
                    else
                    {
                        context.Errorln($"module '{unloadModuleName}' is not registered");
                        return new CommandResult<List<ModuleSpecification>>(ReturnCode.Error);
                    }
                }
            }
            else
            {
                // get list
                var repoList = context.ShellEnv.GetValue<List<string>>(ShellEnvironmentVar.settings_module_providerUrls);
                if (repoList == null || repoList.Count == 0)
                {
                    context.Out.Errorln("module repository list is empty");
                }
                else
                {
                    context.Out.Echo(context.ShellEnv.Colors.Log + " fetching data from repositories ... ");
                    foreach (var repo in repoList)
                    {
                        try
                        {
                            var modLists = new List<string>();
                            using (var httpClient = new HttpClient())
                            {
                                using (var request = new HttpRequestMessage(new HttpMethod("GET"), repo))
                                {
                                    var tsk = httpClient.SendAsync(request);
                                    var result = tsk.Result;
                                    if (result.IsSuccessStatusCode)
                                    {
                                        modLists.Add(result.Content.ReadAsStringAsync().Result);
                                    }
                                    else
                                        context.Out.Errorln($"get fail from '{repo}' due to error: {result.ReasonPhrase}");
                                }
                            }
                            context.Out.Echo(context.ShellEnv.Colors.Log + "Done");
                        }
                        catch (Exception repoAccessError)
                        {
                            context.Out.Errorln($"get fail from '{repo}' due to error: {repoAccessError.Message}");
                        }
                    }
                }
            }

            if (moduleSpecification != null)
                return new CommandResult<List<ModuleSpecification>>(new List<ModuleSpecification> { moduleSpecification
    });
            else
                return new CommandResult<List<ModuleSpecification>>(new List<ModuleSpecification> { });
        }

    }
}

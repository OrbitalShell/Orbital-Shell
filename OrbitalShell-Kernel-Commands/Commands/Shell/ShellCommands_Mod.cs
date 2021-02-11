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
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Commands.NuGetServerApi;
using System.IO.Compression;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Help.cs
    /// </summary>
    public partial class ShellCommands
    {
        [Command("otuput a report of loaded modules if no option is specified, else allows to load/unload/install/remove/update modules and get informations from repositories of modules")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.module)]
        [CommandAlias("mod", "module")]
        [CommandAlias("mods", "module -s")]
        public CommandResult<List<ModuleSpecification>> Module(
            CommandEvaluationContext context,
            [Option("l", "load", "load a module from the given path", true, true)] FilePath loadModulePath = null,
            [Option("n", "unload", "unload the module having the given name ", true, true)] string unloadModuleName = null,
            [Option("i", "install", "install a module from the nuget source", true, true)] string installModuleName = null,
            [Option("r", "remove", "uninstall a module and remove the module files", true, true)] string uninstallModuleName = null,
            [Option("u", "update", "try to update an installed module from the nuget source", true, true)] string updateModuleName = null,
            [Option("f", "fetch-list", "fetch list of modules from modules repositories", true)] bool getList = false,
            [Option("o", "fetch-info", "query modules repositories about a module name, if found fetch the module and output informations about it. the module is not installed", true, true)] string fetchInfoName = null,
            [Option("v", "version", "version to be installed if applyable", true, true)] string version = null,
            [Option("s", "short", "output less informations", true)] bool @short = false
            )
        {
            ModuleSpecification moduleSpecification = null;
            var f = context.ShellEnv.Colors.Default.ToString();
            bool fetchInfo = fetchInfoName != null;

            if (loadModulePath == null && unloadModuleName == null && updateModuleName == null && installModuleName == null && uninstallModuleName == null
                && !getList && !fetchInfo)
            {
                // output reports on loaded modules

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

            if (!getList && !fetchInfo)
            {
                // install module

                if (installModuleName != null)
                {
                    var lastVer = string.IsNullOrWhiteSpace(version);
                    var getVersMethod = typeof(NuGetServerApiCommands).GetMethod("NugetVer");                    
                    var r = context.CommandLineProcessor.Eval(context, getVersMethod, $"{installModuleName}", 0);
                    if (r.EvalResultCode==(int)ReturnCode.OK)
                    {
                        var vers = (PackageVersions)r.Result;
                        if (!lastVer && !vers.Versions.Contains(version))
                        {
                            context.Errorln($"module version not found");
                            return _ModuleErr();
                        }
                        if (lastVer)
                        {
                            version = vers.Versions.Last();
                            context.Out.Echoln($"{context.ShellEnv.Colors.Log}select last version: {version}(rdc)");
                        }
                        var dwnMethod = typeof(NuGetServerApiCommands).GetMethod("NugetDownload");
                        var output = context.CommandLineProcessor.Settings.ModulesFolderPath;
                        var folderName = $"{installModuleName.ToLower()}.{version.ToLower()}";
                        var moduleFolder = Path.Combine( output, folderName );

                        if (!Directory.Exists(moduleFolder)) Directory.CreateDirectory(moduleFolder);

                        moduleFolder = FileSystemPath.UnescapePathSeparators(moduleFolder);
                        var rd = context.CommandLineProcessor.Eval(context, dwnMethod, $"{installModuleName} {version} -o {moduleFolder}", 0);
                        if (rd.EvalResultCode==(int)ReturnCode.OK)
                        {
                            context.Out.Echo(context.ShellEnv.Colors.Log + $"extracting package... ");

                            var nupkgFileName = (string)rd.Result;
                            ZipFile.ExtractToDirectory(Path.Combine(moduleFolder, nupkgFileName),moduleFolder,true);

                            context.Out.Echoln(" Done(rdc)");
                            context.Out.Echo(ANSI.RSTXTA + ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo

                            context.Out.Echoln("module installed");

                        } else 
                            return _ModuleErr();
                    } 
                    else 
                        return _ModuleErr();
                }

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
                        return _ModuleErr();
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
                        return _ModuleErr();
                    }
                }
            }
            else
            {
                // fetch mod repos
                if (_GetModuleListFromRepositories(context, out var modRefs))
                {
                    context.Out.Echo(ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo

                    if (modRefs.Count > 0)
                    {
                        // fetch module info

                        if (fetchInfo)
                        {
                            var modRef = modRefs.Where(x => x.Name == fetchInfoName).FirstOrDefault();
                            if (modRef != null)
                            {
                                // try to fetch the module : name[,version]->nuget

                            }
                            else
                                context.Out.Errorln("no module having name '{fetchInfoName}' can be found in repostories");
                        }

                        if (getList)
                        {
                            // get list

                            var tb = new Table(
                                ("name", typeof(string)),
                                ("version", typeof(ModuleVersion)),
                                ("description", typeof(string))
                            );
                            foreach (var modref in modRefs)
                                tb.AddRow(modref.Name, modref.Version, modref.Description);

                            tb.Echo(new EchoEvaluationContext(
                                context.Out,
                                context,
                                new TableFormattingOptions(
                                    context.ShellEnv.GetValue<TableFormattingOptions>(
                                        ShellEnvironmentVar.display_tableFormattingOptions))
                                { }));
                        }
                    }
                    else
                        context.Out.Errorln("module repository list doesn't contains any module reference");
                }
            }

            if (moduleSpecification != null)
                return new CommandResult<List<ModuleSpecification>>(new List<ModuleSpecification> { moduleSpecification });
            else
                return new CommandResult<List<ModuleSpecification>>(new List<ModuleSpecification> { });
        }

        CommandResult<List<ModuleSpecification>> _ModuleErr() => new CommandResult<List<ModuleSpecification>>(ReturnCode.Error);

        #region util

        bool _GetModuleListFromRepositories(CommandEvaluationContext context, out List<ModuleReference> modulesReferences)
        {
            modulesReferences = null;

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
                                    context.Out.Errorln($"fetch list fail from '{repo}' due to error: {result.ReasonPhrase}");
                            }
                        }
                        context.Out.Echoln(context.ShellEnv.Colors.Log + "Done");

                        var modRefs = new List<ModuleReference>();
                        foreach (var modList in modLists)
                            modRefs.AddRange(_ParseModuleList(modList));

                        modulesReferences = modRefs;
                        return true;
                    }
                    catch (Exception repoAccessError)
                    {
                        context.Out.Errorln($"fetch list fail from '{repo}' due to error: {repoAccessError.Message}");
                    }
                }
            }
            return false;
        }

        List<ModuleReference> _ParseModuleList(string modList)
        {
            var r = new List<ModuleReference>();
            var lines = modList.Split("\n");
            foreach (var s in lines)
            {
                var ts = s.Trim();
                if (!string.IsNullOrWhiteSpace(s) && !s.StartsWith(";"))
                {
                    var t = s.Split(";");
                    if (t.Length == 4)
                    {
                        var itemType = t[0].ToLower();
                        if (itemType == "m")
                        {
                            var name = t[1];
                            var desc = t[3];
                            var version = t[2];
                            var modref = new ModuleReference(name, version, desc);
                            r.Add(modref);
                        }
                    }
                }
            }
            return r;
        }

        #endregion
    }
}

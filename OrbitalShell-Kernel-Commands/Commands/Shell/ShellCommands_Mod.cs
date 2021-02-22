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
using OrbitalShell.Commands.FileSystem;
using OrbitalShell.Lib.Data;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Help.cs
    /// </summary>
    public partial class ShellCommands
    {
        [Command("output a report of loaded modules if no option is specified, else allows to load/unload/install/remove/update modules and get informations from repositories of modules")]
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
            [Option("f", "fetch-list", "fetch list of modules from modules repositories", true)] bool fetchList = false,
            [Option("o", "fetch-info", "query modules repositories about a module name, if found fetch the module and output informations about it. the module is not installed", true, true)] string fetchInfoName = null,
            [Option("v", "version", "module version if applyable", true, true)] string version = null,
            [Option("s", "short", "output less informations", true)] bool @short = false,
            [Option(null,"skip-load" , "do not load the module after having installed it",true)] bool skipLoad = false,
            [Option(null,"force","perform the requested operation even if already done, in case of it is meaningfull (example: -i --force constraint the command to reinstall a module)")] bool force = false
            )
        {
            ModuleSpecification moduleSpecification = null;
            var f = context.ShellEnv.Colors.Default.ToString();
            bool fetchInfo = fetchInfoName != null;
            var clog = context.ShellEnv.Colors.Log;
            var o = context.Out;

            if (loadModulePath == null && unloadModuleName == null && updateModuleName == null && installModuleName == null && uninstallModuleName == null
                && !fetchList && !fetchInfo)
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

                    o.Echoln($"{Darkcyan}{kvp.Value.Name.PadRight(col1length, ' ')}{f}{kvp.Value.Description}");

                    if (!@short)
                    {
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}assembly     : (rdc){kvp.Value.Assembly.FullName}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}path         : (rdc){kvp.Value.Assembly.Location}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}version      : (rdc){ver}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}date         : (rdc){dat}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}target       : (rdc){target}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}dependencies : (rdc){deps}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}shell min ver: (rdc){sminv}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}company      : (rdc){comp}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}authors      : (rdc){aut}");
                        o.Echoln($"{"".PadRight(col1length, ' ')}{kvp.Value.Info.GetDescriptor(context)}");
                    }
                    else
                    {
                        o.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}version : {context.ShellEnv.Colors.HalfDark}{ver} (target {((target == null) ? "" : target + ", ")}created {dat}) {context.ShellEnv.Colors.Label}Company : (rdc){comp} ({aut}) ");
                    }
                    if (n < context.CommandLineProcessor.ModuleManager.Modules.Count) o.Echoln();
                    n++;
                }
                return new CommandResult<List<ModuleSpecification>>(context.CommandLineProcessor.ModuleManager.Modules.Values.ToList());
            }

            if (!fetchList && !fetchInfo)
            {
                // install module

                if (installModuleName != null)
                {
                    var lastVer = string.IsNullOrWhiteSpace(version);

                    var queryMethod = typeof(NuGetServerApiCommands).GetMethod("NugetQuery");
                    var r0 = context.CommandLineProcessor.Eval(context, queryMethod, $"{installModuleName} -t 1",0);
                    if (r0.EvalResultCode!=(int)ReturnCode.OK) return _ModuleErr(context, r0.ErrorReason);
                    var queryRes = r0.Result as QueryResultRoot;
                    if (queryRes==null) return _ModuleErr(context, "nuget query return a null result");
                    if (queryRes.Data.Length==0) return _ModuleErr(context, "module id unknown");

                    var packageId = queryRes.Data[0].Id;
                    o.Echoln();

                    var getVersMethod = typeof(NuGetServerApiCommands).GetMethod("NugetVer");                    
                    var r = context.CommandLineProcessor.Eval(context, getVersMethod, $"{installModuleName}", 0);
                    if (r.EvalResultCode==(int)ReturnCode.OK)
                    {
                        var vers = (PackageVersions)r.Result;

                        if (!lastVer && !vers.Versions.Contains(version))                        
                            return _ModuleErr(context,$"module version '{version}' not found");
                        
                        if (lastVer)
                        {
                            version = vers.Versions.Last();
                            o.Echoln($"{clog}select the last version of package: {version}(rdc)");
                        }
                        var dwnMethod = typeof(NuGetServerApiCommands).GetMethod("NugetDownload");
                        var output = context.CommandLineProcessor.Settings.ModulesFolderPath;
                        var folderName = packageId.ToLower();
                        var lowerVersion = version.ToLower();
                        var moduleLowerFullId = $"{folderName}.{lowerVersion}";  // == module lower id

                        if (context.CommandLineProcessor.ModuleManager.IsModuleInstalled(
                            context,folderName,version
                            ) && !force)
                            // error already installed, !force
                            return _ModuleErr(context,$"module '{moduleLowerFullId}' is already installed (may try --force)");

                        var moduleFolder = Path.Combine( output, folderName );
                        if (!Directory.Exists(moduleFolder)) Directory.CreateDirectory(moduleFolder);

                        moduleFolder = FileSystemPath.UnescapePathSeparators(moduleFolder);
                        var rd = context.CommandLineProcessor.Eval(context, dwnMethod, $"{installModuleName} {version} -o {moduleFolder}", 0);
                        if (rd.EvalResultCode==(int)ReturnCode.OK)
                        {
                            o.Echo(clog + "extracting package... ");

                            var versionFolder = Path.Combine(moduleFolder, lowerVersion);
                            if (!Directory.Exists(versionFolder)) Directory.CreateDirectory(versionFolder);

                            var nupkgFileName = (string)rd.Result;
                            ZipFile.ExtractToDirectory(Path.Combine(moduleFolder, nupkgFileName),versionFolder,true);

                            o.Echoln(" Done(rdc)");
                            o.Echo(ANSI.RSTXTA + ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo

                            // find modules dlls : find {folderName}/{lowerVersion}/lib -p *.dll
                            o.Echoln(clog + "scanning package... ");
                            var findMethod = typeof(FileSystemCommands).GetMethod("Find");
                            var find = context.CommandLineProcessor.Eval(context, findMethod, $"{moduleFolder}/{lowerVersion}/lib -p *.dll", 0);
                            var nodllmess = "the module doesn't contain any dll in /lib";
                            if (find.EvalResultCode != (int)ReturnCode.OK) return _ModuleErr(context, nodllmess);

                            var findResult = ((List<FileSystemPath>, FindCounts))find.Result;
                            var nbImport = 0;
                            foreach ( var dll in findResult.Item1 )
                            {
                                if (!context.CommandLineProcessor.ModuleManager.IsModuleAssemblyLoaded(dll.FullName))
                                {
                                    // candidate assembly
                                    o.Echoln(clog + $"importing dll: '{dll.Name}'");
                                    var assembly = Assembly.LoadFrom(dll.FullName);
                                    if (ModuleUtil.IsAssemblyShellModule(assembly))
                                    {
                                        try
                                        {
                                            if (!skipLoad)
                                                context.CommandLineProcessor.ModuleManager.RegisterModule(context, assembly);

                                            o.Echoln(clog + $"register into module-init");
                                            var modInit = ModuleUtil.LoadModuleInitConfiguration(context);

                                            var exMod = modInit.List.Where(x => x.Path == dll.FullName);
                                            if (exMod.Count() > 0) throw new Exception("an module assembly with the same path is altready registered in module-init");

                                            var mod = new ModuleInitItemModel()
                                            {
                                                Path = FileSystemPath.UnescapePathSeparators(dll.FullName),
                                                IsEnabled = true
                                            };
                                            modInit.List = modInit.List.Append(mod).ToArray();
                                            ModuleUtil.SaveModuleInitConfiguration(context, modInit);
                                            nbImport++;
                                        }
                                        catch (Exception ex) { o.Errorln(ex.Message); }
                                    }
                                }
                                else
                                    o.Errorln($"can't import the dll: '{dll.Name}' because it is in loaded state");
                            }

                            // end
                            if (nbImport > 0)
                                o.Echoln("module installed");
                            else
                                o.Errorln("no module assembly found in package");

                        } else 
                            return _ModuleErr(context,rd.ErrorReason);
                    } 
                    else 
                        return _ModuleErr(context,"module id is required");
                }

                // load/init module

                if (loadModulePath != null)
                {
                    if (loadModulePath.CheckExists(context))
                    {
                        var a = Assembly.LoadFrom(loadModulePath.FileSystemInfo.FullName);
                        moduleSpecification = context.CommandLineProcessor.ModuleManager.RegisterModule(context, a);
                        if (moduleSpecification != null && moduleSpecification.Info != null) o.Echoln($" Done : {moduleSpecification.Info.GetDescriptor(context)}");
                    }
                    else
                        return _ModuleErr(null,null);
                }

                // unload module

                if (unloadModuleName != null)
                {
                    if (context.CommandLineProcessor.ModuleManager.Modules.Values.Any(x => x.Name == unloadModuleName))
                    {
                        moduleSpecification = context.CommandLineProcessor.ModuleManager.UnregisterModule(context, unloadModuleName);
                        if (moduleSpecification != null && moduleSpecification.Info != null) o.Echoln($"unloaded: {moduleSpecification.Info.GetDescriptor(context)}");
                    }
                    else
                        return _ModuleErr(context, $"module '{unloadModuleName}' is not registered");
                }
            }
            else
            {
                // fetch mod repos
                if (_GetModuleListFromRepositories(context, out var modRefs))
                {
                    o.Echo(ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo

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
                                o.Errorln($"no module having name '{fetchInfoName}' can be found in repostories");
                        }

                        if (fetchList)
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
                                o,
                                context,
                                new TableFormattingOptions(
                                    context.ShellEnv.GetValue<TableFormattingOptions>(
                                        ShellEnvironmentVar.display_tableFormattingOptions))
                                { }));
                        }
                    }
                    else
                        o.Errorln("module repository list doesn't contains any module reference");
                }
            }

            if (moduleSpecification != null)
                return new CommandResult<List<ModuleSpecification>>(new List<ModuleSpecification> { moduleSpecification });
            else
                return new CommandResult<List<ModuleSpecification>>(new List<ModuleSpecification> { });
        }

        CommandResult<List<ModuleSpecification>> _ModuleErr(CommandEvaluationContext context,string reason)
        {
            context?.Errorln(reason);
            return new CommandResult<List<ModuleSpecification>>(ReturnCode.Error, reason);
        }

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

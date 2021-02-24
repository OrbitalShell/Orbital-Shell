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
using System.Runtime.Loader;
using Newtonsoft.Json;
using OrbitalShell.Component.Shell.Module.Data;

namespace OrbitalShell.Commands.Shell
{
    /// <summary>
    /// ShellCommands_Mod.cs
    /// </summary>
    public partial class ShellCommands
    {
        static List<string> _kernelModuleIds = new List<string>
        {
            "orbitalshell-kernel" , "orbitalshell-kernel-commands"
        };

        [Command("output a report of loaded modules if no option is specified, else allows to load/unload/install/remove/update modules and get informations from repositories of modules")]
        [CommandNamespace(CommandNamespace.shell, CommandNamespace.module)]
        [CommandAlias("mod", "module")]
        [CommandAlias("mods", "module -s")]
        public CommandResult<List<ModuleSpecification>> Module(
            CommandEvaluationContext context,
            [Option("l", "load", "load a module from the given path", true, true)] FilePath loadModulePath = null,
            [Option("n", "unload", "unload the module having the given name ", true, true)] string unloadModuleName = null,
            [Option("i", "install", "install a module from the nuget source", true, true)] string installModuleName = null,
            [Option("r", "remove", "uninstall a module. module files are still available in $modules", true, true)] string uninstallModuleName = null,
            [Option("u", "update", "try to update an installed module from the nuget source", true, true)] string updateModuleName = null,
            [Option(null, "check-only", "do not install update, just check if an update exists", true)] bool checkOnly = false,
            [Option(null,"update-all", "try to update all installed modules from the nuget source")] bool updateAll = false,
            [Option(null, "register-update-only", "do not add new entries to module-init, simply update existing entries")] bool registerUpdateOnly = false,
            [Option("f", "fetch-list", "fetch list of modules from modules repositories", true)] bool fetchList = false,
            [Option("o", "fetch-info", "query modules repositories about a module name, if found fetch the module info and output results. the module is not installed", true, true)] string fetchInfoName = null,
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
            string n;

            if (loadModulePath == null && unloadModuleName == null && updateModuleName == null 
                && installModuleName == null && uninstallModuleName == null && !updateAll
                && !fetchList && !fetchInfo)
            {
                // output reports on loaded modules

                var col1length = context.CommandLineProcessor.ModuleManager.Modules.Values.Select(x => x.Name.Length).Max() + 1;
                int i = 1;
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
                    var deps = (!deps_attr.Any()) ? "" : string.Join(",", deps_attr.Select(x => x.ModuleName + " " + x.ModuleMinVersion));
                    var sminv = kvp.Value.Assembly.GetCustomAttribute<ModuleShellMinVersionAttribute>()?.ShellMinVersion;

                    o.Echoln($"{Darkcyan}{kvp.Value.Key.PadRight(col1length, ' ')}{f}{kvp.Value.Description}");

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
                    if (i < context.CommandLineProcessor.ModuleManager.Modules.Count) o.Echoln();
                    i++;
                }
                return new CommandResult<List<ModuleSpecification>>(context.CommandLineProcessor.ModuleManager.Modules.Values.ToList());
            }

            static void _checkIsNotAKernelModule(string n) {
                if (_kernelModuleIds.Contains(n.ToLower()))
                    throw new Exception($"the kernel module '{n}' can't be handled by the module command. Please refers to kernel update documentation");
            }

            if (!fetchList && !fetchInfo)
            {
                // update all

                if (updateAll)
                {
                    var ids = ModuleUtil.GetInstalledModulesLowerPackageId(context);
                    if (ids.Count == 0) 
                        o.Echoln("nothing to update");
                    else
                        foreach (var id in ids)
                            Module(context: context, updateModuleName: id, checkOnly: checkOnly, registerUpdateOnly:true);
                }

                // update module

                if (updateModuleName!=null)
                {
                    n = updateModuleName;
                    if (ModuleUtil.IsModuleInstalled(context, n))
                    {
                        _checkIsNotAKernelModule(n);

                        // find the right module assembly

                        (AssemblyLoadContext assemblyLoadContext, Assembly moduleAssembly) = ModuleUtil.GetModuleAssembly(context, n);

                        // get the nupkg style version number
                        var curPackVer = moduleAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                        if (string.IsNullOrWhiteSpace(curPackVer))
                            return _ModuleErr(context, $"module assembly file version attribute missing/empty/null");

                        var getVersMethod = typeof(NuGetServerApiCommands).GetMethod("NugetVer");
                        var r = context.CommandLineProcessor.Eval(context, getVersMethod, $"{n} -q", 0);
                        if (r.EvalResultCode == (int)ReturnCode.OK)
                        {
                            var vers = ((PackageVersions)r.Result).Versions;
                            var lastPackageIndex = vers.Length-1;
                            var curPackageIndex = Array.IndexOf(vers, curPackVer);
                            if (lastPackageIndex > curPackageIndex)
                            {
                                // new version
                                var lastVer = vers.Last();
                                o.Echoln($"{context.ShellEnv.Colors.Highlight}a new version of '{n} {curPackVer}' is available: {lastVer}");

                                if (!checkOnly)
                                {
                                    // module -i {modulePackageId} --force --skip-load
                                    var installUpdateRes = Module(context: context, installModuleName: n, force: true, skipLoad: true, registerUpdateOnly:registerUpdateOnly);
                                    if (installUpdateRes.ReturnCode != (int)ReturnCode.OK)
                                        return _ModuleErr(context, $"module update failed due to error: {installUpdateRes.ExecErrorText}");

                                    o.Echoln($"module '{n}' has been updated from version '{curPackVer}' to version '{lastVer}'");
                                }
                            }
                            else
                                o.Echoln($"{n} ({curPackVer}) : no new version available");
                        }
                        else
                            return _ModuleErr(context, $"module id '{n}' not found at NuGet");
                    } else 
                        return _ModuleErr(context, $"module '{n}' is not installed");
                }

                // uninstall module

                if (uninstallModuleName!=null)
                {
                    n = uninstallModuleName;
                    var folderName = n.ToLower();

                    _checkIsNotAKernelModule(n);

                    if (_kernelModuleIds.Contains(n)) _checkIsNotAKernelModule(n); 

                    if (!ModuleUtil.IsModuleInstalled( context, folderName ))
                        // error not installed
                        return _ModuleErr(context, $"module '{n}' is not installed");

                    o.Echoln(clog + "removing potentially registered dlls:");
                    var modInit = ModuleUtil.LoadModuleInitConfiguration(context);
                    var modInits = modInit.List.ToList();
                    foreach ( var moduleAssemblyFilePath in ModuleUtil.GetModuleAssemblies(context,folderName) )
                    {
                        o.Echo(clog + moduleAssemblyFilePath.FullName + " ... ");
                        var removableItems = modInits.Where(x => x.Path == FileSystemPath.UnescapePathSeparators(moduleAssemblyFilePath.FullName));
                        if (removableItems.Any())
                        {
                            removableItems.ToList().ForEach(x => modInits.Remove(x));
                            o.Echoln("removed");
                        }
                        else
                            o.Echoln(context.ShellEnv.Colors.Error + "not found");
                    }
                    modInit.List = modInits.ToArray();

                    ModuleUtil.SaveModuleInitConfiguration(context, modInit);
                }

                // install module

                if (installModuleName != null)
                {
                    n = installModuleName;

                    _checkIsNotAKernelModule(n);

                    var lastVer = string.IsNullOrWhiteSpace(version);

                    #region fetch module info

                    var queryMethod = typeof(NuGetServerApiCommands).GetMethod("NugetQuery");
                    var r0 = context.CommandLineProcessor.Eval(context, queryMethod, $"{n} -t 1",0);
                    if (r0.EvalResultCode!=(int)ReturnCode.OK) return _ModuleErr(context, r0.ErrorReason);
                    var queryRes = r0.Result as QueryResultRoot;
                    if (queryRes==null) return _ModuleErr(context, "nuget query return a null result");
                    if (queryRes.Data.Length==0) return _ModuleErr(context, "module id unknown");
                    
                    #endregion

                    var packageId = queryRes.Data[0].Id;
                    o.Echoln();

                    var getVersMethod = typeof(NuGetServerApiCommands).GetMethod("NugetVer");                    
                    var r = context.CommandLineProcessor.Eval(context, getVersMethod, $"{n} -q", 0);
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
                        var output = CommandLineProcessorSettings.ModulesFolderPath;
                        var folderName = packageId.ToLower();
                        var lowerVersion = version.ToLower();
                        var moduleLowerFullId = $"{folderName}.{lowerVersion}";  // == module lower id

                        if (ModuleUtil.IsModuleInstalled(
                            folderName,version
                            ) && !force)
                            // error already installed, !force
                            return _ModuleErr(context,$"module '{moduleLowerFullId}' is already installed (may try --force)");

                        var moduleFolder = Path.Combine( output, folderName );
                        if (!Directory.Exists(moduleFolder)) Directory.CreateDirectory(moduleFolder);

                        moduleFolder = FileSystemPath.UnescapePathSeparators(moduleFolder);
                        var rd = context.CommandLineProcessor.Eval(context, dwnMethod, $"{n} {version} -o {moduleFolder}", 0);
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

                                    // must load assembly in another app domain (not .core) to avoid conflict with any loaded dll ==> use a new assembly load context
                                    var alc = new AssemblyLoadContext($"module assembly load context",true);
                                    var assembly = alc.LoadFromAssemblyPath(dll.FullName);

                                    if (ModuleUtil.IsAssemblyShellModule(assembly))
                                    {
                                        try
                                        {
                                            if (!skipLoad)
                                                context.CommandLineProcessor.ModuleManager.RegisterModule(context, assembly);

                                            o.Echoln(clog + (registerUpdateOnly? "update module-init" : "register into module-init") );
                                            var modInit = ModuleUtil.LoadModuleInitConfiguration(context);

                                            // remove others versions of the same module assembly
                                            bool oldVersionExists = false;
                                            var exMods = modInit.List.Where(x => x.LowerPackageId == folderName);
                                            foreach (var exMA in exMods)
                                            {
                                                var lst = modInit.List.ToList();
                                                lst.Remove(exMA);
                                                oldVersionExists = true;
                                                modInit.List = lst.ToArray();
                                            }

                                            var mod = new ModuleInitItem()
                                            {
                                                Path = FileSystemPath.UnescapePathSeparators(dll.FullName),
                                                LowerPackageId = folderName,
                                                LowerVersionId = lowerVersion,
                                                IsEnabled = true
                                            };
                                            
                                            if (!registerUpdateOnly || oldVersionExists)
                                                modInit.List = modInit.List.Append(mod).ToArray();

                                            ModuleUtil.SaveModuleInitConfiguration(context, modInit);
                                            nbImport++;

                                            o.Warningln("shell may needs restart to load module assemblies");
                                        }
                                        catch (Exception ex) { o.Errorln(ex.Message); }
                                    }

                                    // destroy the alc
                                    alc.Unload();
                                    alc = null;
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

                // unload module (unregistered in session)

                if (unloadModuleName != null)
                {
                    n = unloadModuleName;

                    _checkIsNotAKernelModule(n);

                    if (context.CommandLineProcessor.ModuleManager.GetModuleByLowerPackageId(n.ToLower())!=null)
                    {
                        moduleSpecification = context.CommandLineProcessor.ModuleManager.UnregisterModule(context, n );
                        if (moduleSpecification != null) o.Echoln($"unloaded: {n} {moduleSpecification.Info?.GetDescriptor(context)}");
                    }
                    else
                        return _ModuleErr(context, $"module '{n}' is not registered");
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
                            var modRef = modRefs.Where(x => x.ModuleId == fetchInfoName).FirstOrDefault();
                            if (modRef != null)
                            {
                                // try to fetch the module : name[,version]->nuget
                                #region fetch module info

                                var queryMethod = typeof(NuGetServerApiCommands).GetMethod("NugetQuery");
                                var r0 = context.CommandLineProcessor.Eval(context, queryMethod, $"{modRef.ModuleId} -t 1", 0);
                                if (r0.EvalResultCode != (int)ReturnCode.OK) return _ModuleErr(context, r0.ErrorReason);
                                var queryRes = r0.Result as QueryResultRoot;
                                if (queryRes == null) return _ModuleErr(context, "nuget query return a null result");
                                if (queryRes.Data.Length == 0) return _ModuleErr(context, "module id unknown");

                                #endregion
                            }
                            else
                                o.Errorln($"no module having name '{fetchInfoName}' can be found in repositories");
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
                                tb.AddRow(modref.ModuleId, modref.Version, modref.Description);

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

        static bool _GetModuleListFromRepositories(CommandEvaluationContext context, out List<ModuleReference> modulesReferences)
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

                var modList = new ModuleList();
                foreach (var repo in repoList)
                {
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("GET"), repo))
                            {
                                var tsk = httpClient.SendAsync(request);
                                var result = tsk.Result;
                                if (result.IsSuccessStatusCode)
                                {
                                    try
                                    {
                                        var modListJson = result.Content.ReadAsStringAsync().Result;
                                        var modRefs = JsonConvert.DeserializeObject<ModuleList>(modListJson);
                                        modList.Merge(modRefs);
                                    } catch (Exception getModListError)
                                    {
                                        context.Errorln($"failed to handle module list from repo '{repo}' due to errror: {getModListError.Message}");
                                    }
                                }
                                else
                                    context.Out.Errorln($"fetch list fail from '{repo}' due to error: {result.ReasonPhrase}");
                            }
                        }
                    }
                    catch (Exception repoAccessError)
                    {
                        context.Out.Errorln($"fetch list fail from '{repo}' due to error: {repoAccessError.Message}");
                    }
                }

                modulesReferences = modList.Modules;
                context.Out.Echoln(context.ShellEnv.Colors.Log + "Done");
                return true;
            }
            return false;
        }

        [Obsolete]
        List<ModuleReference> _ParseModuleList(string repoModulesList)
        {
            var r = new List<ModuleReference>();
            var lines = repoModulesList.Split("\n");
            foreach (var s in lines)
            {
                var ts = s.Trim();
                if (!string.IsNullOrWhiteSpace(s) && !ts.StartsWith(";"))
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

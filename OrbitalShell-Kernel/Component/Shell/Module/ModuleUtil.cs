using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Newtonsoft.Json;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Lib.FileSystem;
using OrbitalShell.Lib;
using OrbitalShell.Lib.Data;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// module utilitary methods
    /// </summary>
    public static class ModuleUtil
    {
        /// <summary>
        /// format and returns command declaring type from given type class name
        /// </summary>
        /// <param name="type">a command declaring type</param>
        /// <returns>camel case name , ends 'Commands' removed if present</returns>
        public static string DeclaringTypeShortName(Type type)
        {
            var r = type.Name;
            var i = r.LastIndexOf("Commands");
            if (i > 0)
                r = r.Substring(0, i);
            return r;
        }

        /// <summary>
        /// get module-init config
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <returns>ModuleInitModel</returns>
        public static ModuleInitModel LoadModuleInitConfiguration(CommandEvaluationContext context)
            => JsonConvert.DeserializeObject<ModuleInitModel>(
                File.ReadAllText(
                    context
                        .CommandLineProcessor
                        .Settings
                        .ModulesInitFilePath
                    ));

        /// <summary>
        /// save module-init config
        /// </summary>
        /// <param name="context">command evaluation context</param>
        /// <param name="o">module-init model object</param>
        public static void SaveModuleInitConfiguration(CommandEvaluationContext context,ModuleInitModel o)
        {
            File.WriteAllText(
                context
                    .CommandLineProcessor
                    .Settings
                    .ModulesInitFilePath,
                JsonConvert.SerializeObject(o,Formatting.Indented));
        }

        public static bool IsAssemblyShellModule(Assembly o)
            => o.GetCustomAttribute<ShellModuleAttribute>() != null;


        public static bool IsModuleInstalled(
            CommandEvaluationContext context,
            string moduleId,
            string version)
        {
            var path = context.CommandLineProcessor.Settings.ModulesFolderPath;
            path = Path.Combine(path, moduleId);
            if (!Directory.Exists(path)) return false;
            path = Path.Combine(path, version);
            if (!Directory.Exists(path)) return false;
            return true;
        }

        public static bool IsModuleInstalled(
            CommandEvaluationContext context,
            string moduleId)
        {
            var path = context.CommandLineProcessor.Settings.ModulesFolderPath;
            path = Path.Combine(path, moduleId);
            if (!Directory.Exists(path)) return false;
            return true;
        }

        public static List<FilePath> GetModuleAssemblies(CommandEvaluationContext context,string moduleId)
        {
            var r = new List<FilePath>();
            var path = context.CommandLineProcessor.Settings.ModulesFolderPath;
            var lowerModuleId = moduleId?.ToLower() ?? throw new Exception("module id should not be empty or null");
            path = Path.Combine(path, lowerModuleId);
            if (!Directory.Exists(path)) throw new Exception($"no module folder found for module '{moduleId}' in '{FileSystemPath.UnescapePathSeparators(path)}'");

            var files = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories);
            r = files.Select(x => new FilePath(x)).ToList();
            return r;
        }

        public static (AssemblyLoadContext assemblyLoadContext,Assembly moduleAssembly) GetModuleAssembly(CommandEvaluationContext context,string moduleId)
        {
            var assys = GetModuleAssemblies(context, moduleId);
            
            var versions = new Dictionary<VersionNumber, (AssemblyLoadContext alc,Assembly asy)>();
            string ver;

            foreach (var assy in assys)
            {
                try
                {
                    if (context.CommandLineProcessor.ModuleManager.IsModuleAssemblyLoaded(assy.FullName))
                    {
                        var assembly = context.CommandLineProcessor.ModuleManager.GetLoadedModuleAssembly(assy.FullName);
                        if ( assembly.GetCustomAttribute<ShellModuleAttribute>() != null &&
                            (ver = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion) != null) 
                                versions.AddOrReplace( new VersionNumber(ver), (null,assembly) );
                    }
                    else
                    {
                        var alc = new AssemblyLoadContext($"module assembly load context for {assy.FullName}", true);
                        var assembly = alc.LoadFromAssemblyPath(assy.FullName);
                        if (assembly.GetCustomAttribute<ShellModuleAttribute>() != null &&
                            (ver = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion) != null) 
                                versions.AddOrReplace( new VersionNumber(ver), (alc,assembly) );
                        alc.Unload();
                    }
                } catch (Exception notLoadableError) {
                    /* ignore error (maybe dll buggy due to bad dependencies -> not loadable == not handlable) */
                    context.Warningln($"a module assembly could not be loaded: '{assy.PrintableFullName}' due to error: '{notLoadableError.Message}'");
                }
            }

            // sort by version, select the most recent
            var vlist = versions.Keys.ToList();
            vlist.Sort();
            var sver = versions[vlist.Last()];

            return sver;
        }
    }
}
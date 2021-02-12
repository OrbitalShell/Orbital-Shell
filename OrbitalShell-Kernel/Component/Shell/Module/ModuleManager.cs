using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Hook;
using System.Reflection;
using System.IO;
using OrbitalShell.Component.CommandLine.Parsing;
using System;

namespace OrbitalShell.Component.Shell.Module
{
    /// <summary>
    /// manage the shell modules
    /// </summary>
    public class ModuleManager
    {
        #region attributes

        readonly ModuleSet _modules = new ModuleSet();

        public IReadOnlyDictionary<string, ModuleSpecification> Modules => new ReadOnlyDictionary<string, ModuleSpecification>(_modules);

        readonly SyntaxAnalyser _syntaxAnalyzer = new SyntaxAnalyser();

        public readonly ModuleCommandManager ModuleCommandManager;

        public readonly ModuleHookManager ModuleHookManager;

        #endregion

        #region init

        public ModuleManager(SyntaxAnalyser syntaxAnalyser)
        {
            _syntaxAnalyzer = syntaxAnalyser;
            ModuleCommandManager = new ModuleCommandManager(_syntaxAnalyzer, _modules);
            ModuleHookManager = new ModuleHookManager(_modules);
        }

        #endregion

        public ModuleSpecification RegisterModule(
            CommandEvaluationContext context,
            string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return RegisterModule(context, assembly);
        }

        public ModuleSpecification UnregisterModule(
            CommandEvaluationContext context,
            string moduleName)
        {
            var moduleSpecification = GetModule(context, moduleName);
            if (moduleSpecification == null) return null;
            var r = ModuleCommandManager.UnregisterModuleCommands(context, moduleName);
            _modules.Remove(moduleSpecification.Key);
            return r;
        }

        public ModuleSpecification GetModule(
            CommandEvaluationContext context,
            string moduleName)
        {
            var moduleSpecification = _modules.Values.Where(x => x.Name == moduleName).FirstOrDefault();
            if (moduleSpecification != null) return moduleSpecification;
            return null;
        }

        public ModuleSpecification RegisterModule(
            CommandEvaluationContext context,
            Assembly assembly)
        {
            ModuleSpecification moduleSpecification;

            var moduleAttr = assembly.GetCustomAttribute<ShellModuleAttribute>();
            if (moduleAttr == null)
            {
                context.Errorln($"assembly is not a shell module: '{assembly.FullName}'");
                return ModuleSpecification.ModuleSpecificationNotDefined;
            }

            var id = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? 
                throw new Exception($"module id missing in assembly '{assembly.ManifestModule.Name}' ('AssemblyTitle' attribute missing)");
            var ver = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? 
                throw new Exception($"module version missing in assembly '{assembly.ManifestModule.Name}' ('AssemblyInformationalVersion' attribute missing)");
            var modKey = GetModuleLowerId(id,ver);

            if (_modules.ContainsKey(modKey))
            {
                context.Errorln($"module already registered: {modKey} (path={assembly.FullName})");
                return ModuleSpecification.ModuleSpecificationNotDefined;
            }

            var typesCount = 0;
            var comTotCount = 0;
            var hooksCount = 0;

            foreach (var type in assembly.GetTypes())
            {
                // register hooks

                var hookAttr = type.GetCustomAttribute<HooksAttribute>();
                if (hookAttr != null)
                {
                    // module,class owns hooks
                    foreach (var mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var hook = mi.GetCustomAttribute<HookAttribute>();
                        if (hook != null)
                        {
                            ModuleHookManager.RegisterHook(context, hook.HookName, mi);
                            hooksCount++;
                        }
                    }
                }

                // register commands

                var comsAttr = type.GetCustomAttribute<CommandsAttribute>();

                var comCount = 0;
                if (comsAttr != null && type.GetInterface(typeof(ICommandsDeclaringType).FullName) != null)
                    comCount = ModuleCommandManager.RegisterCommandClass(context, type, false);
                if (comCount > 0)
                    typesCount++;
                comTotCount += comCount;

            }

            // register module

            var descAttr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            var description = (descAttr != null) ? descAttr.Description : "";
            _modules.Add(
                modKey,
                moduleSpecification = new ModuleSpecification(
                    modKey,
                    Path.GetFileNameWithoutExtension(assembly.Location),
                    description,
                    assembly,
                    new ModuleInfo(
                        typesCount,
                        comTotCount,
                        hooksCount
                    )
                ));

            // run module hook init
            ModuleHookManager.InvokeHooks(
                context,
                Hooks.ModuleInit,
                (o) =>
                {
                    moduleSpecification.IsInitialized = true;
                }
            );

            return moduleSpecification;
        }

        /// <summary>
        /// get the normalized lower module id (for nuget and orbsh)
        /// </summary>
        /// <param name="id">module id (= module nuget package id = assembly manifest module name )</param>
        /// <param name="version">module version id (= module nuget package version)</param>
        public string GetModuleLowerId(string id,string version) => id.ToLower()+"."+version.ToLower();

        public bool IsModuleInstalled(string moduleLowerId) => _modules.ContainsKey(moduleLowerId);

        public bool IsModuleInstalled(string id, string version) => IsModuleInstalled(GetModuleLowerId(id, version));
        
    }
}
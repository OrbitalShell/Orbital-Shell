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
    public class ModuleManager : IModuleManager
    {
        #region attributes

        readonly IModuleSet _modules;

        public IReadOnlyDictionary<string, ModuleSpecification> Modules => new ReadOnlyDictionary<string, ModuleSpecification>(_modules);

        private readonly List<string> _loadedModules = new List<string>();

        private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:remove not read private member", Justification = "<Wait future impl.>")]
        readonly ISyntaxAnalyser _syntaxAnalyzer = new SyntaxAnalyser();

        public IModuleCommandManager ModuleCommandManager { get; protected set; }

        public IHookManager ModuleHookManager { get; protected set; }

        #endregion

        #region init

        public ModuleManager(            
            //ISyntaxAnalyser syntaxAnalyser,
            IModuleCommandManager modComManager,
            IHookManager modHookManager,
            IModuleSet moduleSet
            )
        {
            _modules = moduleSet;
            //_syntaxAnalyzer = syntaxAnalyser;
            ModuleCommandManager = modComManager; // new ModuleCommandManager(_syntaxAnalyzer, _modules);
            ModuleHookManager = modHookManager; // new ModuleHookManager(_modules);
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
            moduleName = moduleName?.ToLower() ?? throw new ArgumentNullException(nameof(moduleName));
            var moduleSpecification = GetModuleByLowerPackageId(moduleName);
            if (moduleSpecification == null) return null;
            var r = ModuleCommandManager.UnregisterModuleCommands(context, moduleName);
            _modules.Remove(moduleSpecification.Key);
            return r;
        }

        public ModuleSpecification GetModuleByName(
            string moduleName)
        {
            var moduleSpecification = _modules.Values.Where(x => x.Name == moduleName).FirstOrDefault();
            if (moduleSpecification != null) return moduleSpecification;
            return null;
        }

        public ModuleSpecification GetModuleByLowerPackageId(
            string lowerPackareId)
        {
            lowerPackareId = lowerPackareId?.ToLower() ?? throw new ArgumentNullException(nameof(lowerPackareId));
            var moduleSpecification = _modules.Values.Where(x => x.Key == lowerPackareId).FirstOrDefault();
            if (moduleSpecification != null) return moduleSpecification;
            return null;
        }

        static string AssemblyKey(Assembly assembly) => assembly.Location + "." + assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        private static string ModuleKey(Assembly assembly, out string id, out string ver)
        {
            id = assembly.GetCustomAttribute<ShellModuleAttribute>()?.PackageId ??
                throw new Exception($"module package id missing or null in assembly '{assembly.ManifestModule.Name}' ('ShellModule' attribute missing or has null value)");
            ver = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
                throw new Exception($"module version missing in assembly '{assembly.ManifestModule.Name}' ('AssemblyInformationalVersion' attribute missing)");
            return id.ToLower();
        }

        public ModuleSpecification RegisterModule(
            CommandEvaluationContext context,
            Assembly assembly)
        {
            ModuleSpecification moduleSpecification = null;

            try
            {
                var moduleAttr = assembly.GetCustomAttribute<ShellModuleAttribute>();
                if (moduleAttr == null)
                {
                    context.Errorln($"assembly is not a shell module: '{assembly.FullName}'");
                    return ModuleSpecification.ModuleSpecificationNotDefined;
                }

                // id is the name of the assembly (/!\ should not fit nuget packet id)

                var modKey = ModuleKey(assembly, out var id, out var ver);
                var assKey = AssemblyKey(assembly);
                if (_loadedModules.Contains(assKey)) throw new Exception($"assembly already loaded: '{assKey}'");

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
                _loadedModules.Add(AssemblyKey(assembly));
                _loadedAssemblies.Add(assembly.Location.ToLower(), assembly);

                // run module hook init
                ModuleHookManager.InvokeHooks(
                    context,
                    Hooks.ModuleInit,
                    HookTriggerMode.FirstTimeOnly,                    
                    (o) =>
                    {
                        moduleSpecification.IsInitialized = true;
                    }
                );

            }
            catch (Exception ex)
            {
                throw new Exception($"register module assembly '{assembly.FullName}' failed due to error: '{ex.Message}'", ex);
            }

            return moduleSpecification;
        }

        public bool IsModuleAssemblyLoaded(string path) => _loadedAssemblies.ContainsKey(path.ToLower());

        public Assembly GetLoadedModuleAssembly(string path) => _loadedAssemblies[path.ToLower()];
    }
}
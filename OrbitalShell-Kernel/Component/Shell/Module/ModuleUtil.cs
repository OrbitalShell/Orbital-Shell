using System;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using OrbitalShell.Component.CommandLine.Processor;

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
    }
}
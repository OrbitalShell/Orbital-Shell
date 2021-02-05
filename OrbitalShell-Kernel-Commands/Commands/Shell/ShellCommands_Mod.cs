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
            [Option("s", "short", "output less informations", true)] bool @short = false
            )
        {
            var f = context.ShellEnv.Colors.Default.ToString();
            if (loadModulePath == null && unloadModuleName == null)
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

                    context.Out.Echoln($"{Darkcyan}{kvp.Value.Name.PadRight(col1length, ' ')}{f}{kvp.Value.Description}");

                    if (!@short)
                    {
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}assembly: {context.ShellEnv.Colors.HalfDark}{kvp.Value.Assembly.FullName}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}path    : {context.ShellEnv.Colors.HalfDark}{kvp.Value.Assembly.Location}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}version : {context.ShellEnv.Colors.HalfDark}{ver}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}date    : {context.ShellEnv.Colors.HalfDark}{dat}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}company : {context.ShellEnv.Colors.HalfDark}{comp}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}authors : {context.ShellEnv.Colors.HalfDark}{aut}");
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{kvp.Value.Info.GetDescriptor(context)}");
                    }
                    else
                    {
                        context.Out.Echoln($"{"".PadRight(col1length, ' ')}{context.ShellEnv.Colors.Label}version : {context.ShellEnv.Colors.HalfDark}{ver} ({dat}) company={comp} authors={aut}");
                    }
                    if (n < context.CommandLineProcessor.ModuleManager.Modules.Count) context.Out.Echoln();
                    n++;
                }
                return new CommandResult<List<ModuleSpecification>>(context.CommandLineProcessor.ModuleManager.Modules.Values.ToList());
            }
            if (loadModulePath != null)
            {
                if (loadModulePath.CheckExists(context))
                {
                    var a = Assembly.LoadFrom(loadModulePath.FileSystemInfo.FullName);
                    var moduleSpecification = context.CommandLineProcessor.ModuleManager.RegisterModule(context, a);
                    context.Out.Echoln($"loaded: {moduleSpecification.Info.GetDescriptor(context)}");
                }
                else
                    return new CommandResult<List<ModuleSpecification>>(ReturnCode.Error);
            }

            if (unloadModuleName != null)
            {
                if (context.CommandLineProcessor.ModuleManager.Modules.Values.Any(x => x.Name == unloadModuleName))
                {
                    var moduleSpecification = context.CommandLineProcessor.ModuleManager.UnregisterModule(context, unloadModuleName);
                    context.Out.Echoln($"unloaded: {moduleSpecification.Info.GetDescriptor(context)}");
                }
                else
                {
                    context.Errorln($"module '{unloadModuleName}' is not registered");
                    return new CommandResult<List<ModuleSpecification>>(ReturnCode.Error);
                }
            }
            return new CommandResult<List<ModuleSpecification>>();
        }

    }
}

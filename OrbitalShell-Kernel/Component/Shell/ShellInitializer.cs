using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using cons = System.Console;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Lib.FileSystem;
using OrbitalShell.Component.Shell.Module;
using System.Linq;
using OrbitalShell.Component.Console;
using OrbitalShell.Component.Shell.Module.Data;

namespace OrbitalShell.Component.Shell
{
    public class ShellInitializer
    {
        readonly ICommandLineProcessor _clp;
        readonly IConsole _console;

        public ShellInitializer(ICommandLineProcessor clp)
        {
            _clp = clp;
            _console = clp.Console;
        }


        /// <summary>
        /// perform kernel inits and run init scripts
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:remove the parameter not used", Justification = "maybe in future change - waiting impl.")]
        public void Run(
            CommandEvaluationContext context
            )
        {
            if (_clp.IsInitialized) return;

            ShellInit( _clp.Args, _console, _clp.Settings, _clp.CommandEvaluationContext);

            // late init of settings from the context
            _clp.Settings.Initialize(_clp.CommandEvaluationContext);

            // run user profile
            try
            {
                _clp.CommandBatchProcessor.RunBatch(
                    _clp.CommandEvaluationContext, 
                    _clp.Settings.UserProfileFilePath);
            }
            catch (Exception ex)
            {
                _console.Warning($"Run 'user profile file' skipped. Reason is : {ex.Message}");
            }

            // run user aliases
            try
            {
                _clp.CommandsAlias.Init(
                    _clp.CommandEvaluationContext, 
                    _clp.Settings.AppDataRoamingUserFolderPath, 
                    _clp.Settings.CommandsAliasFileName);
            }
            catch (Exception ex)
            {
                _console.Warning($"Run 'user aliases' skipped. Reason is : {ex.Message}");
            }

            _clp.ModuleManager.ModuleHookManager.InvokeHooks(
                _clp.CommandEvaluationContext,
                Hooks.ShellInitialized,
                HookTriggerMode.FirstTimeOnly
                );

            _clp.IsInitialized = true;
        }

        /// <summary>
        /// shell init actions sequence<br/>
        /// use system in,out,err TODO: plugable to any stream - just add parameters
        /// </summary>
        /// <param name="args">orbsh args</param>
        /// <param name="settings">(launch) settings object</param>
        /// <param name="context">shell default command evaluation context.Provides null to build a new one</param>
        public void ShellInit(
            string[] args,
            IConsole console,
            ICommandLineProcessorSettings settings,
            CommandEvaluationContext context = null
            )
        {
            _clp.Init(args,settings,context);

            // get final clp command evaluation context
            context = _clp.CommandEvaluationContext;

            context.Logger.MuteLogErrors = true;
            _clp.Settings.Initialize(context);

            // pre console init
            if (console.DefaultForeground != null) cons.ForegroundColor = console.DefaultForeground.Value;

            // apply orbsh command args -env:{varName}={varValue}
            var appliedSettings = new List<string>();
            _clp.SetArgs(args, _clp.CommandEvaluationContext, appliedSettings);

            // init from settings
            ShellInitFromSettings(_clp,settings);

            ConsoleInit(_clp.CommandEvaluationContext);

            // check shell app data folder
            if (!Directory.Exists(_clp.Settings.ShellAppDataPath))
                Directory.CreateDirectory(_clp.Settings.ShellAppDataPath);

            // clp info output
            if (settings.PrintInfo) _clp.PrintInfo(_clp.CommandEvaluationContext);

            #region -- load kernel modules --

            var a = Assembly.GetExecutingAssembly();
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading kernel module: '{a}' ... ", true, false);
            var moduleSpecification = _clp.ModuleManager.RegisterModule(_clp.CommandEvaluationContext, a);
            context.Logger.Done(moduleSpecification.Info.GetDescriptor(context));

            a = Assembly.Load(settings.KernelCommandsModuleAssemblyName);
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading kernel commands module: '{a}' ... ", true, false);
            moduleSpecification = _clp.ModuleManager.RegisterModule(_clp.CommandEvaluationContext, a);
            context.Logger.Done(moduleSpecification.Info.GetDescriptor(context));

            #endregion

            _clp.CommandEvaluationContext.Logger.MuteLogErrors = false;

            #region -- load modules ---

            var mpath = _clp.Settings.ModulesInitFilePath;
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading modules: '{FileSystemPath.UnescapePathSeparators(mpath)}' ... ", true, false);
            
            ModulesInit(context,mpath);
            context.Logger.Done("modules loaded");

            #endregion

            #region init from user profile

            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"init user profile from: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.AppDataRoamingUserFolderPath)}' ... ", true, false);

            InitUserProfileFolder();

            _clp.PostInit();

            context.Logger.Done();
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"restoring user history file: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.HistoryFilePath)}' ... ", true, false);

            CreateRestoreUserHistoryFile();

            context.Logger.Done();
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading user aliases: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.CommandsAliasFilePath)}' ... ", true, false);

            CreateRestoreUserAliasesFile();

            context.Logger.Done();

            #endregion

            if (appliedSettings.Count > 0) context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"shell args: {string.Join(" ", appliedSettings)}");

            // end inits
            console.Out.Echoln();

            _clp.PostInit();
        }

        void ModulesInit(CommandEvaluationContext context,string moduleInitFilePath)
        {
            if (!File.Exists(moduleInitFilePath))
            {
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating shell module init file: '{FileSystemPath.UnescapePathSeparators(moduleInitFilePath)}' ... ", true, false);
                try
                {
                    File.Copy(
                        Path.Combine(CommandLineProcessorSettings.BinFolderPath, "Component", "Shell", "Module", _clp.Settings.ModulesInitFileName),
                        _clp.Settings.ModulesInitFilePath
                    );
                    _clp.CommandEvaluationContext.Logger.Success("",true,false);                    
                }
                catch (Exception createModuleInitFilePathException)
                {
                    _clp.CommandEvaluationContext.Logger.Fail(createModuleInitFilePathException);
                }
            }

            try
            {
                LoadModulesFromConfig(context);
            } catch (Exception loadModuleException)
            {
                _clp.CommandEvaluationContext.Logger.Fail(loadModuleException);
            }
        }

        void LoadModulesFromConfig(CommandEvaluationContext context)
        {
            var mods = ModuleUtil.LoadModuleInitConfiguration(context);

            if (mods==null)
            {
                // rebuild the module-init file - it is crashed
                mods = new ModuleInit()
                    {
                        ReadMe = $"new file generated on {System.DateTime.Now}",
                        List = Array.Empty<ModuleInitItem>()
                };
                ModuleUtil.SaveModuleInitConfiguration(context, mods);
                context.Errorln("a crashed version of module-init has been restored to initial state");
            }
                
            var enabledMods = mods.List.Where(x => x.IsEnabled);
            if (!enabledMods.Any()) return;
            var o = context.Out;
            o.Echoln();
            foreach ( var mod in enabledMods)
            {
                try
                {
                    context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading module: '{mod.Path}' ... ", true, false);
                    var a = Assembly.LoadFile(mod.Path);
                    context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"module assembly loaded: '{a}'. registering module ... ", true, false);
                    var modSpec = _clp.ModuleManager.RegisterModule(_clp.CommandEvaluationContext, a);
                    context.Logger.Done(modSpec.Info.GetDescriptor(context));
                } catch (Exception loadModException)
                {
                    _clp.CommandEvaluationContext.Logger.Fail(loadModException);
                }
            }
        }

        void ShellInitFromSettings(
            ICommandLineProcessor clp,
            ICommandLineProcessorSettings settings
            )
        {
            var ctx = clp.CommandEvaluationContext;
            _console.Out.EnableAvoidEndOfLineFilledWithBackgroundColor = ctx.ShellEnv.GetValue<bool>(ShellEnvironmentVar.settings_console_enableAvoidEndOfLineFilledWithBackgroundColor);
            var prompt = ctx.ShellEnv.GetValue<string>(ShellEnvironmentVar.settings_console_prompt);
            clp.CommandLineReader.SetDefaultPrompt(prompt);

            var pathexts = ctx.ShellEnv.GetValue<List<string>>(ShellEnvironmentVar.pathExt);
            var pathextinit = ctx.ShellEnv.GetDataValue(ShellEnvironmentVar.pathExtInit);
            var pathextinittext = (string)pathextinit.Value;
            if (!string.IsNullOrWhiteSpace(pathextinittext)) pathextinittext += ShellEnvironment.SystemPathSeparator;
            pathextinittext += settings.PathExtInit.Replace(";", ShellEnvironment.SystemPathSeparator);
            pathextinit.SetValue(pathextinittext);
            var exts = pathextinittext.Split(ShellEnvironment.SystemPathSeparator);
            foreach (var ext in exts)
                pathexts.AddUnique(ext);

            ctx.ShellEnv.SetValue(ShellEnvironmentVar.settings_clp_shellExecBatchExt, settings.ShellExecBatchExt);
        }


        /// <summary>
        /// init the console. these init occurs before any display
        /// </summary>
        static void ConsoleInit(CommandEvaluationContext context)
        {
            var oWinWidth = context.ShellEnv.GetDataValue(ShellEnvironmentVar.settings_console_initialWindowWidth);
            var oWinHeight = context.ShellEnv.GetDataValue(ShellEnvironmentVar.settings_console_initialWindowHeight);

            if (context.ShellEnv.IsOptionSetted(ShellEnvironmentVar.settings_console_enableCompatibilityMode))
            {
                oWinWidth.SetValue(2000);
                oWinHeight.SetValue(2000);
            }

            var WinWidth = (int)oWinWidth.Value;
            var winHeight = (int)oWinHeight.Value;
            try
            {
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                if (WinWidth > -1) System.Console.WindowWidth = WinWidth;
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                if (winHeight > -1) System.Console.WindowHeight = winHeight;
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
                System.Console.Clear();
            }
            catch { }
        }

        public void CreateRestoreUserAliasesFile()
        {
            // create/restore user aliases
            var createNewCommandsAliasFile = !File.Exists(_clp.Settings.CommandsAliasFilePath);
            if (createNewCommandsAliasFile)
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user commands aliases file: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.CommandsAliasFilePath)}' ... ", true, false);
            try
            {
                if (createNewCommandsAliasFile)
                {
                    var defaultAliasFilePath = Path.Combine(_clp.Settings.DefaultsFolderPath, _clp.Settings.DefaultCommandsAliasFileName);
                    File.Copy(defaultAliasFilePath, _clp.Settings.CommandsAliasFilePath);
                    
                    _clp.CommandEvaluationContext.Logger.Success();
                }
            }
            catch (Exception createUserProfileFileException)
            {
                _clp.CommandEvaluationContext.Logger.Fail(createUserProfileFileException);
            }
        }

        public void CreateRestoreUserHistoryFile()
        {
            // create/restore commands history
            _clp.CmdsHistory = new CommandsHistory();
            var createNewHistoryFile = !File.Exists(_clp.Settings.HistoryFilePath);
            if (createNewHistoryFile)
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user commands history file: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.HistoryFilePath)}' ... ", true, false);
            try
            {
                if (createNewHistoryFile)
#pragma warning disable CS0642 // Possibilité d'instruction vide erronée
                    using (var fs = File.Create(_clp.Settings.HistoryFilePath)) ;
#pragma warning restore CS0642 // Possibilité d'instruction vide erronée
                _clp.CmdsHistory.Init(_clp.Settings.AppDataRoamingUserFolderPath, _clp.Settings.HistoryFileName);
                if (createNewHistoryFile) _clp.CommandEvaluationContext.Logger.Success();
            }
            catch (Exception createUserProfileFileException)
            {
                _clp.CommandEvaluationContext.Logger.Fail(createUserProfileFileException);
            }
        }

        public void InitUserProfileFolder()
        {
            // creates user app data folders
            if (!Directory.Exists(_clp.Settings.AppDataRoamingUserFolderPath))
            {
                _clp.Settings.LogAppendAllLinesErrorIsEnabled = false;
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user shell folder: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.AppDataRoamingUserFolderPath)}' ... ", true, false);
                try
                {
                    Directory.CreateDirectory(_clp.Settings.AppDataRoamingUserFolderPath);
                    _clp.CommandEvaluationContext.Logger.Success();
                }
                catch (Exception createAppDataFolderPathException)
                {
                    _clp.CommandEvaluationContext.Logger.Fail(createAppDataFolderPathException);
                }
            }

            // initialize log file
            if (!File.Exists(_clp.Settings.LogFilePath))
            {
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating log file: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.LogFilePath)}' ... ", true, false);
                try
                {
                    var logError = _clp.CommandEvaluationContext.Logger.Log($"file created on {System.DateTime.Now}");
                    if (logError == null)
                        _clp.CommandEvaluationContext.Logger.Success();
                    else
                        throw logError;
                }
                catch (Exception createLogFileException)
                {
                    _clp.Settings.LogAppendAllLinesErrorIsEnabled = false;
                    _clp.CommandEvaluationContext.Logger.Fail(createLogFileException);
                }
            }

            // initialize user profile
            if (!File.Exists(_clp.Settings.UserProfileFilePath))
            {
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user profile file: '{FileSystemPath.UnescapePathSeparators(_clp.Settings.UserProfileFilePath)}' ... ", true, false);
                try
                {
                    var defaultProfileFilePath = Path.Combine(_clp.Settings.DefaultsFolderPath, _clp.Settings.DefaultUserProfileFileName);
                    File.Copy(defaultProfileFilePath, _clp.Settings.UserProfileFilePath);
                    _clp.CommandEvaluationContext.Logger.Success();
                }
                catch (Exception createUserProfileFileException)
                {
                    _clp.CommandEvaluationContext.Logger.Fail(createUserProfileFileException);
                }
            }
        }

    }

}

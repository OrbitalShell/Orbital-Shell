using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static OrbitalShell.DotNetConsole;
using cons = System.Console;
using OrbitalShell.Component.Shell.Hook;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell
{
    public class ShellInitializer
    {
        CommandLineProcessor _clp;        

        public ShellInitializer(CommandLineProcessor clp)
        {
            _clp = clp;
        }

        /// <summary>
        /// perform kernel inits and run init scripts
        /// </summary>
        public void Run(
            CommandEvaluationContext context
            )
        {
            if (_clp.IsInitialized) return;

            //clp.ShellInit(_args, _settings, _commandEvaluationContext);
            ShellInit( _clp.Args, _clp.Settings, _clp.CommandEvaluationContext);

            // late init of settings from the context
            //_settings.Initialize(CommandEvaluationContext);
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
                Warning($"Run 'user profile file' skipped. Reason is : {ex.Message}");
            }

            // run user aliases
            try
            {
                _clp.CommandsAlias.Init(
                    _clp.CommandEvaluationContext, 
                    _clp.Settings.AppDataFolderPath, 
                    _clp.Settings.CommandsAliasFileName);
            }
            catch (Exception ex)
            {
                Warning($"Run 'user aliases' skipped. Reason is : {ex.Message}");
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
            CommandLineProcessorSettings settings,
            CommandEvaluationContext context = null
            )
        {
            _clp.Init(args,settings,context);

            // get final clp command evaluation context
            context = _clp.CommandEvaluationContext;

            context.Logger.MuteLogErrors = true;
            _clp.Settings.Initialize(context);

            // pre console init
            if (DefaultForeground != null) cons.ForegroundColor = DefaultForeground.Value;

            // apply orbsh command args -env:{varName}={varValue}
            var appliedSettings = new List<string>();
            _clp.SetArgs(args, _clp.CommandEvaluationContext, appliedSettings);

            // init from settings
            ShellInitFromSettings(_clp,settings);

            ConsoleInit(_clp.CommandEvaluationContext);

            if (settings.PrintInfo) _clp.PrintInfo(_clp.CommandEvaluationContext);

            // load kernel modules

            var a = Assembly.GetExecutingAssembly();
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading kernel module: '{a}' ... ", true, false);
            var moduleSpecification = _clp.ModuleManager.RegisterModule(_clp.CommandEvaluationContext, a);
            context.Logger.Done(moduleSpecification.Info.GetDescriptor(context));

            // TODO: can't reference by type an external module for which we have not a project reference
            a = Assembly.Load(settings.KernelCommandsModuleAssemblyName);
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading kernel commands module: '{a}' ... ", true, false);
            moduleSpecification = _clp.ModuleManager.RegisterModule(_clp.CommandEvaluationContext, a);
            context.Logger.Done(moduleSpecification.Info.GetDescriptor(context));

            var lbr = false;

            _clp.CommandEvaluationContext.Logger.MuteLogErrors = false;

            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"init user profile from: '{_clp.Settings.AppDataFolderPath}' ... ", true, false);

            lbr = InitUserProfileFolder(lbr);

            _clp.PostInit();

            context.Logger.Done();
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"restoring user history file: '{_clp.Settings.HistoryFilePath}' ... ", true, false);

            lbr |= CreateRestoreUserHistoryFile(lbr);

            context.Logger.Done();
            context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"loading user aliases: '{_clp.Settings.CommandsAliasFilePath}' ... ", true, false);

            lbr |= CreateRestoreUserAliasesFile(lbr);

            context.Logger.Done();
            if (appliedSettings.Count > 0) context.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"shell args: {string.Join(" ", appliedSettings)}");

            // end inits
            if (lbr) Out.Echoln();

            Out.Echoln();

            _clp.PostInit();
        }

        void ShellInitFromSettings(
            CommandLineProcessor clp,
            CommandLineProcessorSettings settings
            )
        {
            var ctx = clp.CommandEvaluationContext;
            Out.EnableAvoidEndOfLineFilledWithBackgroundColor = ctx.ShellEnv.GetValue<bool>(ShellEnvironmentVar.settings_console_enableAvoidEndOfLineFilledWithBackgroundColor);
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
        void ConsoleInit(CommandEvaluationContext context)
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
                if (WinWidth > -1) System.Console.WindowWidth = WinWidth;
                if (winHeight > -1) System.Console.WindowHeight = winHeight;
                System.Console.Clear();
            }
            catch { }
        }

        public bool CreateRestoreUserAliasesFile(bool lbr)
        {
            // create/restore user aliases
            var createNewCommandsAliasFile = !File.Exists(_clp.Settings.CommandsAliasFilePath);
            if (createNewCommandsAliasFile)
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user commands aliases file: '{_clp.Settings.CommandsAliasFilePath}' ... ", true, false);
            try
            {
                if (createNewCommandsAliasFile)
                {
                    var defaultAliasFilePath = Path.Combine(_clp.Settings.DefaultsFolderPath, _clp.Settings.DefaultCommandsAliasFileName);
                    File.Copy(defaultAliasFilePath, _clp.Settings.CommandsAliasFilePath);
                    lbr |= true;
                    _clp.CommandEvaluationContext.Logger.Success();
                }
            }
            catch (Exception createUserProfileFileException)
            {
                _clp.CommandEvaluationContext.Logger.Fail(createUserProfileFileException);
            }
            return lbr;
        }

        public bool CreateRestoreUserHistoryFile(bool lbr)
        {
            // create/restore commands history
            _clp.CmdsHistory = new CommandsHistory();
            var createNewHistoryFile = !File.Exists(_clp.Settings.HistoryFilePath);
            if (createNewHistoryFile)
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user commands history file: '{_clp.Settings.HistoryFilePath}' ... ", true, false);
            try
            {
                if (createNewHistoryFile)
#pragma warning disable CS0642 // Possibilité d'instruction vide erronée
                    using (var fs = File.Create(_clp.Settings.HistoryFilePath)) ;
#pragma warning restore CS0642 // Possibilité d'instruction vide erronée
                _clp.CmdsHistory.Init(_clp.Settings.AppDataFolderPath, _clp.Settings.HistoryFileName);
                if (createNewHistoryFile) _clp.CommandEvaluationContext.Logger.Success();
            }
            catch (Exception createUserProfileFileException)
            {
                _clp.CommandEvaluationContext.Logger.Fail(createUserProfileFileException);
            }
            lbr |= createNewHistoryFile;
            return lbr;
        }

        public bool InitUserProfileFolder(bool lbr)
        {
            // assume the application folder ($Env.APPDATA/OrbitalShell) exists and is initialized

            // creates user app data folders
            if (!Directory.Exists(_clp.Settings.AppDataFolderPath))
            {
                _clp.Settings.LogAppendAllLinesErrorIsEnabled = false;
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user shell folder: '{_clp.Settings.AppDataFolderPath}' ... ", true, false);
                try
                {
                    Directory.CreateDirectory(_clp.Settings.AppDataFolderPath);
                    _clp.CommandEvaluationContext.Logger.Success();
                }
                catch (Exception createAppDataFolderPathException)
                {
                    _clp.CommandEvaluationContext.Logger.Fail(createAppDataFolderPathException);
                }
                lbr = true;
            }

            // initialize log file
            if (!File.Exists(_clp.Settings.LogFilePath))
            {
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating log file: '{_clp.Settings.LogFilePath}' ... ", true, false);
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
                lbr = true;
            }

            // initialize user profile
            if (!File.Exists(_clp.Settings.UserProfileFilePath))
            {
                _clp.CommandEvaluationContext.Logger.Info(_clp.CommandEvaluationContext.ShellEnv.Colors.Log + $"creating user profile file: '{_clp.Settings.UserProfileFilePath}' ... ", true, false);
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
                lbr = true;
            }

            return lbr;
        }

    }

}

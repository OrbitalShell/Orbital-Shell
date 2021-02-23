using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis.Scripting;

using OrbitalShell.Component.Console;
using OrbitalShell.Component.Script;

namespace OrbitalShell.Component.CommandLine.Processor
{
    /// <summary>
    /// orbital shell kernel settings
    /// </summary>
    public class CommandLineProcessorSettings
    {
        public CommandLineProcessorSettings()
        {
        }

        /// <summary>
        /// initialize contextual attributes and setup the context streams
        /// </summary>
        /// <param name="commandEvaluationContext">context</param>
        public void Initialize(CommandEvaluationContext commandEvaluationContext)
        {
            this.CommandEvaluationContext = commandEvaluationContext;

            // TODO: activate after DotNetConsole is no more static - see remarks below
            /*
            Out = new ConsoleTextWriterWrapper(System.Console.Out);   
            Err = new TextWriterWrapper(System.Console.Error);
            In = System.Console.In;
            */

            var scriptOptions = ScriptOptions.Default;
            var abl = Assembly.GetExecutingAssembly();
            scriptOptions = scriptOptions.WithReferences(abl);
            var cSharpScriptEngine = new CSharpScriptEngine(
                commandEvaluationContext.CommandLineProcessor.Console,
                scriptOptions);

            //Out = DotNetConsole.Out;
            Out = new ShellConsoleTextWriterWrapper(
                commandEvaluationContext,
                System.Console.Out,
                cSharpScriptEngine
            );

            /*  
                it can exists only one wrapper for out,
                between the command evaluation context and dot net console
            */


            commandEvaluationContext.CommandLineProcessor.Console.Out = Out;
            //DotNetConsole.Out = Out;

            Err = commandEvaluationContext.CommandLineProcessor.Console.Err;
            //Err = DotNetConsole.Err;
            In = commandEvaluationContext.CommandLineProcessor.Console.In;
            //In = DotNetConsole.In;

            commandEvaluationContext.SetStreams(Out, In, Err);
        }

        public CommandEvaluationContext CommandEvaluationContext { get; protected set; }

        #region streams

        public ConsoleTextWriterWrapper Out { get; protected set; }

        public TextWriterWrapper Err;

        public TextReader In;

        #endregion

        #region product info

        public string AppName = "CommandLineProcessor";
        public string AppLongName = "Orbital Shell";
        public string AppEditor = "(c) Orbital Shell 2020";
        public string AppVersion = "0.0.0";
        public string AppLicense = "MIT";

        #endregion

        #region official files names

        public string UserProfileFileName = ".profile";
        public string DefaultUserProfileFileName = "profile";
        public string LogFileName = "log";
        public string HistoryFileName = ".history";
        public string CommandsAliasFileName = ".aliases";
        public string DefaultCommandsAliasFileName = "aliases";
        public string DefaultsFolderName = "Defaults";
        public string ModulesInitFileName = "modules-init.json";

        #endregion

        #region official paths

        /// <summary>
        /// shell app data folder name (application settings)
        /// </summary>
        public string AppDataFolderName = "OrbitalShell";

        public string ShellAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppDataFolderName);

        public string AppDataRoamingUserFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName);
        public string UserProfileFilePath => Path.Combine(AppDataRoamingUserFolderPath, UserProfileFileName);
        public string LogFilePath => Path.Combine(AppDataRoamingUserFolderPath, LogFileName);
        public string HistoryFilePath => Path.Combine(AppDataRoamingUserFolderPath, HistoryFileName);
        public string CommandsAliasFilePath => Path.Combine(AppDataRoamingUserFolderPath, CommandsAliasFileName);

        public string ModulesInitFilePath => Path.Combine(ShellAppDataPath, ModulesInitFileName );

        public static string UserProfileFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string BinFolderPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ModulesFolderPath => Path.Combine(BinFolderPath, "Modules");

        public string DefaultsFolderPath
        {
            get
            {
                var segs = this.GetType().FullName.Replace("CommandLine", "Shell").Split('.');
                var splits = segs.AsSpan(1, segs.Length - 3);
                var path = BinFolderPath;
                foreach (var split in splits)
                    path = Path.Combine(path, split);
                return Path.Combine(path, DefaultsFolderName);
            }
        }

        #endregion

        #region settings    TODO: check real useless / move to good location

        public string PathExtInit = "sh;init;";

        public string ShellExecBatchExt = ".sh";

        public bool LogAppendAllLinesErrorIsEnabled = true;

        public bool PrintInfo = true;

        public char ErrorPositionMarker = '^';

        public string KernelCommandsModuleAssemblyName = "OrbitalShell-Kernel-Commands";

        public string KernelCommandsRootNamespace = "Commands";

        #endregion
    }
}
